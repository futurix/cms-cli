using System;
using System.Diagnostics;
using System.IO;
using Wave.Common;

namespace Wave.Platform.Messaging
{
    public class StringField : IntegralFieldBase
    {
        public string Data
        {
            get { return data; }
            set { data = value; }
        }

        private string data;

        public StringField(short fID, string value)
            : base(fID)
        {
            data = value;
        }

        public StringField(short fID, char[] value)
            : base(fID)
        {
            data = new string(value);
        }

        public StringField(short fID, byte[] value)
            : base(fID)
        {
            data = StringHelper.GetString(value);
        }

        public StringField(short fID, Stream str)
            : base(fID)
        {
            Unpack(str);
        }

        public byte[] DataAsBytes
        {
            get { return StringHelper.GetBytes(data); }
            set { data = StringHelper.GetString(value); }
        }

        public override int DataLength
        {
            get
            {
                int res = 0;

                if (Data != null)
                {
                    try
                    {
                        res = StringHelper.GetByteCount(data);
                    }
                    catch (IOException ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                return res;
            }
        }

        public override int PackedLength
        {
            get
            {
                int res = 0;

                try
                {
                    if (Data != null)
                        res = StringHelper.GetByteCount(data) + 2;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                return res;
            }
        }

        public override void Pack(Stream str)
        {
            IntegralFieldBase.WriteFieldHeader(str, FieldType.String, fieldID);

            byte[] tempArr = StringHelper.GetBytes(data);

            if (tempArr != null)
            {
                str.WriteShort((short)tempArr.Length);
                str.Write(tempArr, 0, tempArr.Length);
            }
            else
                str.WriteShort(0);
        }

        public void Unpack(Stream str)
        {
            short length = str.ReadShort();

            if (length > 0)
                data = StringHelper.GetString(str.ReadBytes(length));
            else if (length == 0)
                data = String.Empty;
            else
                data = null;
        }

        public override string ToString()
        {
            return String.Format("String: {0}", Data);
        }
    }
}