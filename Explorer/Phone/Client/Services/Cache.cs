using System;
using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class CacheAgent : IMessageEndpoint
    {
        public const byte Cacheable = 
            (byte)(CacheHint.SessionStore_MayCache | CacheHint.SessionStore_ShouldCache | CacheHint.PersistStore_MayCache | CacheHint.PersistStore_ShouldCache | CacheHint.GuaranteedStore);
        
        /// <summary>
        /// Server-synchronised cache.
        /// </summary>
        public CacheStore Server { get; private set; }

        public CacheItemID? CacheID
        {
            get { return cacheID; }
        }

        public byte[] CacheHash
        {
            get { return Server.CalculateHash(); }
        }

        public Dictionary<string, int> URIs { get; private set; }

        #region Private fields

        private CacheItemID? cacheID = null;

        #endregion

        public CacheAgent()
        {
            URIs = new Dictionary<string, int>();
            
            Server = new CacheStore();
        }

        public void Start()
        {
            // loading data
            Server.Load();

            // loading cache CIID
            object setting = Core.Settings[SettingKey.CacheCIID];

            if (setting is string)
                cacheID = new CacheItemID((string)setting);
            else
                cacheID = null;
        }

        public void Suspend()
        {
            End();
        }

        public void End()
        {
            // saving data
            Server.Save();

            // saving cache CIID
            Core.Settings[SettingKey.CacheCIID] = cacheID.HasValue ? cacheID.Value.ToString() : null;
        }

        public void OnMessageReceived(WaveServerComponent dest, Enum msgID, WaveMessage data)
        {
            if (msgID is CacheAgentMessageID)
            {
                if (data != null)
                {
                    switch ((CacheAgentMessageID)msgID)
                    {
                        case CacheAgentMessageID.RequestCacheHash:
                            {
                                WaveMessage msgOut = new WaveMessage();

                                msgOut.AddBinary(CacheAgentFieldID.CacheHash, Server.CalculateHash());

                                // return cache's own CIID (if any)
                                if (cacheID.HasValue)
                                    msgOut.AddBinary(MessageOutFieldID.CacheItemID, cacheID.Value.ToByteArray());

                                msgOut.Send(WaveServerComponent.CacheAgent, CacheAgentMessageID.CacheHashCheck);
                                break;
                            }

                        case CacheAgentMessageID.SendWABCacheCIID:
                            {
                                BinaryField bin = (BinaryField)data[MessageOutFieldID.CacheItemID];

                                if (bin.Data != null)
                                {
                                    CacheItemID temp = new CacheItemID(bin.Data);

                                    if (!cacheID.HasValue || !cacheID.Value.Equals(temp))
                                    {
                                        cacheID = temp;

                                        goto case CacheAgentMessageID.ClearCache;
                                    }
                                }

                                break;
                            }

                        case CacheAgentMessageID.ClearCache:
                            {
                                Server.Clear();

                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Called everytime the application ID changes.
        /// </summary>
        public void OnApplicationIDReceived(string unqualifiedApplicationURI, int applicationID)
        {
            if (!String.IsNullOrEmpty(unqualifiedApplicationURI))
            {
                bool store = false;
                
                if ((URIs.Count == 0) || !URIs.ContainsKey(unqualifiedApplicationURI))
                {
                    URIs[unqualifiedApplicationURI] = applicationID;
                    store = true;
                }
                else
                {
                    int oldApplicationID = -1;
                    URIs.TryGetValue(unqualifiedApplicationURI, out oldApplicationID);

                    if (oldApplicationID != applicationID)
                    {
                        URIs[unqualifiedApplicationURI] = applicationID;
                        store = true;

                        if (oldApplicationID != -1)
                            Server.PurgeApplicationID(oldApplicationID);
                    }
                }

                if (store)
                {
                    if (URIs.Count == 0)
                    {
                        Core.Settings[SettingKey.ApplicationID] = null;
                    }
                    else
                    {
                        short count = 0;
                        
                        using (MemoryStream ms = new MemoryStream())
                        {
                            ms.WriteByte(0);
                            ms.WriteShort(count);
                            
                            foreach (var item in URIs)
                            {
                                byte[] stringBytes = StringHelper.GetBytes(item.Key);

                                if ((stringBytes != null) && (stringBytes.Length > 0))
                                {
                                    ms.WriteInteger(item.Value);
                                    ms.WriteShort((short)stringBytes.Length);
                                    ms.WriteBytes(stringBytes);

                                    count++;
                                }
                            }

                            // rewind and write count of items
                            ms.Seek(1, SeekOrigin.Begin);
                            ms.WriteShort(count);

                            Core.Settings[SettingKey.ApplicationID] = ms.ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if the entity should be cached, according to the hint.
        /// </summary>
        public bool ShouldCache(byte cacheHint)
        {
            return (cacheHint & 
                ((byte)CacheHint.SessionStore_MayCache | (byte)CacheHint.SessionStore_ShouldCache | 
                (byte)CacheHint.PersistStore_MayCache | (byte)CacheHint.PersistStore_ShouldCache)) > 0;
        }
    }
}
