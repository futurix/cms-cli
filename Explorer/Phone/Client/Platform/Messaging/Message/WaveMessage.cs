using System;
using System.IO;
using Wave.Common;
using Wave.Services;

namespace Wave.Platform.Messaging
{
    public class WaveMessage
    {
        public const short MagicNumberV2 = 0x27;
        public const short MagicNumberV3 = 0x37;
        public const short MagicNumberV4 = 0x49;
        
        public const int HeaderSizeV3 = 7;
        public const int HeaderSizeV4 = 9;

        public const int CompressedDataFlag = 1;

        public FieldList RootList
        {
            get { return rootList; }
            set { rootList = value; }
        }

        public int ApplicationID
        {
            get { return applicationID; }
            set { applicationID = value; }
        }

        public short MessageID
        {
            get { return messageID; }
            set { messageID = value; }
        }

        public WaveProtocolVersion ProtocolVersion
        {
            get { return protocolVersion; }
            set { protocolVersion = value; }
        }

        private int applicationID = -1;
        private short messageID = -1;
        private FieldList rootList = null;
        private int messageSize = 0;

        private WaveProtocolVersion protocolVersion = Core.ProtocolVersion;

        public WaveMessage()
        {
            rootList = new FieldList();
        }

        public WaveMessage(Stream str)
        {
            InitialiseFromStream(str);
        }

        public WaveMessage(byte[] buffer)
        {
            using (MemoryStream str = new MemoryStream(buffer))
                InitialiseFromStream(str);
        }

        public WaveMessage(int appID, short msgID)
        {
            rootList = new FieldList();
            
            applicationID = appID;
            messageID = msgID;
        }

        public IFieldBase this[short fID]
        {
            get { return (rootList != null) ? rootList[fID] : null; }
        }

        public IFieldBase this[Enum fID]
        {
            get { return this[Convert.ToInt16(fID)]; }
        }

        private void InitialiseFromStream(Stream str)
        {
            protocolVersion = MagicNumberToProtocolVersion((short)str.ReadByte());

            if (protocolVersion == WaveProtocolVersion.Version4)
                messageSize = str.ReadInteger24();
            else
                messageSize = str.ReadShort();

            applicationID = (byte)str.ReadByte();
            messageID = (short)str.ReadByte();

            short numberOfFields = str.ReadShort();
            byte extras = 0;
            int extendedHeaderLength = 0;
            bool isCompressed = false;

            if (protocolVersion == WaveProtocolVersion.Version4)
                extras = (byte)str.ReadByte();

            isCompressed = ((extras & CompressedDataFlag) == CompressedDataFlag);
            extendedHeaderLength = extras >> 4;

            if (extendedHeaderLength > 0)
                str.Seek(extendedHeaderLength, SeekOrigin.Current);

            if (isCompressed)
            {
                // get size of data when uncompressed
                int uncompressedSize = str.ReadInteger24();

                // calculate size of compressed data
                int compressedSize = messageSize;

                if (protocolVersion == WaveProtocolVersion.Version4)
                    compressedSize -= HeaderSizeV4;
                else
                    compressedSize -= HeaderSizeV3;

                compressedSize -= extendedHeaderLength;
                compressedSize -= 3; // size of uncompressedSize
                
                // uncompress
                byte[] compressedData = str.ReadBytes(compressedSize);

                if (compressedData != null)
                {
                    byte[] uncompressedMessage = CompressionHelper.DeflateBuffer(compressedData);

                    using (MemoryStream mem = new MemoryStream(uncompressedMessage))
                        rootList = new FieldList(mem, numberOfFields);
                }

            }
            else
                rootList = new FieldList(str, numberOfFields);
        }

        private void GenerateHeaders(Stream str)
        {
            // magic number
            str.WriteByte((byte)ProtocolVersionToMagicNumber(protocolVersion));

            // length
            int length = rootList.DataPackedLength;

            if (protocolVersion == WaveProtocolVersion.Version4)
                length += HeaderSizeV4;
            else
                length += HeaderSizeV3;

            // size
            if (protocolVersion == WaveProtocolVersion.Version4)
                str.WriteInteger24(length);
            else
                str.WriteShort((short)length);

            // application ID
            str.WriteByte((byte)applicationID);

            // message type
            str.WriteByte((byte)MessageID);

            // number of fields in the message
            str.WriteShort((short)rootList.Count);

            // extra flags
            if (protocolVersion == WaveProtocolVersion.Version4)
                str.WriteByte(0);
        }

        public void EncodeMessage(Stream str)
        {
            GenerateHeaders(str);
            rootList.EncodeFields(str);
        }

        #region Shortcuts

