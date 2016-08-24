using System.Collections.Generic;
using System.Linq;

namespace Wave.Common
{
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (x == null || y == null)
                return x == y;

            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            if (obj == null)
                return -1;

            return ByteArrayHelper.GetReasonablyGoodHashCode(obj);
        }
    }
}
