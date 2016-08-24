using System;
using Wave.Common;
using Wave.Explorer;
using Wave.Network;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    /// <summary>
    /// Core is a static class that links together various Wave Explorer agents and routes Wave messages between them.
    /// </summary>
    public static class Core
    {
        // application-wide constant
        public const string ApplicationName = "Wave Explorer";

        // global settings
        public static WaveProtocolVersion ProtocolVersion = WaveProtocolVersion.Version3;
        public static WaveCSLVersion CSLVersion = WaveCSLVersion.Version3;
        
        // link to UI
        public static MainPage UI { get; private set; }
        public static bool HasUI { get { return (UI != null); } }

        // links to services
        public static AuthenticationAgent Authentication { get; private set; }
        public static CacheAgent Cache { get; private set; }
        public static DefinitionAgent Definitions { get; private set; }
        public static DownloadAgent Downloads { get; private set; }
        public static NavigationAgent Navigation { get; private set; }
        public static NetworkAgent Network { get; private set; }
        public static SettingsAgent Settings { get; private set; }
        public static SystemAgent System { get; private set; }
        public static UIAgent UIFactory { get; private set; }

        // state data
        public static WaveApplication Application { get; private set; }
        public static string BuildID { get; private set; }
        public static bool UseEncryption { get; private set; }
        public static bool IsMetro { get; private set; }

        // notification messages
        public static event EventHandler<EncryptionKeysChangeEventArgs> EncryptionKeysChanged;
        public static event EventHandler<AuthenticatedEventArgs> Authenticated;
        public static event EventHandler<SuccessfulLoginEventArgs> SuccessfulLogin;
        public static event DataEventHandler<SessionTerminationReasonCode> TerminateSession;
        public static event EventHandler TransportConnected;
        public static event EventHandler TransportDisconnected;
        public static event EventHandler ConnectionCompletelyFailed;
        public static event EventHandler<StreamResponseEventArgs> StreamResponseReceived;
        public static event EventHandler LocationChanged;
        public static event EventHandler LocationUnavailable;

        static Core()
        {
            // initialising variables
            UI = null;
            
            // creating all agents
            Settings = new SettingsAgent();
            System = new SystemAgent();
            UIFactory = new UIAgent();
            Cache = new CacheAgent();
            Downloads = new DownloadAgent();
            Authentication = new AuthenticationAgent();
            Navigation = new NavigationAgent();
            Definitions = new DefinitionAgent();
            Network = new NetworkAgent();
        }

        public static void Start(WaveApplication appInfo)
        {
            // saving application info
            Application = appInfo;

            // saving data from global App object
            BuildID = App.Instance.Build.BuildID;
            UseEncryption = !App.Instance.Build.PlatformOptions.Contains(PlatformOption.NoEncryption);
            IsMetro = App.Instance.Build.ClientOptions[ClientOption.MetroOptimisations].Equals(WaveConstant.True, StringComparison.InvariantCultureIgnoreCase);

            // initialising agents
            Settings.Start();
            Cache.Start();
            Network.Start();
        }

        public static void Resume()
        {
            // signalling agents that we resumed from dormant state
            Network.Resume();
        }

        public static void Suspend()
        {
            // signalling agents that we are about to go dormant
            Network.Suspend();
            Cache.Suspend();
            Settings.Suspend();
        }

        public static void End()
        {
            // signalling agents that we are about to exit
            Network.End();
            Cache.End();
            Settings.End();
        }

        public static bool UseRequestIDs
        {
            get { return ((CSLVersion == WaveCSLVersion.Version4) || (CSLVersion == WaveCSLVersion.Version5)); }
        }

        #region Messaging

        /// <summary>
        /// Routes server message to corresponding Wave Explorer service (if there is one).
        /// </summary>
        public static void RouteServerMessage(WaveMessage msg)
        {
            if (msg != null)
            {
                WaveServerComponent destination = (WaveServerComponent)msg.ApplicationID;
                Enum msgID = TransportBase.ConvertWaveMessageID(destination, msg.MessageID);

                DebugHelper.Trace("In: {0} -> {1}", destination, msgID);

                switch (destination)
                {
                    case WaveServerComponent.NavigationAgent:
                        Navigation.OnMessageReceived(destination, msgID, msg);
                        break;

                    case WaveServerComponent.DefinitionsAgent:
                        Definitions.OnMessageReceived(destination, msgID, msg);
                        break;

                    case WaveServerComponent.CacheAgent:
                        Cache.OnMessageReceived(destination, msgID, msg);
                        break;

                    case WaveServerComponent.UserManager:
                        Authentication.OnMessageReceived(destination, msgID, msg);
                        break;

                    case WaveServerComponent.AggregatedMessageAgent:
                        Network.OnMessageReceived(destination, msgID, msg);
                        break;

                    case WaveServerComponent.MediaAgent:
                        Downloads.OnMessageReceived(destination, msgID, msg);
                        break;

                    default:
                        DebugHelper.Out("Message for unsupported or unknown RTDS component {0} was discarded.", destination);
                        break;
                }
            }
        }

        /// <summary>
        /// Adds message to the transport queue - to be send whenever possible (depends on transport).
        /// </summary>
        /// <param name="destination">Server component to send message to.</param>
        /// <param name="msgID">Message ID (from enumeration of corresponding server component).</param>
        /// <param name="data">Data to be sent - as either ready WaveMessage or a FieldList.</param>
        /// <returns>Returns true if the transport is active and message was added to the queue.</returns>
        public static bool SendMessage(WaveServerComponent destination, Enum msgID, WaveMessage msg)
        {
            if (destination != WaveServerComponent.Unknown)
            {
                DebugHelper.Trace("Out: {0} -> {1}", destination, msgID);
                
                msg.ApplicationID = (int)destination;
                msg.MessageID = Convert.ToInt16(msgID);

                return Network.PostServerMessage(msg);
            }

            return false;
        }

        #endregion

        #region Event triggers

        // Following methods are simple triggers for various application-wide notification events.
        // They check event for presence of subscribers and create corresponding event arguments.

        public static void NotifyEncryptionKeysChanged(object sender, byte[] tr, byte[] st)
        {
            if (EncryptionKeysChanged != null)
                EncryptionKeysChanged(sender, new EncryptionKeysChangeEventArgs(tr, st));
        }

        public static void NotifyAuthentication(object sender, byte[] sid)
        {
            if (Authenticated != null)
                Authenticated(sender, new AuthenticatedEventArgs(sid));
        }

        public static void NotifySuccessfulLogin(object sender, int appID, string fullyQualifiedAppURI, string unqualifiedAppURI)
        {
            if (SuccessfulLogin != null)
                SuccessfulLogin(sender, new SuccessfulLoginEventArgs(appID, fullyQualifiedAppURI, unqualifiedAppURI));
        }

        public static void NotifyTerminateSession(object sender, SessionTerminationReasonCode code)
        {
            if (TerminateSession != null)
                TerminateSession(sender, new DataEventArgs<SessionTerminationReasonCode>(code));
        }

        public static void NotifyTransportConnect(object sender)
        {
            if (TransportConnected != null)
                TransportConnected(sender, EventArgs.Empty);
        }

        public static void NotifyTransportDisconnect(object sender)
        {
            if (TransportDisconnected != null)
                TransportDisconnected(sender, EventArgs.Empty);
        }

        public static void NotifyCompleteFailureOfConnection(object sender)
        {
            if (ConnectionCompletelyFailed != null)
                ConnectionCompletelyFailed(sender, EventArgs.Empty);
        }

        public static void NotifyStreamResponseReceived(object sender, byte[] cRefID, string streamURL, byte[] ssID, bool isNonMsg, short mType)
        {
            if (StreamResponseReceived != null)
                StreamResponseReceived(sender, new StreamResponseEventArgs(cRefID, streamURL, ssID, isNonMsg, mType));
        }

        public static void NotifyLocationChanged(object sender)
        {
            if (LocationChanged != null)
                LocationChanged(sender, EventArgs.Empty);
        }

        public static void NotifyLocationUnavailable(object sender)
        {
            if (LocationUnavailable != null)
                LocationUnavailable(sender, EventArgs.Empty);
        }

        /* add new triggers here */

        #endregion

        #region MainPage integration

        public static void RegisterPage(MainPage page)
        {
            UI = page;
        }

        public static void UnregisterPage(MainPage page)
        {
            UI = null;
        }

        #endregion
    }
}
