using System;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public struct EntityID
    {
        public const int Size = 10;

        public byte[] Data { get; private set; }

        public CacheEntityType EntityType
        {
            get { return (Data != null) ? (CacheEntityType)BitConverter.ToInt16(Data, 0) : CacheEntityType.Definition; }
        }

        public int ApplicationID
        {
            get { return (Data != null) ? BitConverter.ToInt32(Data, 2) : 0; }
        }

        public int ItemID
        {
            get { return (Data != null) ? BitConverter.ToInt32(Data, 6) : 0; }
        }

        public EntityID(byte[] input)
            : this()
        {
            if ((input != null) && (input.Length == Size))
                Data = (byte[])input.Clone();
            else
                Data = null;
        }

        public EntityID(CacheEntityType et, int appID, int itemID)
            : this()
        {
            Data = new byte[Size];

            using (MemoryStream mem = new MemoryStream(Data))
            {
                mem.WriteShort((short)et);
                mem.WriteInteger(appID);
                mem.WriteInteger(itemID);
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", ApplicationID, ItemID);
        }

        public static implicit operator byte[](EntityID arg)
        {
            return arg.Data;
        }

        public static byte[] Create(CacheEntityType et, int appID, int itemID)
        {
            byte[] res = new byte[Size];

            using (MemoryStream mem = new MemoryStream(res))
            {
                mem.WriteShort((short)et);
                mem.WriteInteger(appID);
                mem.WriteInteger(itemID);
            }

            return res;
        }

        public static int GetApplicationID(byte[] source)
        {
            if ((source != null) && (source.Length == Size))
                return BitConverter.ToInt32(source, 2);

            return -1;
        }
    }
}
