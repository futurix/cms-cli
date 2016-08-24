using System;
using System.IO;

namespace Wave.Platform.Messaging
{
    public class BinaryField : BinaryFieldBase
    {
        public BinaryField(short fID, byte[] value)
            : base(fID, value, FieldType.Binary, false)
        {
        }

        public BinaryField(short fID, Stream str)
            : base(fID, str, FieldType.Binary, false)
        {
        }

        public override string ToString()
        {
            return String.Format("Binary: {0} bytes", (Data != null) ? Data.Length : 0);
        }
    }
}