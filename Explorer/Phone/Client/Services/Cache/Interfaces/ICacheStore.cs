using System;
using System.Collections.Generic;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public interface ICacheStore
    {
        void Load();
        void Save();
        
        ICacheRecord this[long recordID]
        {
            get;
        }

        long Add(byte[] data, CacheItemID? ciid, byte[] key, CacheEntityType entityType, CacheMode cacheMode);
        long[] AddRange(List<CacheDataTemplate> items);
        void Remove(long recordID);
        bool Contains(long recordID);

        void EnumerateAll(Action<CacheItemID?, byte[], long> callback);

        void PurgeApplicationID(int appID);
        void Clear();
    }
}
