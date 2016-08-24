using System;
using System.IO;
using System.Xml.Serialization;

namespace Wave.Common
{
    public static class XmlHelper
    {
        public static bool SerializeObject(Stream output, object obj)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(obj.GetType());
                xs.Serialize(output, obj);

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public static object DeserializeObject(Stream input, Type objectType)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(objectType);
                return xs.Deserialize(input);
            }
            catch
            {
                return null;
            }
        }
    }

    public static class LazySerializerHelper
    {
        public static byte[] Serialize(object obj, Type objectType)
        {
            try
            {
                using (MemoryStream mem = new MemoryStream())
                {
                    XmlSerializer xs = new XmlSerializer(objectType);
                    xs.Serialize(mem, obj);

                    return mem.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        public static object Deserialize(byte[] input, Type objectType)
        {
            try
            {
                using (MemoryStream mem = new MemoryStream(input))
                {
                    XmlSerializer xs = new XmlSerializer(objectType);
                    return xs.Deserialize(mem);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
