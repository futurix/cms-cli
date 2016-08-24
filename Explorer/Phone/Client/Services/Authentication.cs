using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Wave.Common;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class AuthenticationAgent : IMessageEndpoint
    {
        public const string GeneratedUsernamePrefix = "_$";
        public const string DefaultPassword = "vcp";

        public bool Authenticated
        {
            get { return authenticated; }
        }
        
        public bool AuthenticatedEver
        {
            get { return authenticatedEver; }
        }

        private Blowfish sessionHandshakeFish = null;
        private InterlockedBool authenticated = false;
        private InterlockedBool authenticatedEver = false;

        public void OnMessageReceived(WaveServerComponent dest, Enum msgID, WaveMessage data)
        {
            if (msgID is UserManagerMessageID)
            {
                if (data != null)
                {
                    switch ((UserManagerMessageID)msgID)
                    {
                        case UserManagerMessageID.Challenge:
                            {
                                bool createAccount = false;

                                // check if there is login already (otherwise set defaults)
                                if (!Core.Application.HasLogin)
                                {
                                    createAccount = true;
                                    Core.Application.UpdateCredentials(GenerateNewUsername(), StringHelper.GetBytes(DefaultPassword));
                                }

                                // get CSL version
                                WaveCSLVersion serverCSL = (WaveCSLVersion)(data[UserManagerFieldID.CSLVersion].AsShort() ?? (short)WaveCSLVersion.Unknown);

                                switch (serverCSL)
                                {
                                    case WaveCSLVersion.Version5:
                                    case WaveCSLVersion.Version4:
                                        Core.CSLVersion = serverCSL; 
                                        break;
                                    
                                    default:
                                        Core.CSLVersion = WaveCSLVersion.Version3;
                                        break;
                                }

                                // get maximum protocol version
                                WaveProtocolVersion serverProto = (WaveProtocolVersion)(data[UserManagerFieldID.MaxWeMessageVersion].AsByte() ?? (byte)WaveProtocolVersion.Unknown);

                                switch (serverProto)
                                {
                                    case WaveProtocolVersion.Version4:
                                        Core.ProtocolVersion = WaveProtocolVersion.Version4;
                                        break;

                                    default:
                                        Core.ProtocolVersion = WaveProtocolVersion.Version3;
                                        break;
                                }

                                // get challenge
                                BinaryField challenge = (BinaryField)data[UserManagerFieldID.Challenge];

                                // assemble login message
                                WaveMessage msgOut = new WaveMessage();

                                msgOut.AddInt16(UserManagerFieldID.CSLVersion, (short)Core.CSLVersion);
                                msgOut.AddBoolean(UserManagerFieldID.EncryptSession, Core.UseEncryption);
                                msgOut.AddInt16(UserManagerFieldID.PriorityMask, NetworkAgent.PrioritiesActiveMask);
                                msgOut.AddBinary(UserManagerFieldID.UserCredentials, ProcessChallenge(challenge.Data, Core.Application, createAccount));
                                msgOut.AddBoolean(UserManagerFieldID.CreateAccount, createAccount);

                                // cache hash
                                byte[] cacheHash = Core.Cache.CacheHash;
                                msgOut.AddBinary(CacheAgentFieldID.CacheHashCompressed, (cacheHash.Length > 0) ? CompressionHelper.GZipBuffer(cacheHash) : cacheHash);

                                // cache ID (if any)
                                if (Core.Cache.CacheID.HasValue)
                                    msgOut.AddBinary(MessageOutFieldID.CacheItemID, Core.Cache.CacheID.Value.ToByteArray());
                                
                                // compiling device settings
                                FieldList deviceSettingsList = FieldList.CreateField(UserManagerFieldID.DeviceSettings);
                                deviceSettingsList.AddString(UserManagerFieldID.DeviceBuildID, Core.BuildID);
                                deviceSettingsList.AddBoolean(NaviAgentFieldID.DeviceSupportsTouch, true);
                                deviceSettingsList.AddInt16(NaviAgentFieldID.DeviceScreenResolutionWidth, 480);
                                deviceSettingsList.AddInt16(NaviAgentFieldID.DeviceScreenResolutionHeight, 800);

                                DeviceGroup[] devs = Core.System.SupportedDeviceGroups;

                                foreach (DeviceGroup dev in devs)
                                    deviceSettingsList.AddInt16(NaviAgentFieldID.DeviceProfileGroup, (short)dev);

                                deviceSettingsList.AddString(UserManagerFieldID.LanguageID, CultureInfo.CurrentCulture.Name);
                                deviceSettingsList.AddString(UserManagerFieldID.Timezone, Core.Settings.TimeZone);
                                deviceSettingsList.AddBoolean(UserManagerFieldID.AlphaSupport, true);
                                deviceSettingsList.AddBoolean(UserManagerFieldID.CompressionSupport, true);
                                msgOut.AddFieldList(deviceSettingsList);

                                // compiling application request list
                                FieldList appRequestList = FieldList.CreateField(UserManagerFieldID.ApplicationRequest);
                                appRequestList.AddString(NaviAgentFieldID.ApplicationURN, Core.Application.URI);
                                appRequestList.AddInt16(NaviAgentFieldID.ApplicationRequestAction, (short)AppRequestAction.GetAppEntryPage);
                                msgOut.AddFieldList(appRequestList);

                                msgOut.Send(WaveServerComponent.UserManager, UserManagerMessageID.Login);

                                Core.UI.SignalViewNavigationStart(Core.UI.RootViewID);
                                break;
                            }

                        case UserManagerMessageID.EncryptionKeys:
                            {
                                // setting encryption (if allowed by build configuration)
                                if (Core.UseEncryption && (sessionHandshakeFish != null))
                                {
                                    BinaryField sessionKeyField = (BinaryField)data[UserManagerFieldID.SessionKey];

                                    byte[] sessionKey = (byte[])sessionKeyField.Data;
                                    sessionHandshakeFish.Decrypt(sessionKey, sessionKey.Length);

                                    BinaryField globalServerKeyField = (BinaryField)data[UserManagerFieldID.GlobalServerKey];

                                    byte[] globalServerKey = (byte[])globalServerKeyField.Data;
                                    sessionHandshakeFish.Decrypt(globalServerKey, globalServerKey.Length);

                                    Core.NotifyEncryptionKeysChanged(this, sessionKey, globalServerKey);
                                }
                                else
                                    Core.NotifyEncryptionKeysChanged(this, null, null);
                                
                                // setting login data
                                StringField userName = (StringField)data[UserManagerFieldID.CreatedAccountUserName];

                                if (userName != null)
                                {
                                    BinaryField userPass = (BinaryField)data[UserManagerFieldID.CreatedAccountPasswordHash];

                                    if ((userPass != null) && (sessionHandshakeFish != null))
                                    {
                                        byte[] passBuffer = (byte[])userPass.Data.Clone();
                                        sessionHandshakeFish.Decrypt(passBuffer, passBuffer.Length);

                                        Core.Application.UpdateCredentials(userName.Data, passBuffer);

                                        // no longer needed
                                        sessionHandshakeFish = null;
                                    }
                                }

                                break;
                            }

                        case UserManagerMessageID.LoginResponse:
                            {
                                if (!authenticated)
                                {
                                    Int16Field loginStatus = (Int16Field)data[UserManagerFieldID.LoginStatus];

                                    switch ((UserLoginStatus)loginStatus.Data)
                                    {
                                        case UserLoginStatus.Success:
                                            {
                                                // signal authentication success
                                                Core.NotifyAuthentication(this, data[UserManagerFieldID.SessionInfo].AsByteArray());

                                                // preparing login data message
                                                FieldList appContext = (FieldList)data[UserManagerFieldID.ApplicationContext];

                                                if (appContext != null)
                                                {
                                                    int appID = appContext[MessageOutFieldID.ApplicationID].AsInteger() ?? 0;
                                                    string unqualifiedAppURI = appContext[UserManagerFieldID.ApplicationUnqualifiedURI].AsText();
                                                    string fullyQualifiedAppURI = appContext[UserManagerFieldID.ApplicationFullyQualifiedURI].AsText();

                                                    Core.NotifySuccessfulLogin(this, appID, unqualifiedAppURI, fullyQualifiedAppURI);
                                                }

                                                authenticated = true;
                                                authenticatedEver = true;
                                                break;
                                            }

                                        case UserLoginStatus.FailedInvalidCredentials:
                                            Core.NotifyTerminateSession(this, SessionTerminationReasonCode.InvalidCredentials);
                                            break;

                                        case UserLoginStatus.FailedNoUser:
                                            Core.NotifyTerminateSession(this, SessionTerminationReasonCode.NoSuchUser);
                                            break;
                                    }
                                }

                                break;
                            }
                    }
                }
            }
        }

        private byte[] ProcessChallenge(byte[] cha, WaveApplication app, bool createAccount)
        {
            byte[] user = null;
            byte[] password = null;
            byte[] passwordHash = null;
            byte[] encCha = (byte[])cha.Clone();

            if (app.HasLogin)
            {
                user = StringHelper.GetBytes(app.Login);
                password = (byte[])app.Password.Clone();
            }
            else
            {
                user = new byte[0];
                password = new byte[4];
            }

            // determine if the password should be hashed
            if (!createAccount && app.HasLogin && app.Login.StartsWith(GeneratedUsernamePrefix))
            {
                // no hashing in here
                passwordHash = (byte[])password.Clone();
            }
            else
            {
                // hash it
                using (SHA1Managed hasher = new SHA1Managed())
                {
                    passwordHash = hasher.ComputeHash(password);
                }
            }

            // encrypt the challenge
            sessionHandshakeFish = new Blowfish(passwordHash);
            sessionHandshakeFish.Encrypt(encCha, encCha.Length);

            // assembling result
            byte[] res = new byte[1 + user.Length + encCha.Length];
            res[0] = (byte)user.Length;

            for (int i = 0; i < user.Length; i++)
            {
                res[i + 1] = (byte)(user[i] ^ (i + 0xD1)); // simple XOR encryption
            }

            Array.Copy(encCha, 0, res, 1 + user.Length, encCha.Length);

            return res;
        }

        private string GenerateNewUsername()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(GeneratedUsernamePrefix);
            sb.Append(Convert.ToBase64String(Core.Settings.InstallationID.ToByteArray()));

            return sb.ToString();
        }
    }

    public class EncryptionKeysChangeEventArgs : EventArgs
    {
        public byte[] Transactional { get; private set; }
        public byte[] Streaming { get; private set; }

        public EncryptionKeysChangeEventArgs(byte[] tr, byte[] st)
        {
            Transactional = tr;
            Streaming = st;
        }
    }

    public class AuthenticatedEventArgs : EventArgs
    {
        public byte[] SessionID { get; private set; }

        public AuthenticatedEventArgs(byte[] sid)
        {
            SessionID = sid;
        }
    }

    public class SuccessfulLoginEventArgs : EventArgs
    {
        public int ApplicationID { get; private set; }
        public string FullyQualifiedApplicationURI { get; private set; }
        public string UnqualifiedApplicationURI { get; private set; }

        public SuccessfulLoginEventArgs(int appID, string fullyQualifiedAppURI, string unqualifiedAppURI)
        {
            ApplicationID = appID;
            FullyQualifiedApplicationURI = fullyQualifiedAppURI;
            UnqualifiedApplicationURI = unqualifiedAppURI;
        }
    }

    public enum SessionTerminationReasonCode
    {
        InvalidCredentials,
        NoSuchUser
    }
}
