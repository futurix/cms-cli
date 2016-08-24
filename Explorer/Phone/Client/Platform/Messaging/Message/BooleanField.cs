using System;
using System.IO;

namespace Wave.Platform.Messaging
{
    public class BooleanField : IntegralFieldBase
    {
        public const int Size = 1;
        
        public bool Data
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

        private bool data;

        public BooleanField(short fID, bool value)
            : base(fID)
        {
            data = value;
        }

        public BooleanField(short fID, byte value)
            : base(fID)
        {
            data = (Convert.ToInt32(value) != 0);
        }

        public BooleanField(short fID, Stream str)
            : base(fID)
        {
            Unpack(str);
        }

        public override void Pack(Stream str)
        {
            IntegralFieldBase.WriteFieldHeader(str, FieldType.Bool, FieldID);

            if (data)
                str.WriteByte(1);
            else
                str.WriteByte(0);
        }

        public void Unpack(Stream str)
        {
            data = ((byte)str.ReadByte() != 0);
        }

        public override string ToString()
        {
            return String.Format("Boolean: {0}", Data);
        }
    }
}