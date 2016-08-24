using System;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public sealed class FileCacheRecord : ICacheRecord, IEquatable<FileCacheRecord>
    {
        public long ID { get; set; }
        public CacheItemID? CIID { get; set; }
        public byte[] Key { get; set; }

        public CacheEntityType EntityType { get; set; }
        public CacheMode CacheMode { get; set; }

        public string DataFile { get { return dataFile; } }

        private string dataFile = null;

        public FileCacheRecord()
        {
        }

        public byte[] Data
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(dataFile))
                {
                    string filePath = String.Format(CacheStore.CacheFilePathFormat, FileCacheStore.CacheDirectory, dataFile, FileCacheStore.CacheFileExtension);

                    if (IsolatedStorageHelper.FileExists(filePath))
                        return IsolatedStorageHelper.ReadFile(filePath);
                }

                return null;
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(dataFile))
                {
                    string filePath = String.Format(CacheStore.CacheFilePathFormat, FileCacheStore.CacheDirectory, dataFile, FileCacheStore.CacheFileExtension);

                    if (IsolatedStorageHelper.FileExists(filePath))
                        IsolatedStorageHelper.DeleteFile(filePath);

                    if (value != null)
                        IsolatedStorageHelper.WriteFile(filePath, value);
                }
            }
        }

        public bool Equals(FileCacheRecord other)
        {
            if (ID != other.ID)
                return false;
            
            if (EntityType != other.EntityType)
                return false;

            if (CacheMode != other.CacheMode)
                return false;

            if (CIID.HasValue && other.CIID.HasValue && !CIID.Value.Equals(other.CIID.Value))
                return false;

            if ((CIID.HasValue && !other.CIID.HasValue) || (!CIID.HasValue && other.CIID.HasValue))
                return false;

            if (Key != other.Key)
                return false;

            if (Data != other.Data)
                return false;

            if (DataFile != other.DataFile)
                return false;

            return true;
        }

        #region Storage

        public bool LoadFromStream(Stream str)
        {
            if (str != null)
            {
                if (str.ReadByte() == 0)
                {
                    long id = str.ReadLong();
                    short entType = str.ReadShort();
                    short cacheMode = str.ReadShort();

                    if ((id != -1) && (entType != -1) && (cacheMode != -1))
                    {
                        int ciidLength = str.ReadByte();

                        if (ciidLength != -1)
                        {
                            byte[] ciid = null;

                            if (ciidLength > 0)
                                ciid = str.ReadBytes(ciidLength);

                            short keyLength = str.ReadShort();

                            if (keyLength != -1)
                            {
                                byte[] key = null;

                                if (keyLength > 0)
                                    key = str.ReadBytes(keyLength);

                                short dataFileLength = str.ReadShort();

                                if (dataFileLength != -1)
                                {
                                    string dataFileName = null;

                                    if (dataFileLength > 0)
                                    {
                                        byte[] dataFileBytes = str.ReadBytes(dataFileLength);

                                        if (dataFileBytes != null)
                                            dataFileName = StringHelper.GetString(dataFileBytes);
                                    }

                                    if (!String.IsNullOrEmpty(dataFileName) &&
                                        (id != CacheStore.BadRecordID) && 
                                        IsolatedStorageHelper.FileExists(
                                            String.Format(
                                                CacheStore.CacheFilePathFormat, 
                                                FileCacheStore.CacheDirectory, dataFileName, FileCacheStore.CacheFileExtension)))
                                    {
                                        ID = id;
                                        EntityType = (CacheEntityType)entType;
                                        CacheMode = (CacheMode)cacheMode;

                                        if (ciid != null)
                                            CIID = new CacheItemID(ciid);
                                        else
                                            CIID = null;

                                        Key = key;
                                        dataFile = dataFileName;

                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool SaveToStream(Stream str)
        {
            if (str != null)
            {
                // version byte
                str.WriteByte(0);

                str.WriteLong(ID);
                str.WriteShort((short)EntityType);
                str.WriteShort((short)CacheMode);

                if (CIID.HasValue)
                {
                    byte[] ciid = CIID.Value.ToByteArray();

                    str.WriteByte((byte)ciid.Length);
                    str.WriteBytes(ciid);
                }
                else
                    str.WriteByte(0);

                if ((Key != null) && (Key.Length > 0))
                {
                    str.WriteShort((short)Key.Length);
                    str.WriteBytes(Key);
                }
                else
                    str.WriteByte(0);

                if (!String.IsNullOrEmpty(dataFile))
                {
                    byte[] dfb = StringHelper.GetBytes(dataFile);

                    str.WriteShort((short)dfb.Length);
                    str.WriteBytes(dfb);
                }
                else
                    str.WriteByte(0);

                return true;
            }

            return false;
        }

        #endregion

        #region Utility methods

        public void SetDataFile(string newFileName)
        {
            dataFile = newFileName;
        }

        public void RemoveLinkedFile()
        {
            if (!String.IsNullOrWhiteSpace(dataFile))
            {
                string filePath = String.Format(CacheStore.CacheFilePathFormat, FileCacheStore.CacheDirectory, dataFile, FileCacheStore.CacheFileExtension);

                if (IsolatedStorageHelper.FileExists(filePath))
                    IsolatedStorageHelper.DeleteFile(filePath);
            }
        }

        #endregion
    }
}
