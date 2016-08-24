using System;

namespace Wave.Common
{
    public static class ByteArrayHelper
    {
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] res = new byte[first.Length + second.Length];
            
            Buffer.BlockCopy(first, 0, res, 0, first.Length);
            Buffer.BlockCopy(second, 0, res, first.Length, second.Length);
            
            return res;
        }

        public static bool IsEqual(byte[] first, byte[] second)
        {
            if ((first == null) || (second == null))
                return false;
            
            if (first.Length != second.Length)
                return false;
            
            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                    return false;
            }

            return true;
        }

        public static int GetReasonablyGoodHashCode(byte[] input)
        {
            if (input == null)
                return 0;

            int res = 0;

            foreach (byte item in input)
                res = 33 * res + item;

            return res;
        }
    }
}
