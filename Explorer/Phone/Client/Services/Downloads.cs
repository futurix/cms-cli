using System;
using Wave.Common;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class DownloadAgent : IMessageEndpoint
    {
        public const short DefaultDownloadFrameSize = 20000;

        private DownloadManager downloads = new DownloadManager();

        public void OnMessageReceived(WaveServerComponent dest, Enum msgID, WaveMessage data)
        {
            if (msgID is MediaAgentMessageID)
            {
                if (data != null)
                {
                    switch ((MediaAgentMessageID)msgID)
                    {
                        case MediaAgentMessageID.DownloadResponse:
                            {
                                // extract useful data
                                byte[] ciidData = data[MessageOutFieldID.CacheItemID].AsByteArray();
                                byte[] cRefID = data[MessageOutFieldID.ContentReferenceID].AsByteArray();
                                byte cacheHint = (byte)CacheHint.PersistStore_ShouldCache;

                                if (ciidData != null)
                                    cacheHint = data[MessageOutFieldID.CacheHint].AsByte() ?? (byte)CacheHint.PersistStore_ShouldCache;

                                int downloadSize = data[MessageOutFieldID.DownloadSize].AsInteger() ?? -1; // only sent for multi-frame downloads

                                if (downloadSize != -1)
                                {
                                    // first frame of a multi-frame download
                                    byte[] binary = data[MediaReferenceFieldID.BinaryFrame].AsByteArray();

                                    if (binary != null)
                                        downloads.Start(ciidData, cRefID, binary, downloadSize, cacheHint);

                                    // ack the frame
                                    SendFrameAcknowledgement(ciidData, cRefID, 0);
                                }
                                else
                                {
                                    // single frame download
                                    byte[] binary = data[MediaReferenceFieldID.BinaryFrame].AsByteArray();

                                    if (binary != null)
                                    {
                                        // act on download
                                        OnDownloadComplete(ciidData, cRefID, binary, cacheHint);

                                        // ack the frame
                                        SendFrameAcknowledgement(ciidData, cRefID, 0);
                                    }
                                }

                                break;
                            }

                        case MediaAgentMessageID.Frame:
                            {
                                short frameNumber = data[MediaReferenceFieldID.FrameNumber].AsShort() ?? -1;

                                if (frameNumber != -1)
                                {
                                    byte[] ciidData = data[MessageOutFieldID.CacheItemID].AsByteArray();
                                    byte[] crefID = data[MessageOutFieldID.ContentReferenceID].AsByteArray();

                                    if ((crefID != null) || (ciidData != null))
                                    {
                                        //IMPLEMENT: support for bulk downloads

                                        byte[] binary = data[MediaReferenceFieldID.BinaryFrame].AsByteArray();

                                        if (binary != null)
                                        {
                                            downloads.Add(ciidData, crefID, binary);
                                            
                                            // check if complete
                                            if (downloads.IsComplete(ciidData, crefID))
                                            {
                                                DownloadManager.DownloadRecord dr = downloads.Retrieve(ciidData, crefID);

                                                if (dr != null)
                                                    OnDownloadComplete(dr.CacheItemID, dr.ContentRefenceID, dr.Buffer, dr.CacheHint);
                                            }

                                            // ack the frame
                                            SendFrameAcknowledgement(ciidData, crefID, frameNumber);
                                        }
                                    }
                                }

                                break;
                            }

                        case MediaAgentMessageID.OpenStreamResponse:
                            {
                                byte[] contentID = data[MessageOutFieldID.ContentReferenceID].AsByteArray(); // content reference
                                string streamURL = data[MediaReferenceFieldID.StreamURL].AsString();
                                byte[] streamSessionID = data[MediaReferenceFieldID.StreamSessionID].AsByteArray();

                                // for a stream switch between streams where 1 is not an MSG stream the player must be torn down and recreated.
                                // only MSG supports the "fast stream switch"
                                bool nonMsgStream = data.RootList.HasField(MediaReferenceFieldID.NonMSGStream);
                                short mediaType = data[MessageOutFieldID.MediaPrimitiveType].AsShort() ?? -1;

                                Core.NotifyStreamResponseReceived(this, contentID, streamURL, streamSessionID, nonMsgStream, mediaType);
                                break;
                            }
                    }
                }
            }
        }

        public void OpenMediaStream(string contentID, byte[] streamSessionID = null)
        {
            WaveMessage msg = new WaveMessage();

            msg.AddBinary(MessageOutFieldID.ContentReferenceID, StringHelper.GetBytes(contentID));

            if (streamSessionID != null)
                msg.AddBinary(MediaReferenceFieldID.StreamSessionID, streamSessionID);

            msg.Send(WaveServerComponent.MediaAgent, MediaAgentMessageID.OpenStream);
        }

        private void OnDownloadComplete(byte[] ciid, byte[] cRefID, byte[] buffer, int cacheHint)
        {
            // maybe cache - consider hint
            if (Core.Cache.ShouldCache((byte)cacheHint) && (ciid != null))
                Core.Cache.Server.Add(buffer, new CacheItemID(ciid), cRefID, CacheEntityType.Content, (byte)cacheHint, true);
            
            // notify current node
            Core.UIFactory.SignalDownloadReady(cRefID, buffer);
        }

        private void SendFrameAcknowledgement(byte[] ciid, byte[] cRefID, short frameNumber)
        {
            WaveMessage msg = new WaveMessage();

            if (ciid != null)
                msg.AddBinary(MessageOutFieldID.CacheItemID, ciid);
            else
                msg.AddBinary(MessageOutFieldID.ContentReferenceID, cRefID);

            msg.AddInt16(MediaReferenceFieldID.FrameNumber, frameNumber);

            msg.Send(WaveServerComponent.MediaAgent, MediaAgentMessageID.FrameAcknowledgement);
        }
    }

    public class StreamResponseEventArgs : EventArgs
    {
        public byte[] ContentReferenceID { get; private set; }
        public string StreamURL { get; private set; }
        public byte[] StreamSessionID { get; private set; }

        public bool IsNonMsgStream { get; private set; }
        public short MediaType { get; private set; }

        public StreamResponseEventArgs(byte[] cRefID, string streamURL, byte[] ssID, bool isNonMsg, short mType)
        {
            ContentReferenceID = cRefID;
            StreamURL = streamURL;
            StreamSessionID = ssID;

            IsNonMsgStream = isNonMsg;
            MediaType = mType;
        }
    }
}
