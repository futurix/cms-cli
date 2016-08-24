using System;
using System.IO;
using System.Windows.Media;

namespace Wave.Common
{
    public static class StreamExtensions
    {
        public static short ReadShort(this Stream str)
        {
            byte[] temp = new byte[2];

            if (str.Read(temp, 0, 2) == 2)
                return BitConverter.ToInt16(temp, 0);
            else
                return -1;
        }

        public static int ReadInteger24(this Stream str)
        {
            byte[] temp = new byte[3];

            if (str.Read(temp, 0, 3) == 3)
                return (temp[2] << 16) | (temp[1] << 8) | (temp[0] << 0);
            else
                return -1;
        }

        public static int ReadInteger(this Stream str)
        {
            byte[] temp = new byte[4];

            if (str.Read(temp, 0, 4) == 4)
                return BitConverter.ToInt32(temp, 0);
            else
                return -1;
        }

        public static long ReadLong(this Stream str)
        {
            byte[] temp = new byte[8];

            if (str.Read(temp, 0, 8) == 8)
                return BitConverter.ToInt64(temp, 0);
            else
                return -1;
        }

        public static double ReadDouble(this Stream str)
        {
            byte[] temp = new byte[8];

            if (str.Read(temp, 0, 8) == 8)
                return BitConverter.ToDouble(temp, 0);
            else
                return Double.NaN;
        }

        public static bool ReadBool(this Stream str)
        {
            if (str.ReadByte() == 1)
                return true;
            else
                return false;
        }

        public static Color ReadColour(this Stream str)
        {
            return Color.FromArgb((byte)str.ReadByte(), (byte)str.ReadByte(), (byte)str.ReadByte(), (byte)str.ReadByte());
        }

        public static byte[] ReadBytes(this Stream str, int count)
        {
            byte[] temp = new byte[count];

            if (str.Read(temp, 0, count) == count)
                return temp;
            else
                return null;
        }

        public static void WriteShort(this Stream str, short value)
        {
            str.Write(BitConverter.GetBytes(value), 0, 2);
        }

        public static void WriteInteger24(this Stream str, int value)
        {
            byte[] temp = BitConverter.GetBytes(value);

            str.WriteByte(temp[0]);
            str.WriteByte(temp[1]);
            str.WriteByte(temp[2]);
        }

        public static void WriteInteger(this Stream str, int value)
        {
            str.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public static void WriteLong(this Stream str, long value)
        {
            str.Write(BitConverter.GetBytes(value), 0, 8);
        }

        public static void WriteDouble(this Stream str, double value)
        {
            str.Write(BitConverter.GetBytes(value), 0, 8);
        }

        public static void WriteBool(this Stream str, bool value)
        {
            str.WriteByte((byte)(value ? 1 : 0));
        }

        public static void WriteColour(this Stream str, Color value)
        {
            str.WriteByte(value.A);
            str.WriteByte(value.R);
            str.WriteByte(value.G);
            str.WriteByte(value.B);
        }

        public static void WriteBytes(this Stream str, byte[] bytes)
        {
            str.Write(bytes, 0, bytes.Length);
        }

        public static byte[] ReadAll(this Stream stream)
        {
            byte[] buffer = new byte[32768];

            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);

                    if (read <= 0)
                        return ms.ToArray();

                    ms.Write(buffer, 0, read);
                }
            }
        }
    }
}
