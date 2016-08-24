using System.IO;

namespace Wave.Platform.Messaging
{
    public interface IFieldBase
    {
        short FieldID
        {
            get;
            set;
        }

        int DataLength
        {
            get;
        }

        int PackedLength
        {
            get;
        }

        int PackedSize
        {
            get;
        }

        void Pack(Stream str);
    }
}
