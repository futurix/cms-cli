using System.IO;
using Ionic.Zlib;

namespace Wave.Common
{
    public static class CompressionHelper
    {
        public static byte[] GZipBuffer(byte[] input)
        {
            using (MemoryStream mem = new MemoryStream())
            using (GZipStream gzip = new GZipStream(mem, CompressionMode.Compress))
            {
                gzip.WriteBytes(input);
                
                return mem.ToArray();
            }
        }

        public static byte[] DeflateBuffer(byte[] input)
        {
            using (MemoryStream inputStream = new MemoryStream(input))
            using (ZlibStream deflater = new ZlibStream(inputStream, CompressionMode.Decompress))
            using (MemoryStream outputStream = new MemoryStream())
            {
                byte[] buff = new byte[512];
                int read = deflater.Read(buff, 0, buff.Length);

                while (read > 0)
                {
                    outputStream.Write(buff, 0, read);

                    read = deflater.Read(buff, 0, buff.Length);
                }

                return outputStream.ToArray();
            }
        }
    }
}
