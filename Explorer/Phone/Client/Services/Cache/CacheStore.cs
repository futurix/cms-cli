using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Wave.Common;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class CacheStore
    {
        public const string CacheFilePathFormat = "{0}/{1}.{2}";
        public const string CacheFilePathNoExtFormat = "{0}/{1}";
        public const long BadRecordID = -1;
        
        private CacheStoreIndices indices = new CacheStoreIndices();
        private ICacheStore store = null;

        public CacheStore()
        {
        }

        public object this[CacheItemID id]
        {
            get
            {
                if (store != null)
                {
                    long recordID = indices[id];

                    if (recordID != BadRecordID)
                    {
                        ICacheRecord record = store[recordID];

                        if (record != null)
                            return CacheHelper.Unpack(record.Data);
                    }
                }

                return null;
            }
        }

        public object this[byte[] key]
        {
            get
            {
                if (store != null)
                {
                    long recordID = indices[key];

                    if (recordID != BadRecordID)
                    {
                        ICacheRecord record = store[recordID];

                        if (record != null)
                            return CacheHelper.Unpack(record.Data);
                    }
                }

                return null;
            }
        }

        public object this[string uri]
        {
            get
            {
                if (store != null)
                {
                    if (!String.IsNullOrEmpty(uri))
                    {
                        byte[] data = StringHelper.GetBytes(uri);

                        if (data != null)
                        {
                            long recordID = indices[data];

                            if (recordID != BadRecordID)
                            {
                                ICacheRecord record = store[recordID];

                                if (record != null)
                                    return CacheHelper.Unpack(record.Data);
                            }
                        }
                    }
                }

                return null;
            }
        }

        public bool Add(object data, CacheItemID? ciid, byte[] key, CacheEntityType entityType, byte cacheHint = 0, bool cacheLiveData = false)
        {
            if (store != null)
            {
                if ((!ciid.HasValue && (key == null)) || (data == null))
                    return false;

                bool isSession = (cacheHint & ((byte)CacheHint.SessionStore_MayCache | (byte)CacheHint.SessionStore_ShouldCache)) > 0;
                bool isPersistent = (cacheHint & ((byte)CacheHint.PersistStore_MayCache | (byte)CacheHint.PersistStore_ShouldCache)) > 0;

                // determine cache mode
                CacheMode? mode = null;

                if (isPersistent)
                    mode = CacheMode.Persistant;
                else if (isSession)
                    mode = CacheMode.Session;

                // add to cache
                if (mode.HasValue)
                {
                    byte[] packed = CacheHelper.Pack(data);
                    
                    if (packed != null)
                    {
                        long recordID = store.Add(packed, ciid, key, entityType, mode.Value);

                        if (recordID != BadRecordID)
                        {
                            indices.Add(ciid, key, recordID);

                            if (cacheLiveData)
                                AddLiveEntity(ciid, key, data);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool AddRange(List<CacheDataTemplate> items, bool cacheLiveData = false)
        {
            if ((store != null) && (items != null) && (items.Count > 0))
            {
                List<CacheDataTemplate> records = new List<CacheDataTemplate>(items.Count);

                foreach (CacheDataTemplate item in items)
                {
                    if ((item == null) || (!item.CIID.HasValue && (item.Key == null)) || (item.Data == null))
                        continue;

                    bool isSession = (item.CacheHint & ((byte)CacheHint.SessionStore_MayCache | (byte)CacheHint.SessionStore_ShouldCache)) > 0;
                    bool isPersistent = (item.CacheHint & ((byte)CacheHint.PersistStore_MayCache | (byte)CacheHint.PersistStore_ShouldCache)) > 0;

                    // determine cache mode
                    if (isPersistent)
                        item.Mode = CacheMode.Persistant;
                    else if (isSession)
                        item.Mode = CacheMode.Session;
                    else
                        item.Mode = null;

                    // add to cache
                    if (item.Mode.HasValue)
                    {
                        item.DataBinary = CacheHelper.Pack(item.Data);

                        if (item.DataBinary != null)
                            records.Add(item);
                    }
                }

                long[] IDs = store.AddRange(records);

                for (int i = 0; i < IDs.Length; i++)
                {
                    if (IDs[i] != BadRecordID)
                    {
                        indices.Add(records[i].CIID, records[i].Key, IDs[i]);

                        if (cacheLiveData)
                            AddLiveEntity(records[i].CIID, records[i].Key, records[i].Data);
                    }
                }

                return true;
            }

            return false;
        }

        public void Remove(CacheItemID ciid)
        {
            if (store != null)
            {
                long recordID = indices[ciid];

                if (recordID != BadRecordID)
                {
                    store.Remove(recordID);
                    indices.Remove(ciid);
                }
            }
        }

        public object FindLiveEntity(CacheItemID? ciid, byte[] key)
        {
            if (store != null)
                return indices.FindLiveEntity(ciid, key);
            else
                return null;
        }

        public void AddLiveEntity(CacheItemID? ciid, byte[] key, object liveEntity)
        {
            if (store != null)
                indices.AddLiveEntity(ciid, key, liveEntity);
        }

        public byte[] FindKey(CacheItemID ciid)
        {
            if (store != null)
                return indices.FindKey(ciid);
            else
                return null;
        }

        public bool Contains(CacheItemID ciid)
        {
            if (store != null)
                return indices.Contains(ciid);
            else
                return false;
        }

        public bool Contains(byte[] key)
        {
            if (store != null)
                return indices.Contains(key);
            else
                return false;
        }

        public void Load()
        {
            if (store == null)
            {
                store = CreateCacheStoreObject();

                if (store != null)
                {
                    store.Load();

                    PopulateIndices();
                }
            }
        }

        public void Save()
        {
            if (store != null)
                store.Save();
        }

        public void Clear()
        {
            if (store != null)
            {
                indices.Clear();
                store.Clear();
            }
        }

        public byte[] CalculateHash()
        {
            if (store != null)
                return indices.CalculateHash();
            else
                return new byte[0];
        }

        public void PurgeApplicationID(int appID)
        {
            if (store != null)
            {
                // remove items
                store.PurgeApplicationID(appID);

                // recreate indices
                indices.Clear();
                PopulateIndices();
            }
        }

        public void DestroyCacheOffline()
        {
            // deleting cache files
            DBCacheStore.DestroyOffline();

            // deleting cache CIID
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains(SettingKey.CacheCIID))
                settings.Remove(SettingKey.CacheCIID);
        }

        private ICacheStore CreateCacheStoreObject()
        {
            return new DBCacheStore();
        }

        private void PopulateIndices()
        {
            if (store != null)
                store.EnumerateAll((ciid, key, recID) => indices.Add(ciid, key, recID));
        }
    }
}
