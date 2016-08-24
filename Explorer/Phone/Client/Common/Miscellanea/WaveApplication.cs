using System;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Wave.Common
{
    public class WaveApplication : IXmlSerializable
    {
        public int ID { get; private set; }

        public string Name { get; private set; }
        public string URI { get; private set; }

        public ProtocolData UDP { get; private set; }
        public ProtocolData TCP { get; private set; }
        public ProtocolData HTTP { get; private set; }

        public string Login { get; private set; }
        public byte[] Password { get; private set; }

        #region Utility properties

        public bool HasUDP { get { return UDP.IsValid; } }
        public bool HasTCP { get { return TCP.IsValid; } }
        public bool HasHTTP { get { return HTTP.IsValid; } }

        public string PasswordString
        {
            get { return StringHelper.GetString(Password) ?? String.Empty; }
        }

        public bool IsValid
        {
            get { return (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(URI) && (HasUDP || HasTCP || HasHTTP)); }
        }

        public bool HasLogin
        {
            get { return (!String.IsNullOrEmpty(Login) && (Password != null) && (Password.Length > 0)); }
        }

        public string NameLC
        {
            get { return String.IsNullOrEmpty(Name) ? String.Empty : Name.ToLowerInvariant(); }
        }

        #endregion

        #region Modification tracking

        private bool credentialsModified = false;
        private bool tcpPortModified = false;

        #endregion

        public WaveApplication()
        {
        }

        public void UpdateCredentials(string userName, byte[] userPassword)
        {
            if (!String.IsNullOrWhiteSpace(userName) && (userPassword != null) && 
                !(Login.Equals(userName) && ByteArrayHelper.IsEqual(Password, userPassword)))
            {
                credentialsModified = true;

                Login = userName;
                Password = (byte[])userPassword.Clone();
            }
        }

        public void UpdateTCPPort(int newPort)
        {
            if (newPort > 0)
            {
                tcpPortModified = true;

                TCP = new ProtocolData() { Host = TCP.Host, Port = newPort };
            }
        }

        public void ApplyOverrides(WaveApplicationOverrides overrides)
        {
            if ((overrides != null) && overrides.TargetID.HasValue && (overrides.TargetID.Value == ID))
            {
                UpdateCredentials(overrides.Login, overrides.Password);

                if (overrides.TcpPort.HasValue)
                    UpdateTCPPort(overrides.TcpPort.Value);
            }
        }

        public WaveApplicationOverrides ToOverrides()
        {
            if (credentialsModified || tcpPortModified)
            {
                return new WaveApplicationOverrides(
                    ID,
                    credentialsModified ? Login : null,
                    credentialsModified ? Password : null,
                    tcpPortModified ? TCP.Port : (int?)null
                );
            }

            return null;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Name, URI);
        }

        #region IXmlSerializable implementation

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                ID = DataHelper.StringToInt(reader["ID"] ?? String.Empty, 0);

                Name = reader["Name"] ?? String.Empty;
                URI = reader["URI"] ?? String.Empty;

                UDP = new ProtocolData(reader["UDP"]);
                TCP = new ProtocolData(reader["TCP"]);
                HTTP = new ProtocolData(reader["HTTP"]);

                Login = reader["Login"] ?? String.Empty;
                Password = StringHelper.GetBytes(reader["Password"] ?? String.Empty);
            }

            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("ID", ID.ToString());

            writer.WriteAttributeString("Name", Name ?? String.Empty);
            writer.WriteAttributeString("URI", URI ?? String.Empty);

            writer.WriteAttributeString("UDP", UDP.ToString());
            writer.WriteAttributeString("TCP", TCP.ToString());
            writer.WriteAttributeString("HTTP", HTTP.ToString());
            
            writer.WriteAttributeString("Login", Login ?? String.Empty);
            writer.WriteAttributeString("Password", StringHelper.GetString(Password) ?? String.Empty);
        }

        #endregion
    }

    public class WaveApplicationOverrides
    {
        public int? TargetID { get; private set; }
        
        public string Login { get; private set; }
        public byte[] Password { get; private set; }

        public int? TcpPort { get; private set; }

        public WaveApplicationOverrides(int? targetID, string login, byte[] password, int? tcpport)
        {
            TargetID = targetID;
            Login = login;
            Password = password;
            TcpPort = tcpport;
        }

        public WaveApplicationOverrides()
            : this(null, null, null, null)
        {
        }
        
        public WaveApplicationOverrides(string source)
            : this()
        {
            if (!String.IsNullOrWhiteSpace(source))
            {
                string[] parts = source.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                if ((parts.Length > 0) && !String.IsNullOrWhiteSpace(parts[0]))
                {
                    TargetID = DataHelper.StringToInt(parts[0], -1);

                    if (TargetID == -1)
                        TargetID = null;
                }

                if (TargetID.HasValue)
                {
                    if ((parts.Length > 1) && !String.IsNullOrWhiteSpace(parts[1]))
                        Login = parts[1].Trim();

                    if ((parts.Length > 2) && !String.IsNullOrWhiteSpace(parts[2]))
                        Password = StringHelper.GetBytes(parts[2]);

                    if ((parts.Length > 3) && !String.IsNullOrWhiteSpace(parts[3]))
                    {
                        TcpPort = DataHelper.StringToInt(parts[3], -1);

                        if (TcpPort == -1)
                            TcpPort = null;
                    }
                }
            }
        }

        public override string ToString()
        {
            if (TargetID.HasValue && ((Login != null) || (Password != null) || TcpPort.HasValue))
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(TargetID.Value.ToString());
                sb.Append(Environment.NewLine);
                sb.Append((Login != null) ? Login : String.Empty);
                sb.Append(Environment.NewLine);
                sb.Append((Password != null) ? StringHelper.GetString(Password) : String.Empty);
                sb.Append(Environment.NewLine);
                sb.Append(TcpPort.HasValue ? TcpPort.ToString() : String.Empty);

                return sb.ToString();
            }
            
            return null;
        }
    }

    public struct ProtocolData
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public ProtocolData(string source)
            : this()
        {
            if (!String.IsNullOrWhiteSpace(source))
            {
                string[] parts = source.Split(':');

                if (parts.Length == 2)
                {
                    Host = parts[0].Trim();
                    Port = DataHelper.StringToInt(parts[1], -1);
                }
            }
        }

        public bool IsValid
        {
            get { return (!String.IsNullOrEmpty(Host) && (Port > 0)); }
        }

        public override string ToString()
        {
            if (IsValid)
                return String.Format("{0}:{1}", Host, Port);
            else
                return String.Empty;
        }
    }
}
