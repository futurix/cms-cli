using System;
using System.IO;

namespace Wave.Platform.Messaging
{
    public class ByteField : IntegralFieldBase
    {
        public const int Size = 1;
        
        public byte Data
        {
            get { return data; }
            set { data = value; }
        }

        public override int DataLength
        {
            get { return Size; }
        }

        public override int PackedLength
        {
            get { return Size; }
        }

        private byte data;

        public ByteField(short fID, byte value)
            : base(fID)
        {
            data = value;
        }

        public ByteField(short fID, Stream str)
            : base(fID)
        {
            Unpack(str);
        }

        public override void Pack(Stream str)
        {
            IntegralFieldBase.WriteFieldHeader(str, FieldType.Byte, FieldID);
            str.WriteByte(Data);
        }

        public void Unpack(Stream str)
        {
            data = (byte)str.ReadByte();
        }

        public override string ToString()
        {
            return String.Format("Byte: {0}", Data);
        }
    }
}