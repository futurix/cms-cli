using System;
using System.IO;

namespace Wave.Common
{
    public static class BinaryHelper
    {
        public static string ReadString(Stream str)
        {
            if (str != null)
            {
                int dataLength = str.ReadInteger();

                if (dataLength > 0)
                {
                    byte[] stringBytes = str.ReadBytes(dataLength);

                    if (stringBytes != null)
                        return StringHelper.GetString(stringBytes);
                }
            }

            return String.Empty;
        }

        public static void WriteString(Stream str, string data)
        {
            if (str != null)
            {
                if (!String.IsNullOrEmpty(data))
                {
                    byte[] stringBytes = StringHelper.GetBytes(data);

                    if (stringBytes != null)
                    {
                        str.WriteInteger(stringBytes.Length);
                        str.WriteBytes(stringBytes);
                    }
                    else
                        str.WriteInteger(0);
                }
                else
                    str.WriteInteger(0);
            }
        }
    }
}
