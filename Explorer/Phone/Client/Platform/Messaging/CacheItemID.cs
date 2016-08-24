using System;
using System.IO;
using Wave.Common;

namespace Wave.Platform.Messaging
{
    public struct CacheItemID : IEquatable<CacheItemID>
    {
        public const int MaximumBinarySize = 14;
        
        public long ItemID { get; private set; }
        public short GeneratorID { get; private set; }
        public short CacheID { get; private set; }
        public short ClusterID { get; private set; }

        public CacheItemID(byte[] input)
            : this()
        {
            if (input != null)
            {
                using (MemoryStream ms = new MemoryStream(input))
                {
                    ItemID = (input.Length >= 8) ? ms.ReadLong() : 0;
                    GeneratorID = (input.Length >= 10) ? ms.ReadShort() : (short)0;
                    CacheID = (input.Length >= 12) ? ms.ReadShort() : (short)0;
                    ClusterID = (input.Length >= 14) ? ms.ReadShort() : (short)0;
                }
            }
        }

        public CacheItemID(string input)
            : this()
        {
            if (!String.IsNullOrEmpty(input))
            {
                long tempItemID = 0;
                short tempGeneratorID = 0, tempCacheID = 0, tempClusterID = 0;
                string[] temp = input.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                if (temp.Length >= 1)
                    long.TryParse(temp[0], out tempItemID);

                ItemID = tempItemID;

                if (temp.Length >= 2)
                    short.TryParse(temp[1], out tempGeneratorID);
                
                GeneratorID = tempGeneratorID;

                if (temp.Length >= 3)
                    short.TryParse(temp[2], out tempCacheID);

                CacheID = tempCacheID;

                if (temp.Length >= 4)
                    short.TryParse(temp[3], out tempClusterID);

                ClusterID = tempClusterID;
            }
        }

        public bool Equals(CacheItemID other)
        {
            if (ItemID != other.ItemID)
                return false;

            if (GeneratorID != other.GeneratorID)
                return false;

            if (CacheID != other.CacheID)
                return false;

            if (ClusterID != other.ClusterID)
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CacheItemID))
                return false;

            return Equals((CacheItemID)obj);
        }

        public override int GetHashCode()
        {
            return (int)ItemID ^ (int)(ItemID >> 32) ^ GeneratorID ^ CacheID ^ ClusterID;
        }

        public override string ToString()
        {
            return String.Format("{0:X16}.{1:X4}.{2:X4}.{3:X4}", ItemID, GeneratorID, CacheID, ClusterID);
        }

        public string ToHexString()
        {
            return String.Format("{0:X16}{1:X4}{2:X4}{3:X4}", ItemID, GeneratorID, CacheID, ClusterID);
        }

        public byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteLong(ItemID);
                ms.WriteShort(GeneratorID);
                ms.WriteShort(CacheID);
                ms.WriteShort(ClusterID);

                return ms.ToArray();
            }
        }
    }
}
