using System;
using System.IO;
using Wave.Common;

namespace Wave.Platform.Messaging
{
    public class DateTimeField : IntegralFieldBase
    {
        public const int Size = 8;
        
        public DateTime Data
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

        private DateTime data;

        public DateTimeField(short fID, long value)
            : base(fID)
        {
            data = DataHelper.FileTimeToDateTime(value);
        }

        public DateTimeField(short fID, DateTime value)
            : base(fID)
        {
            data = value;
        }

        public DateTimeField(short fID, Stream str)
            : base(fID)
        {
            Unpack(str);
        }

        public override void Pack(Stream str)
        {
            IntegralFieldBase.WriteFieldHeader(str, FieldType.DateTime, FieldID);
            str.WriteLong(Data.ToFileTime());
        }

        public void Unpack(Stream str)
        {
            Data = DataHelper.FileTimeToDateTime(str.ReadLong());
        }

        public override string ToString()
        {
            return String.Format("Date-time: {0}", Data);
        }
    }
}