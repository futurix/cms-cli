using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;
using Microsoft.Phone.Scheduler;

namespace Wave.Agent
{
    public class BackgroundAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        public BackgroundAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                
                // subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        protected override void OnInvoke(ScheduledTask task)
        {
            if ((bool)(IsolatedStorageSettings.ApplicationSettings[BackgroundAgentSettingKey.Enabled] ?? false) == true)
            {
                string url = (string)IsolatedStorageSettings.ApplicationSettings[BackgroundAgentSettingKey.URL];
                string login = (string)IsolatedStorageSettings.ApplicationSettings[BackgroundAgentSettingKey.Login];
                string pass = (string)IsolatedStorageSettings.ApplicationSettings[BackgroundAgentSettingKey.Password];

                LiveTileUpdater updater = new LiveTileUpdater(url, login, pass);

                updater.StartUpdate(() => NotifyComplete());
            }
            else
                NotifyComplete();
        }
    }
}