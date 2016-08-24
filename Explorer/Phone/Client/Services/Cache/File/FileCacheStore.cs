using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Wave.Common;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public sealed class FileCacheStore : ICacheStore
    {
        public const string CacheIndexFileName = "cache.dat";
        public const string CacheDirectory = "fwcache1";
        public const string CacheFileExtension = "wex";

        private List<FileCacheRecord> data = new List<FileCacheRecord>();
        private IsolatedStorageFile files = null;

        private long nextRecordID = 0;
        
        public FileCacheStore()
        {
        }
        
        void ICacheStore.Load()
        {
            // preparing file storage
            files = IsolatedStorageFile.GetUserStoreForApplication();

            if (!files.DirectoryExists(CacheDirectory))
                files.CreateDirectory(CacheDirectory);

            // loading next record number
            object temp = Core.Settings[SettingKey.FileCacheIteration];

            if ((temp != null) && (temp is long))
                nextRecordID = (long)temp;
            else
                nextRecordID = 1;

            // loading index
            string indexFileName = String.Format(CacheStore.CacheFilePathNoExtFormat, CacheDirectory, CacheIndexFileName);

            if (files.FileExists(indexFileName))
            {
                using (IsolatedStorageFileStream str = new IsolatedStorageFileStream(indexFileName, FileMode.Open, files))
                {
                    if (str.ReadByte() == 0)
                    {
                        while (1 == 1)
                        {
                            FileCacheRecord rec = new FileCacheRecord();

                            if (rec.LoadFromStream(str))
                                data.Add(rec);
                            else
                                break;
                        }
                    }
                }
            }
        }

        void ICacheStore.Save()
        {
            // saving next record number
            Core.Settings[SettingKey.FileCacheIteration] = nextRecordID;

            // saving index
            string indexFileName = String.Format(CacheStore.CacheFilePathNoExtFormat, CacheDirectory, CacheIndexFileName);

            using (IsolatedStorageFileStream str = new IsolatedStorageFileStream(indexFileName, FileMode.Create, files))
            {
                str.WriteByte(0);

                foreach (FileCacheRecord record in data)
                    record.SaveToStream(str);
            }
        }

        ICacheRecord ICacheStore.this[long recordID]
        {
            get { return data.FirstOrDefault(r => r.ID == recordID); }
        }

        long ICacheStore.Add(byte[] newData, CacheItemID? ciid, byte[] key, CacheEntityType entityType, CacheMode cacheMode)
        {
            if (((key == null) && !ciid.HasValue) || (newData == null))
                return CacheStore.BadRecordID;

            // creating a new record
            FileCacheRecord rec = CreateRecord(newData, ciid, key, entityType, cacheMode);

            data.Add(rec);

            return rec.ID;
        }

        long[] ICacheStore.AddRange(List<CacheDataTemplate> items)
        {
            if ((items != null) && (items.Count > 0))
            {
                long[] res = new long[items.Count];

                for (int i = 0; i < items.Count; i++)
                {
                    if ((items[i] == null) || ((items[i].Key == null) && !items[i].CIID.HasValue) || (items[i].Data == null))
                    {
                        res[i] = CacheStore.BadRecordID;
                        continue;
                    }

                    // creating a new record
                    FileCacheRecord rec = CreateRecord(items[i].DataBinary, items[i].CIID, items[i].Key, items[i].EntityType, items[i].Mode ?? CacheMode.Persistant);

                    data.Add(rec);

                    res[i] = rec.ID;
                }

                return res;
            }

            return new long[0];
        }

        void ICacheStore.Remove(long recordID)
        {
            var results = data.Where(r => r.ID == recordID);

            foreach (FileCacheRecord rec in results)
            {
                rec.RemoveLinkedFile();

                data.Remove(rec);
            }
        }

        bool ICacheStore.Contains(long recordID)
        {
            return data.Any(r => r.ID == recordID);
        }

        void ICacheStore.EnumerateAll(Action<CacheItemID?, byte[], long> callback)
        {
            if (callback != null)
            {
                foreach (FileCacheRecord rec in data)
                    callback(rec.CIID, rec.Key, rec.ID);
            }
        }

        void ICacheStore.PurgeApplicationID(int appID)
        {
            var results = data
                .Where(r => r.Key != null)
                .Where(r => EntityID.GetApplicationID(r.Key) == appID);

            foreach (FileCacheRecord rec in results)
            {
                rec.RemoveLinkedFile();

                data.Remove(rec);
            }
        }

        void ICacheStore.Clear()
        {
            foreach (FileCacheRecord rec in data)
                rec.RemoveLinkedFile();
            
            data.Clear();
        }

        private FileCacheRecord CreateRecord(byte[] newData, CacheItemID? ciid, byte[] key, CacheEntityType entityType, CacheMode cacheMode)
        {
            FileCacheRecord rec = new FileCacheRecord();
            rec.ID = nextRecordID++;

            if (ciid.HasValue)
                rec.CIID = ciid.Value;
            else
                rec.CIID = null;

            rec.Key = key;
            rec.EntityType = entityType;
            rec.CacheMode = cacheMode;

            rec.SetDataFile(Guid.NewGuid().ToString());
            rec.Data = newData;

            return rec;
        }

        public static void DestroyOffline()
        {
            using (IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    // deleting index
                    string indexFilePath = String.Format(CacheStore.CacheFilePathNoExtFormat, CacheDirectory, CacheIndexFileName);

                    if (fileStorage.FileExists(indexFilePath))
                        fileStorage.DeleteFile(indexFilePath);

                    // deleting individual records
                    if (fileStorage.DirectoryExists(CacheDirectory))
                    {
                        string[] fileNames = fileStorage.GetFileNames(String.Format("{0}/*", CacheDirectory));

                        foreach (string fileName in fileNames)
                            fileStorage.DeleteFile(String.Format(CacheStore.CacheFilePathNoExtFormat, CacheDirectory, fileName));

                        fileStorage.DeleteDirectory(CacheDirectory);
                    }
                }
                catch (IOException)
                {
                }
            }
        }
    }
}
