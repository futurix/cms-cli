using System;
using System.IO;

namespace Wave.Platform.Messaging
{
    public class LongBinaryField : BinaryFieldBase
    {
        public LongBinaryField(short fID, byte[] value)
            : base(fID, value, FieldType.LongBinary, true)
        {
        }

        public LongBinaryField(short fID, Stream str)
            : base(fID, str, FieldType.LongBinary, true)
        {
        }

        public override string ToString()
        {
            return String.Format("Long binary: {0} bytes", (Data != null) ? Data.Length : 0);
        }
    }
}
