using System;
using System.Collections.Generic;
using System.Text;
using Wave.Common;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class CacheStoreIndices
    {
        // main indices
        private Dictionary<CacheItemID, long> ciidIndex = new Dictionary<CacheItemID, long>();
        private Dictionary<byte[], long> keyIndex = new Dictionary<byte[], long>(new ByteArrayComparer());
        private Dictionary<long, object> liveIndex = new Dictionary<long, object>();

        // reverse search index
        private Dictionary<long, byte[]> linkIndex = new Dictionary<long, byte[]>();

        public CacheStoreIndices()
        {
        }

        public long this[CacheItemID ciid]
        {
            get
            {
                long res;

                if (ciidIndex.TryGetValue(ciid, out res))
                    return res;
                else
                    return CacheStore.BadRecordID;
            }
        }

        public long this[byte[] key]
        {
            get
            {
                long res;

                if (keyIndex.TryGetValue(key, out res))
                    return res;
                else
                    return CacheStore.BadRecordID;
            }
        }

        public void Add(CacheItemID? ciid, byte[] key, long recordID, object liveEntity = null)
        {
            if ((!ciid.HasValue && (key == null)) || (recordID < 0))
                return;

            // add to CIID index
            if (ciid.HasValue)
                ciidIndex[ciid.Value] = recordID;

            // add to key index and link index
            if (key != null)
            {
                keyIndex[key] = recordID;
                linkIndex[recordID] = key;
            }

            // add to live index
            if (liveEntity != null)
                liveIndex[recordID] = liveEntity;
        }

        public void Remove(CacheItemID ciid)
        {
            if (ciidIndex.ContainsKey(ciid))
            {
                // find record id
                long recordID = ciidIndex[ciid];

                // try to find paired key
                byte[] key = null;
                
                if (linkIndex.ContainsKey(recordID))
                    key = linkIndex[recordID];

                // remove CIID
                ciidIndex.Remove(ciid);

                // remove key and link (if any)
                if (key != null)
                {
                    keyIndex.Remove(key);
                    linkIndex.Remove(recordID);
                }

                // remove live entity (if any)
                if (liveIndex.ContainsKey(recordID))
                    liveIndex.Remove(recordID);
            }
        }

        public object FindLiveEntity(CacheItemID? ciid, byte[] key)
        {
            long recordID = FindRecordID(ciid, key);

            if ((recordID != CacheStore.BadRecordID) && liveIndex.ContainsKey(recordID))
                return liveIndex[recordID];
            else
                return null;
        }

        public void AddLiveEntity(CacheItemID? ciid, byte[] key, object liveEntity)
        {
            long recordID = FindRecordID(ciid, key);

            if (recordID != CacheStore.BadRecordID)
                liveIndex[recordID] = liveEntity;
        }

        public byte[] FindKey(CacheItemID ciid)
        {
            long recordID = this[ciid];

            if ((recordID != CacheStore.BadRecordID) && linkIndex.ContainsKey(recordID))
                return linkIndex[recordID];

            return null;
        }

        public bool Contains(CacheItemID ciid)
        {
            return ciidIndex.ContainsKey(ciid);
        }

        public bool Contains(byte[] key)
        {
            return keyIndex.ContainsKey(key);
        }

        public byte[] CalculateHash()
        {
            // prepare sorted array of cache IDs
            List<string> IDs = new List<string>();

            foreach (var pair in ciidIndex)
                IDs.Add(pair.Key.ToString());

            IDs.Sort(StringComparer.InvariantCulture);

            // make the hash string
            StringBuilder sb = new StringBuilder();

            foreach (string item in IDs)
                sb.Append(item);

            return StringHelper.GetBytes(sb.ToString());
        }

        public void Clear()
        {
            ciidIndex.Clear();
            keyIndex.Clear();
            liveIndex.Clear();
            linkIndex.Clear();
        }

        private long FindRecordID(CacheItemID? ciid, byte[] key)
        {
            long recordID = CacheStore.BadRecordID;

            if (ciid.HasValue && ciidIndex.ContainsKey(ciid.Value))
                recordID = ciidIndex[ciid.Value];

            if ((recordID == CacheStore.BadRecordID) && (key != null) && keyIndex.ContainsKey(key))
                recordID = keyIndex[key];

            return recordID;
        }
    }
}
