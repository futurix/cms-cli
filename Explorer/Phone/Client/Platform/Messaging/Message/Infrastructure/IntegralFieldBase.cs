using System.IO;

namespace Wave.Platform.Messaging
{
    public abstract class IntegralFieldBase : IFieldBase
    {
        public const short FieldHeaderSize = 2;

        public short FieldID
        {
            get { return fieldID; }
            set { fieldID = value; }
        }

        public abstract int DataLength
        {
            get;
        }

        public abstract int PackedLength
        {
            get;
        }

        public int PackedSize
        {
            get { return PackedLength + FieldHeaderSize; }
        }

        protected internal short fieldID;

        public IntegralFieldBase(short fID)
        {
            fieldID = fID;
        }

        public abstract void Pack(Stream str);

        public static void WriteFieldHeader(Stream str, FieldType fType, short fieldID)
        {
            int header = (((int)fType & 0x00F) << 4) | ((fieldID & 0x0F00) >> 8);

            str.WriteByte((byte)header);
            str.WriteByte((byte)(fieldID & 0x00FF));
        }
    }
}