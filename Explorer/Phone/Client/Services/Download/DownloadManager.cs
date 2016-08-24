using System.Collections.Generic;
using Wave.Common;

namespace Wave.Services
{
    public class DownloadManager
    {
        private List<DownloadRecord> store = new List<DownloadRecord>();

        public void Start(byte[] ciid, byte[] cRefID, byte[] buffer, int downloadSize, byte cacheHint = 0)
        {
            if (((ciid == null) && (cRefID == null)) || (buffer == null))
                return;

            if (Find(ciid, cRefID) == null)
            {
                DownloadRecord dr = new DownloadRecord();

                dr.ContentRefenceID = cRefID;
                dr.CacheItemID = ciid;
                dr.Buffer = buffer;
                dr.ExpectedSize = downloadSize;
                dr.CacheHint = cacheHint;

                store.Add(dr);
            }
        }

        public void Add(byte[] ciid, byte[] cRefID, byte[] buffer)
        {
            if (buffer == null)
                return;

            DownloadRecord dr = Find(ciid, cRefID);

            if (dr != null)
            {
                if (dr.Buffer != null)
                    dr.Buffer = ByteArrayHelper.Combine(dr.Buffer, buffer);
                else
                    dr.Buffer = buffer;
            }
        }

        public DownloadRecord Retrieve(byte[] ciid, byte[] cRefID)
        {
            DownloadRecord dr = Find(ciid, cRefID);

            if (dr != null)
            {
                store.Remove(dr);
                
                return dr;
            }

            return null;
        }

        public void Remove(byte[] ciid, byte[] cRefID)
        {
            DownloadRecord dr = Find(ciid, cRefID);

            if (dr != null)
                store.Remove(dr);
        }

        public bool IsComplete(byte[] ciid, byte[] cRefID)
        {
            DownloadRecord dr = Find(ciid, cRefID);

            if (dr != null)
                return dr.IsComplete;

            return false;
        }

        public void Reset()
        {
            store.Clear();
        }

        private DownloadRecord Find(byte[] ciid, byte[] cRefID)
        {
            if ((ciid == null) && (cRefID == null))
                return null;

            if (store.Count == 0)
                return null;

            if (store.Count == 1)
            {
                DownloadRecord rec = store[0];

                if (rec != null)
                {
                    if ((ciid != null) && ByteArrayHelper.IsEqual(ciid, rec.CacheItemID))
                        return rec;

                    if ((cRefID != null) && ByteArrayHelper.IsEqual(cRefID, rec.ContentRefenceID))
                        return rec;
                }
            }
            else
            {
                bool searchCiids = (ciid != null);
                bool searchCRefs = (cRefID != null);

                foreach (DownloadRecord rec in store)
                {
                    if (searchCiids && ByteArrayHelper.IsEqual(ciid, rec.CacheItemID))
                        return rec;

                    if (searchCRefs && ByteArrayHelper.IsEqual(cRefID, rec.ContentRefenceID))
                        return rec;
                }
            }

            return null;
        }

        public class DownloadRecord
        {
            public byte[] CacheItemID { get; set; }
            public byte[] ContentRefenceID { get; set; }

            public byte[] Buffer { get; set; }

            public int ExpectedSize { get; set; }
            public byte CacheHint { get; set; }

            public int CompletedSize
            {
                get
                {
                    if (Buffer != null)
                        return Buffer.Length;
                    else
                        return 0;
                }
            }

            public bool IsComplete
            {
                get { return ((Buffer.Length > 0) && (CompletedSize == ExpectedSize)); }
            }
        }
    }
}