        public void Add(IFieldBase field)
        {
            rootList.Add(field);
        }

        public void AddBinary(short fID, byte[] value)
        {
            rootList.Add(new BinaryField(fID, value));
        }

        public void AddBinary(Enum fID, byte[] value)
        {
            rootList.Add(new BinaryField(Convert.ToInt16(fID), value));
        }

        public void AddBoolean(short fID, bool value)
        {
            rootList.Add(new BooleanField(fID, value));
        }

        public void AddBoolean(Enum fID, bool value)
        {
            rootList.Add(new BooleanField(Convert.ToInt16(fID), value));
        }

        public void AddByte(short fID, byte value)
        {
            rootList.Add(new ByteField(fID, value));
        }

        public void AddByte(Enum fID, byte value)
        {
            rootList.Add(new ByteField(Convert.ToInt16(fID), value));
        }

        public void AddDateTime(short fID, DateTime value)
        {
            rootList.Add(new DateTimeField(fID, value));
        }

        public void AddDateTime(Enum fID, DateTime value)
        {
            rootList.Add(new DateTimeField(Convert.ToInt16(fID), value));
        }

        public void AddDateTime(short fID, long value)
        {
            rootList.Add(new DateTimeField(fID, value));
        }

        public void AddDateTime(Enum fID, long value)
        {
            rootList.Add(new DateTimeField(Convert.ToInt16(fID), value));
        }

        public void AddInt16(short fID, short value)
        {
            rootList.Add(new Int16Field(fID, value));
        }

        public void AddInt16(Enum fID, short value)
        {
            rootList.Add(new Int16Field(Convert.ToInt16(fID), value));
        }

        public void AddInt32(short fID, int value)
        {
            rootList.Add(new Int32Field(fID, value));
        }

        public void AddInt32(Enum fID, int value)
        {
            rootList.Add(new Int32Field(Convert.ToInt16(fID), value));
        }

        public void AddLongBinary(short fID, byte[] value)
        {
            rootList.Add(new LongBinaryField(fID, value));
        }

        public void AddLongBinary(Enum fID, byte[] value)
        {
            rootList.Add(new LongBinaryField(Convert.ToInt16(fID), value));
        }

        public void AddString(short fID, string value)
        {
            rootList.Add(new StringField(fID, value));
        }

        public void AddString(Enum fID, string value)
        {
            rootList.Add(new StringField(Convert.ToInt16(fID), value));
        }

        public void AddFieldList(FieldList list)
        {
            rootList.Add(list);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Just a helper to avoid the creation of ByteArrays.
        /// </summary>
        public byte[] ToEncodedByteArray()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                EncodeMessage(stream);

                return stream.ToArray();
            }
        }

        public override string ToString()
        {
            return String.Format(
                "Message: {0} fields, protocol {1}",
                (RootList != null) ? RootList.Count : 0,
                WaveMessage.ProtocolVersionToInfoString(ProtocolVersion));
        }

        #endregion

        #region Static helper methods

        public static WaveProtocolVersion MagicNumberToProtocolVersion(short magic)
        {
            switch (magic)
            {
                case WaveMessage.MagicNumberV2:
                    return WaveProtocolVersion.Version2;
                
                case WaveMessage.MagicNumberV3:
                    return WaveProtocolVersion.Version3;

                case WaveMessage.MagicNumberV4:
                    return WaveProtocolVersion.Version4;

                default:
                    return WaveProtocolVersion.Unknown;
            }
        }

        public static short ProtocolVersionToMagicNumber(WaveProtocolVersion protocolVer)
        {
            switch (protocolVer)
            {
                case WaveProtocolVersion.Version2:
                    return WaveMessage.MagicNumberV2;

                case WaveProtocolVersion.Version3:
                    return WaveMessage.MagicNumberV3;

                case WaveProtocolVersion.Version4:
                    return WaveMessage.MagicNumberV4;

                default:
                    return 0;
            }
        }

        public static string ProtocolVersionToInfoString(WaveProtocolVersion protocolVer)
        {
            switch (protocolVer)
            {
                case WaveProtocolVersion.Version2:
                    return "v2";

                case WaveProtocolVersion.Version3:
                    return "v3";

                case WaveProtocolVersion.Version4:
                    return "v4";

                case WaveProtocolVersion.Unknown:
                default:
                    return "unknown";
            }
        }

        #endregion
    }

    public enum WaveProtocolVersion : short
    {
        Unknown = 0,

        Version2 = 2,
        Version3 = 3,
        Version4 = 4
    }

    public enum WaveCSLVersion : short
    {
        Unknown = 0,

        Version3 = 3,
        Version4 = 4,
        Version5 = 5
    }
}