using System;
using System.IO;
using Wave.Common;

namespace Wave.Platform.Messaging
{
    public class Int16Field : IntegralFieldBase
    {
        public const int Size = 2;
        
        public short Data
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

        private short data;

        public Int16Field(short fID, short value)
            : base(fID)
        {
            data = value;
        }

        public Int16Field(short fID, Stream str)
            : base(fID)
        {
            Unpack(str);
        }

        public override void Pack(Stream str)
        {
            IntegralFieldBase.WriteFieldHeader(str, FieldType.Int16, FieldID);
            str.WriteShort(data);
        }

        public void Unpack(Stream str)
        {
            data = str.ReadShort();
        }

        public override string ToString()
        {
            return String.Format("Int16: {0}", Data);
        }
    }
}