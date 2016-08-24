using System;
using System.IO;
using Wave.Common;

namespace Wave.Platform.Messaging
{
    public class DoubleField : IntegralFieldBase
    {
        public const int Size = 8;
        
        public double Data
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

        private double data;

        public DoubleField(short fID, double value)
            : base(fID)
        {
            data = value;
        }

        public DoubleField(short fID, Stream str)
            : base(fID)
        {
            Unpack(str);
        }

        public override void Pack(Stream str)
        {
            IntegralFieldBase.WriteFieldHeader(str, FieldType.Double, fieldID);
            str.WriteLong((long)data);
        }

        public void Unpack(Stream str)
        {
            data = str.ReadLong();
        }

        public override string ToString()
        {
            return String.Format("Double: {0}", Data);
        }
    }
}