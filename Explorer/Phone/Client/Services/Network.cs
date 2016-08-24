using System;
using Wave.Common;
using Wave.Network;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class NetworkAgent : IMessageEndpoint
    {
        /// <summary>
        /// Bit mask for supported session priorities: 001001 as a binary mask, 9 in decimal.
        /// </summary>
        public const short PrioritiesActiveMask = 9;

        public bool IsConnected
        {
            get { return (transport != null) ? transport.IsConnected : false; }
        }

        public bool IsStalled
        {
            get { return tracker.IsStalled; }
        }

        // encryption objects
        public Blowfish TransactionalEncryption { get; private set; }
        public Blowfish StreamingEncryption { get; private set; }

        private TransportTracker tracker = null;
        private TransportBase transport = null;

        private NetworkAgentControl serviceControl = null;

        public NetworkAgent()
        {
            serviceControl = new NetworkAgentControl(this);
            tracker = new TransportTracker(serviceControl);
        }

        public void Start()
        {
            // subscribe to global notifications
            Core.EncryptionKeysChanged += new EventHandler<EncryptionKeysChangeEventArgs>(User_EncryptionKeysChanged);
            Core.Authenticated += new EventHandler<AuthenticatedEventArgs>(User_Authenticated);

            // launch transport
            StartTransportAnew();
        }

        public void Resume()
        {
            if (transport != null)
                transport.Connect();
        }

        public void Suspend()
        {
            if (transport != null)
                transport.Suspend();
        }

        public void End()
        {
            StopTransportAndReset();
        }

        public void StartTransportAnew()
        {
            // reset tracker (just in case)
            tracker.Reset();

            // get transport directions
            TransportTrackerResult tr = tracker.GetNext();

            if ((tr.Action == TransportTrackerAction.LoadNewTransport) && (tr.Transport != null))
                StartTransport(tr.Transport);
        }

        private void StartTransport(TransportBase ts)
        {
            StopTransport();
            
            if (ts != null)
            {
                transport = ts;

                transport.DataReceived += new DataEventHandler<object>(transport_DataReceived);
                transport.Error += new DataEventHandler<TransportErrorCode>(transport_Error);
                transport.StateChanged += new EventHandler<TransportStateChangedEventArgs>(transport_StateChanged);

                transport.Open();
            }
        }

        private void RestartExistingTransport()
        {
            if (transport != null)
            {
                transport.Suspend();
                transport.Connect();
            }
        }

        private void StopTransport()
        {
            if (transport != null)
            {
                transport.DataReceived -= transport_DataReceived;
                transport.Error -= transport_Error;
                transport.StateChanged -= transport_StateChanged;
                
                transport.Close();
                transport = null;
            }
        }

        private void StopTransportAndReset()
        {
            StopTransport();

            tracker.Reset();
        }

        #region Public control interface

        public void EnsureConnection()
        {
            if (transport != null)
            {
                if (!transport.IsConnected)
                    transport.Connect();
            }
        }

        public void ForceConnection()
        {
            if (!IsConnected)
            {
                StopTransportAndReset();
                StartTransportAnew();
            }
        }

        public void ForceDisconnect()
        {
            StopTransportAndReset();
        }

        #endregion

        private void User_EncryptionKeysChanged(object sender, EncryptionKeysChangeEventArgs e)
        {
            if ((e.Transactional != null) && (e.Transactional.Length > 0) && (e.Streaming != null) && (e.Streaming.Length > 0))
            {
                TransactionalEncryption = new Blowfish(e.Transactional);
                StreamingEncryption = new Blowfish(e.Streaming);
            }
            else
            {
                TransactionalEncryption = null;
                StreamingEncryption = null;
            }
        }

        private void User_Authenticated(object sender, AuthenticatedEventArgs e)
        {
            if (transport != null)
                transport.SetSessionID(e.SessionID);
        }

        private void transport_DataReceived(object sender, DataEventArgs<object> e)
        {
            Core.RouteServerMessage(e.Data as WaveMessage);
        }

        private void transport_Error(object sender, DataEventArgs<TransportErrorCode> e)
        {
            switch (e.Data)
            {
                case TransportErrorCode.NoConnectionData:
                    {
                        // do nothing; wait for new application data
                        break;
                    }

                case TransportErrorCode.LostConnection:
                    {
                        TransportTrackerResult tr = tracker.GetNext();

                        switch (tr.Action)
                        {
                            case TransportTrackerAction.RetryExistingTransport:
                                RestartExistingTransport();
                                break;

                            case TransportTrackerAction.LoadNewTransport:
                                {
                                    if (tr.Transport != null)
                                        StartTransport(tr.Transport);
                                    
                                    break;
                                }

                            case TransportTrackerAction.ItIsAllOver:
                                StopTransport();
                                Core.NotifyCompleteFailureOfConnection(this);
                                break;
                        }
                        
                        break;
                    }

                case TransportErrorCode.ReconnectRequired:
                    {
                        // unconditional restart (does not decrease internal counters)
                        RestartExistingTransport();
                        break;
                    }

                case TransportErrorCode.DisconnectRequired:
                    {
                        StopTransport();
                        break;
                    }
            }
        }

        private void transport_StateChanged(object sender, TransportStateChangedEventArgs e)
        {
            if (e.New != e.Old)
            {
                if (e.New == TransportState.Connected)
                    Core.NotifyTransportConnect(this);
                else if (e.New == TransportState.Disconnected)
                    Core.NotifyTransportDisconnect(this);
            }
        }

        public void OnMessageReceived(WaveServerComponent dest, Enum msgID, WaveMessage data)
        {
            if (msgID is AggregateMessageMessageID)
            {
                if (data != null)
                {
                    if ((AggregateMessageMessageID)msgID == AggregateMessageMessageID.AggregateMessage)
                    {
                        foreach (IFieldBase field in data.RootList)
                        {
                            if ((field is BinaryField) && (field.FieldID == (short)AggregateMessageFieldID.PackedMessage))
                            {
                                // we have embedded message - lets unpack and publish that
                                BinaryField tempBinary = (BinaryField)field;

                                if ((tempBinary.Data != null) && (tempBinary.Data.Length > 0))
                                    Core.RouteServerMessage(new WaveMessage(tempBinary.Data));
                            }
                        }
                    }
                }
            }
        }

        public bool PostServerMessage(WaveMessage msg)
        {
            if ((msg == null) || (transport == null))
                return false;

            transport.SendData(msg.ToEncodedByteArray());

            return true;
        }

        private void ResetEncryption()
        {
            Core.NotifyEncryptionKeysChanged(this, null, null);
        }

        #region Private control interface

        public interface INetworkAgentControl
        {
            void ResetEncryption();
        }

        private class NetworkAgentControl : INetworkAgentControl
        {
            private NetworkAgent service = null;
            
            public NetworkAgentControl(NetworkAgent svc)
            {
                service = svc;
            }

            void INetworkAgentControl.ResetEncryption()
            {
                service.ResetEncryption();
            }
        }

        #endregion
    }
}
