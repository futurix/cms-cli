using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public sealed class DBCacheStore : ICacheStore
    {
        public const string CacheConnectionString = "Data Source=isostore:/" + CacheDirectory + "/" + CacheDatabase;
        public const string CacheDirectory = "dbcache1";
        public const string CacheDatabase = "cache.sdf";
        public const string CacheFileExtension = "wex";

        private CacheDataContext dc = null;
        private IsolatedStorageFile files = null;
        
        public DBCacheStore()
        {
        }
        
        void ICacheStore.Load()
        {
            // preparing file storage
            files = IsolatedStorageFile.GetUserStoreForApplication();

            if (!files.DirectoryExists(CacheDirectory))
                files.CreateDirectory(CacheDirectory);

            // creating data context
            dc = new CacheDataContext(CacheConnectionString);

            if (!dc.DatabaseExists())
                dc.CreateDatabase();
        }

        void ICacheStore.Save()
        {
            if (dc != null)
                dc.SubmitChanges();
        }

        ICacheRecord ICacheStore.this[long recordID]
        {
            get { return dc.ServerCache.FirstOrDefault(r => r.ID == recordID); }
        }

        long ICacheStore.Add(byte[] data, CacheItemID? ciid, byte[] key, CacheEntityType entityType, CacheMode cacheMode)
        {
            if (((key == null) && !ciid.HasValue) || (data == null))
                return CacheStore.BadRecordID;

            DBCacheRecord rec = CreateRecord(data, ciid, key, entityType, cacheMode);

            dc.ServerCache.InsertOnSubmit(rec);
            dc.SubmitChanges();

            return rec.ID;
        }

        long[] ICacheStore.AddRange(List<CacheDataTemplate> items)
        {
            if ((items != null) && (items.Count > 0))
            {
                DBCacheRecord[] records = new DBCacheRecord[items.Count];
                long[] res = new long[items.Count];

                for (int i = 0; i < items.Count; i++)
                {
                    if ((items[i] == null) || ((items[i].Key == null) && !items[i].CIID.HasValue) || (items[i].Data == null))
                    {
                        res[i] = CacheStore.BadRecordID;
                        continue;
                    }

                    records[i] = CreateRecord(items[i].DataBinary, items[i].CIID, items[i].Key, items[i].EntityType, items[i].Mode ?? CacheMode.Persistant);
                }

                for (int i = 0; i < records.Length; i++)
                    if (records[i] != null)
                        dc.ServerCache.InsertOnSubmit(records[i]);

                dc.SubmitChanges();

                for (int i = 0; i < records.Length; i++)
                {
                    if (records[i] != null)
                        res[i] = records[i].ID;
                    else
                        res[i] = CacheStore.BadRecordID;
                }

                return res;
            }

            return new long[0];
        }

        void ICacheStore.Remove(long recordID)
        {
            var results = dc.ServerCache.Where(r => r.ID == recordID);

            foreach (DBCacheRecord rec in results)
                rec.RemoveLinkedFile();

            dc.ServerCache.DeleteAllOnSubmit(results);
            dc.SubmitChanges();
        }

        bool ICacheStore.Contains(long recordID)
        {
            return dc.ServerCache.Any(r => r.ID == recordID);
        }

        void ICacheStore.EnumerateAll(Action<CacheItemID?, byte[], long> callback)
        {
            if (callback != null)
            {
                foreach (DBCacheRecord rec in dc.ServerCache)
                    callback(((ICacheRecord)rec).CIID, ((ICacheRecord)rec).Key, rec.ID);
            }
        }

        void ICacheStore.PurgeApplicationID(int appID)
        {
            var results = dc.ServerCache
                .Where(r => r.Key != null)
                .Select(r => r.Key)
                .AsEnumerable()
                .Where(k => EntityID.GetApplicationID(k.ToArray()) == appID);

            var records = dc.ServerCache
                .Where(r => r.Key != null)
                .Where(r => results.Contains(r.Key));

            foreach (DBCacheRecord rec in records)
                rec.RemoveLinkedFile();

            dc.ServerCache.DeleteAllOnSubmit(records);
            dc.SubmitChanges();
        }

        void ICacheStore.Clear()
        {
            dc.ServerCache.DeleteAllOnSubmit(dc.ServerCache);
            dc.SubmitChanges();
        }

        private DBCacheRecord CreateRecord(byte[] data, CacheItemID? ciid, byte[] key, CacheEntityType entityType, CacheMode cacheMode)
        {
            if (key.Length <= DBCacheRecord.KeySizeMaximum)
            {
                DBCacheRecord rec = new DBCacheRecord();

                if (ciid.HasValue)
                    rec.CIID = ciid.Value.ToByteArray();
                else
                    rec.CIID = null;

                rec.Key = key;
                rec.EntityType = (short)entityType;
                rec.CacheMode = (short)cacheMode;
                rec.SetData(data);

                return rec;
            }

            return null;
        }

        public static void DestroyOffline()
        {
            using (IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    // deleting index
                    string dbFilePath = String.Format(CacheStore.CacheFilePathNoExtFormat, CacheDirectory, CacheDatabase);

                    if (fileStorage.FileExists(dbFilePath))
                        fileStorage.DeleteFile(dbFilePath);

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
