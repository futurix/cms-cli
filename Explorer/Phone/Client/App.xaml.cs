using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Wave.Common;
using Wave.Services;

namespace Wave.Explorer
{
    public partial class App : Application
    {
        public static App Instance { get; private set; }
        
        public PhoneApplicationFrame RootFrame { get; private set; }
        public WaveBuildConfiguration Build { get; private set; }

        public App()
        {
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
            InitializePhoneApplication();

            Instance = this;
            Build = BuildHelper.LoadConfiguration();

            RootFrame.Navigating += new NavigatingCancelEventHandler(RootFrame_Navigating);

            if (Debugger.IsAttached)
            {
                if (Build == null)
                    DebugHelper.Out("Build configuration is missing!");
                else if (Build.Applications.Count < 1)
                    DebugHelper.Out("No applications defined in build configuration!");
            }
        }

        public void ResetApplications()
        {
            Build = BuildHelper.LoadConfiguration(false);
        }

        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (e.IsApplicationInstancePreserved)
                Core.Resume();
        }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            OnStop();

            Core.Suspend();
        }

        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            OnStop();

            Core.End();
        }

        private void OnStop()
        {
            // save app changes
            BuildHelper.SaveConfiguration(Build);
        }

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            // remove event handler for future navigation
            RootFrame.Navigating -= RootFrame_Navigating;
            
            // do not do anything if navigation is not for the main page
            if (!e.Uri.ToString().Contains("/MainPage.xaml"))
                return;
            
            // redirect to favourites if needed
            if (!Build.PlatformOptions.Contains(PlatformOption.NoFavourites) && (Build.Applications.Count > 1))
            {
                e.Cancel = true;

                ThreadHelper.Sync(() => RootFrame.Navigate(new Uri("/FavouritesPage.xaml", UriKind.Relative)));
            }
        }

        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break(); // navigation has failed; break into the debugger
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break(); // unhandled exception; break into the debugger
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}