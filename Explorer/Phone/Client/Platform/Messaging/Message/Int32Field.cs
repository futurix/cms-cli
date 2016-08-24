using System;
using System.IO;
using Wave.Common;

namespace Wave.Platform.Messaging
{
    public class Int32Field : IntegralFieldBase
    {
        public const int Size = 4;
        
        public int Data
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

        private int data;

        public Int32Field(short fID, int value)
            : base(fID)
        {
            data = value;
        }

        public Int32Field(short fID, Stream str)
            : base(fID)
        {
            Unpack(str);
        }

        public override void Pack(Stream str)
        {
            IntegralFieldBase.WriteFieldHeader(str, FieldType.Int32, fieldID);
            str.WriteInteger(data);
        }

        public void Unpack(Stream str)
        {
            data = str.ReadInteger();
        }

        public override string ToString()
        {
            return String.Format("Int32: {0}", Data);
        }
    }
}