using System;
using System.IO.IsolatedStorage;
using System.Text;
using System.Windows.Media;
using Wave.Common;
using Wave.Explorer;
using Wave.Platform.Messaging;
using Wave.UI;

namespace Wave.Services
{
    public class SettingsAgent
    {
        public const string ResidentMediaPath = "Resources/Embedded/";
        public const string ResidentIconsPath = "Resources/Icons/";
        
        public Guid InstallationID { get; private set; }
        public bool IsMetro { get; private set; }

        private IsolatedStorageSettings settings = null;
        private ResidentMediaManager residentMedia = null;

        public SettingsAgent()
        {
            IsMetro = true;
            
            // connecting to Isolated Storage (persistent settings storage)
            settings = IsolatedStorageSettings.ApplicationSettings;
        }

        public void Start()
        {
            // try reading existing installation ID
            bool createNewInstallationID = true;

            if (settings.Contains(SettingKey.InstallationID))
            {
                string rawGuid = settings[SettingKey.InstallationID] as string;

                if (!String.IsNullOrWhiteSpace(rawGuid))
                {
                    Guid candidate;

                    if (Guid.TryParse(rawGuid, out candidate))
                    {
                        InstallationID = candidate;

                        createNewInstallationID = false;
                    }
                }
            }

            // create new installation ID if needed
            if (createNewInstallationID)
            {
                InstallationID = Guid.NewGuid();

                settings[SettingKey.InstallationID] = InstallationID.ToString();
            }

            // parse resident media (if any)
            if ((App.Instance.Build != null) && !String.IsNullOrWhiteSpace(App.Instance.Build.ResidentMedia))
            {
                residentMedia = new ResidentMediaManager();
                residentMedia.Unpack(App.Instance.Build.ResidentMedia);
            }

            // loading build configuration settings

            // setting system tray colours (if defined in build configuration)
            if (App.Instance.Build.ClientOptions.ContainsKey(ClientOption.SystemTrayColours))
            {
                string[] options = App.Instance.Build[ClientOption.SystemTrayColours].Split(new char[] { ',' }, StringSplitOptions.None);

                Color? bg = null, fg = null;
                double? op = null;

                if ((options.Length > 0) && !String.IsNullOrWhiteSpace(options[0]))
                    bg = DataHelper.HexToColour(options[0]);

                if ((options.Length > 1) && !String.IsNullOrWhiteSpace(options[1]))
                    fg = DataHelper.HexToColour(options[1]);

                if ((options.Length > 2) && !String.IsNullOrWhiteSpace(options[2]))
                {
                    double temp = 0;

                    if (Double.TryParse(options[2], out temp))
                        op = temp;
                }

                Core.UI.SystemTray.SetDefaults(bg, fg, op, true);
            }
            else
                Core.UI.SystemTray.SaveDefaults();

            // setting application bar colours
            if (App.Instance.Build.ClientOptions.ContainsKey(ClientOption.ApplicationBarColours))
            {
                string[] options = App.Instance.Build[ClientOption.ApplicationBarColours].Split(new char[] { ',' }, StringSplitOptions.None);

                Color? bg = null, fg = null;
                double? op = null;

                if ((options.Length > 0) && !String.IsNullOrWhiteSpace(options[0]))
                    bg = DataHelper.HexToColour(options[0]);

                if ((options.Length > 1) && !String.IsNullOrWhiteSpace(options[1]))
                    fg = DataHelper.HexToColour(options[1]);

                if ((options.Length > 2) && !String.IsNullOrWhiteSpace(options[2]))
                {
                    double temp = 0;

                    if (Double.TryParse(options[2], out temp))
                        op = temp;
                }

                Core.UI.ApplicationBar.SetDefaults(ApplicationBarManager.DefaultMode, bg, fg, op, true);
            }

            // setting Metro optimisation
            IsMetro = !App.Instance.Build[ClientOption.MetroOptimisations].Equals(WaveConstant.False, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Suspend()
        {
            Save();
        }

        public void End()
        {
            Save();
        }

        public object this[string key]
        {
            get
            {
                if (settings.Contains(key))
                    return settings[key];
                else
                    return null;
            }
            set
            {
                if (value == null)
                {
                    if (settings.Contains(key))
                        settings.Remove(key);
                }
                else
                    settings[key] = value;
            }
        }

        public void Save()
        {
            if (settings != null)
                settings.Save();
        }

        public string ResolveResidentMedia(string key)
        {
            return ResolveResidentMedia(key, SystemAgent.DefaultDeviceGroup);
        }

        public string ResolveResidentMedia(string key, DeviceGroup device)
        {
            if (residentMedia != null)
            {
                string tempRes = residentMedia.FindFile(key, device);

                if (!String.IsNullOrEmpty(tempRes))
                    return String.Concat(ResidentMediaPath, tempRes);
                else
                    return String.Empty;
            }
            else
                return String.Empty;
        }

        public string TimeZone
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(TimeZoneInfo.Local.StandardName);
                sb.Append(',');
                sb.Append(Math.Round(TimeZoneInfo.Local.BaseUtcOffset.TotalHours, 2));
                sb.Append(',');
                sb.Append(TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now));

                return sb.ToString();
            }
        }
    }
}
