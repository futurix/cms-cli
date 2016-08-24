using System.IO;
using Wave.Common;

namespace Wave.Platform.Messaging
{
    public abstract class BinaryFieldBase : IntegralFieldBase
    {
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public override int DataLength
        {
            get
            {
                return data.Length;
            }
        }

        public override int PackedLength
        {
            get { return isLong ? data.Length + 3 : data.Length + 2; }
        }

        protected FieldType fieldType = FieldType.Undefined;
        protected bool isLong = false;

        private byte[] data;

        public BinaryFieldBase(short fID, byte[] value, FieldType ft, bool lg)
            : base(fID)
        {
            fieldType = ft;
            isLong = lg;
            
            data = value;
        }

        public BinaryFieldBase(short fID, Stream str, FieldType ft, bool lg)
            : base(fID)
        {
            fieldType = ft;
            isLong = lg;
            
            Unpack(str);
        }

        public override void Pack(Stream str)
        {
            IntegralFieldBase.WriteFieldHeader(str, fieldType, fieldID);

            if (isLong)
                str.WriteInteger24(data.Length);
            else
                str.WriteShort((short)data.Length);

            if ((data != null) && (data.Length > 0))
                str.Write(data, 0, data.Length);
        }

        public void Unpack(Stream str)
        {
            int length = isLong ? str.ReadInteger24() : str.ReadShort();

            if (length >= 0)
                data = str.ReadBytes(length);
            else
                data = null;
        }
    }
}
