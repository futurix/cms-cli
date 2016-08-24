using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Wave.Common;
using Wave.Services;

namespace Wave.Explorer
{
    public partial class FavouritesPage : PhoneApplicationPage
    {
        public FavouritesPage()
        {
            InitializeComponent();

            ApplicationList.ItemsSource = App.Instance.Build.Applications;
        }

        private void AppListItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ThreadHelper.Sync(() =>
            {
                int index = ApplicationList.SelectedIndex;

                if ((index >= 0) && (index < App.Instance.Build.Applications.Count))
                    NavigationService.Navigate(new Uri(String.Format("/MainPage.xaml?app={0}", index), UriKind.Relative));
            });
        }

        private void ClearCache_Click(object sender, System.EventArgs e)
        {
            Core.Cache.Server.DestroyCacheOffline();
        }

        private void ResetLogins_Click(object sender, System.EventArgs e)
        {
            App.Instance.ResetApplications();
            ApplicationList.ItemsSource = App.Instance.Build.Applications;
        }

        private void LaunchAgent_Click(object sender, System.EventArgs e)
        {
            if (ScheduledActionService.Find(BackgroundHelper.BackgroundTaskID) != null)
                ScheduledActionService.LaunchForTest(BackgroundHelper.BackgroundTaskID, TimeSpan.FromSeconds(5));
            else
                MessageBox.Show("Agent is not enabled!");
        }
    }
}