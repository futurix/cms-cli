using System;
using System.Collections.Generic;
using Wave.Common;

namespace Wave.Platform.Messaging
{
    public class WaveMessageAssembler
    {
        public int ExpectedSize { get; private set; }

        public bool IsEmpty
        {
            get { return (buffer == null) || (buffer.Length == 0); }
        }

        public bool IsComplete
        {
            get { return (!IsEmpty && (ExpectedSize != 0) && (ExpectedSize <= buffer.Length)); }
        }

        private byte[] buffer = null;
        private bool hasHeader = false;

        public WaveMessageAssembler()
        {
            Reset();
        }

        public void Add(byte[] packet)
        {
            if ((packet == null) || (packet.Length == 0))
                return;
            
            if (buffer == null)
                buffer = (byte[])packet.Clone();
            else
                buffer = ByteArrayHelper.Combine(buffer, packet);

            Parse();
        }

        public void Parse()
        {
            if (IsEmpty)
                return;
            
            if (!hasHeader)
            {
                // determine protocol version
                WaveProtocolVersion version = WaveMessage.MagicNumberToProtocolVersion(buffer[0]);

                if (version != WaveProtocolVersion.Unknown)
                {
                    // check if we have entire header yet
                    int headerSize = 0;

                    switch (version)
                    {
                        case WaveProtocolVersion.Version2:
                        case WaveProtocolVersion.Version3:
                            headerSize = 7;
                            break;

                        case WaveProtocolVersion.Version4:
                            headerSize = 9;
                            break;
                    }

                    if ((headerSize != 0) && (buffer.Length >= headerSize))
                    {
                        // we have entire header, so let's determine entire message size
                        if (version == WaveProtocolVersion.Version4)
                        {
                            // message size is a 24 bit integer
                            byte[] temp = new byte[3] { buffer[1], buffer[2], buffer[3] };

                            ExpectedSize = DataHelper.BytesToInt24(temp);

                            if (ExpectedSize == -1)
                                ExpectedSize = 0; // 0 is our "undefined" value
                        }
                        else
                        {
                            // message size is a 16 bit integer
                            byte[] temp = new byte[2] { buffer[1], buffer[2] };
                            
                            ExpectedSize = BitConverter.ToInt16(temp, 0);
                        }

                        // do not repeat this again
                        hasHeader = true;
                    }
                }
            }
        }

        public void Trim()
        {
            if (IsComplete)
            {
                int remainder = buffer.Length - ExpectedSize;

                if (remainder > 0)
                {
                    byte[] rData = new byte[remainder];
                    Buffer.BlockCopy(buffer, ExpectedSize, rData, 0, rData.Length);

                    buffer = rData;
                    hasHeader = false;
                    ExpectedSize = 0;

                    Parse();
                }
                else
                {
                    buffer = null;
                    hasHeader = false;
                    ExpectedSize = 0;
                }
            }
        }

        public void Reset()
        {
            buffer = null;

            hasHeader = false;
            ExpectedSize = 0;
        }

        public WaveMessage ToWaveMessage()
        {
            if (IsComplete)
                return new WaveMessage(buffer);
            else
                return null;
        }

        public List<WaveMessage> ToWaveMessages()
        {
            if (IsComplete)
            {
                List<WaveMessage> res = new List<WaveMessage>();

                while (IsComplete)
                {
                    res.Add(new WaveMessage(buffer));
                    
                    Trim();
                }

                return res;
            }

            return null;
        }
    }
}
