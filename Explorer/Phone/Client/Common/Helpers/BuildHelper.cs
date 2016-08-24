using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Serialization;
using Wave.Services;

namespace Wave.Common
{
    public static class BuildHelper
    {
        public static WaveBuildConfiguration LoadConfiguration(bool checkSaved = true)
        {
            StreamResourceInfo templateConfig = Application.GetResourceStream(new Uri("BuildConfig.xml", UriKind.Relative));

            if ((templateConfig != null) && (templateConfig.Stream != null))
            {
                WaveBuildConfiguration config = XmlHelper.DeserializeObject(templateConfig.Stream, typeof(WaveBuildConfiguration)) as WaveBuildConfiguration;

                if ((config != null) && checkSaved)
                {
                    if (IsolatedStorageSettings.ApplicationSettings.Contains(SettingKey.ApplicationList))
                    {
                        string[] apps = IsolatedStorageSettings.ApplicationSettings[SettingKey.ApplicationList] as string[];

                        if ((apps != null) && (apps.Length > 0))
                        {
                            Dictionary<int, WaveApplicationOverrides> overrides = new Dictionary<int, WaveApplicationOverrides>();

                            foreach (string app in apps)
                            {
                                WaveApplicationOverrides ovr = new WaveApplicationOverrides(app);

                                if (ovr.TargetID.HasValue)
                                    overrides[ovr.TargetID.Value] = ovr;
                            }

                            if (overrides.Count > 0)
                            {
                                foreach (WaveApplication app in config.Applications)
                                {
                                    WaveApplicationOverrides ovr;

                                    if (overrides.TryGetValue(app.ID, out ovr))
                                        app.ApplyOverrides(ovr);
                                }
                            }
                        }
                    }
                }

                return config;
            }
            else
                return null;
        }

        public static void SaveConfiguration(WaveBuildConfiguration config)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            // clean existing saved data (just in case)
            if (settings.Contains(SettingKey.ApplicationList))
                settings.Remove(SettingKey.ApplicationList);

            // save if there is anything to save
            if ((config != null) && (config.Applications.Count > 0))
            {
                List<WaveApplicationOverrides> overrides = new List<WaveApplicationOverrides>();

                foreach (WaveApplication app in config.Applications)
                {
                    WaveApplicationOverrides ovr = app.ToOverrides();

                    if (ovr != null)
                        overrides.Add(ovr);
                }

                if (overrides.Count > 0)
                {
                    List<string> saved = new List<string>();

                    foreach (WaveApplicationOverrides ovr in overrides)
                    {
                        string temp = ovr.ToString();

                        if (temp != null)
                            saved.Add(temp);
                    }

                    if (saved.Count > 0)
                        settings[SettingKey.ApplicationList] = saved.ToArray();
                }
            }
        }
    }

    [XmlRoot("BuildConfiguration")]
    public class WaveBuildConfiguration
    {
        public string this[string key]
        {
            get
            {
                if (ClientOptions.ContainsKey(key))
                    return ClientOptions[key];
                else
                    return String.Empty;
            }
        }

        [XmlAttribute("BuildID")]
        public string BuildID = "Acme Build";

        public string ResidentMedia = "";

        [XmlArray("Applications")]
        [XmlArrayItem("Application")]
        public List<WaveApplication> Applications = new List<WaveApplication>();

        [XmlArray("PlatformOptions")]
        [XmlArrayItem("Option")]
        public List<string> PlatformOptions = new List<string>();

        public StringDictionary ClientOptions = new StringDictionary();
    }
}
