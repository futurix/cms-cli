using System;
using System.Windows.Media;

namespace Wave.Common
{
    public static class DataHelper
    {
        public static long MaximumFileTime = 0;
        
        static DataHelper()
        {
            MaximumFileTime = DateTime.MaxValue.ToFileTime();
        }
        
        public static int StringToInt(string source, int defaultValue = int.MinValue)
        {
            int res = defaultValue;

            if (!String.IsNullOrEmpty(source))
                int.TryParse(source, out res);

            return res;
        }

        public static DateTime FileTimeToDateTime(long fileTime)
        {
            if ((fileTime >= 0) && (fileTime <= MaximumFileTime))
                return DateTime.FromFileTime(fileTime);
            else
                return DateTime.FromFileTime(0);
        }

        public static int BytesToInt24(byte[] input)
        {
            if ((input == null) || (input.Length != 3))
                return -1;

            return (input[2] << 16) | (input[1] << 8) | (input[0] << 0);
        }

        public static Color IntToColour(int source)
        {
            return Color.FromArgb(
                (byte)((source >> 24) & 0xff),
                (byte)((source >> 16) & 0xff),
                (byte)((source >> 8) & 0xff),
                (byte)((source >> 0) & 0xff));
        }

        public static Color? HexToColour(string hex)
        {
            if (String.IsNullOrWhiteSpace(hex) || (hex.Length < 4) || (hex[0] != '#'))
                return null;

            if ((hex.Length == 9) || (hex.Length == 7) || (hex.Length == 4))
            {
                byte a = 255, r = 255, g = 255, b = 255;
                
                if (hex.Length == 9)
                {
                    a = Convert.ToByte(hex.Substring(1, 2), 16);
                    r = Convert.ToByte(hex.Substring(3, 2), 16);
                    g = Convert.ToByte(hex.Substring(5, 2), 16);
                    b = Convert.ToByte(hex.Substring(7, 2), 16);
                }
                else if (hex.Length == 7)
                {
                    r = Convert.ToByte(hex.Substring(1, 2), 16);
                    g = Convert.ToByte(hex.Substring(3, 2), 16);
                    b = Convert.ToByte(hex.Substring(5, 2), 16);
                }
                else if (hex.Length == 4)
                {
                    r = Convert.ToByte(hex.Substring(1, 1) + hex.Substring(1, 1), 16);
                    g = Convert.ToByte(hex.Substring(2, 1) + hex.Substring(2, 1), 16);
                    b = Convert.ToByte(hex.Substring(3, 1) + hex.Substring(3, 1), 16);
                }

                return Color.FromArgb(a, r, g, b);
            }

            return null;
        }
    }
}
