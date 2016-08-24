using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using Microsoft.Phone.Controls;
using Wave.Common;
using Wave.Services;
using Wave.UI;

namespace Wave.Explorer
{
    public partial class MainPage : PhoneApplicationPage
    {
        public const string SplashScreenPath = @"Resources/Media/SplashScreen.png";
        
        #region State and shortcut properties

        public View RootView
        {
            get { return rootView; }
        }

        public bool HasRootView
        {
            get { return (rootView != null); }
        }

        public long RootViewID
        {
            get { return (rootView != null) ? rootView.ID : -1; }
        }

        public ContentReferenceRegistrar ReferencedContent { get; private set; }

        public InterlockedBool IgnoreBackButton = false;

        public SystemTrayManager SystemTray { get; private set; }
        public new ApplicationBarManager ApplicationBar { get; private set; }

        private IOverlayPlugin OverlayPlugin
        {
            get { return Overlay.Child as IOverlayPlugin; }
        }

        private MainPageState PageState
        {
            get { return (Root.Visibility == Visibility.Visible) ? MainPageState.Root : MainPageState.Overlay; }
            set
            {
                if (PageState != value)
                {
                    if (value == MainPageState.Root)
                    {
                        Overlay.Visibility = Visibility.Collapsed;
                        Root.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Root.Visibility = Visibility.Collapsed;
                        Overlay.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        #endregion

        #region Private fields

        private View rootView = null;
        private Dictionary<long, View> views = new Dictionary<long, View>();

        private bool firstNavigation = true;

        #endregion

        public MainPage()
        {
            InitializeComponent();

            // initialising content reference registrar
            ReferencedContent = new ContentReferenceRegistrar();

            // initialising shared UI managers
            SystemTray = new SystemTrayManager(this);
            ApplicationBar = new ApplicationBarManager(this);

            // creating root view
            rootView = new View(null, this);
            rootView.VerticalAlignment = VerticalAlignment.Stretch;
            rootView.HorizontalAlignment = HorizontalAlignment.Stretch;
            rootView.InitialNodeLoad += new EventHandler(rootView_InitialNodeLoad);

            // enabling root view
            Root.Child = rootView;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (firstNavigation)
            {
                SetupSplashScreen();
                
                // applying orientation restrictions (if any)
                if (App.Instance.Build.PlatformOptions.Contains(PlatformOption.ForcePortrait))
                    SupportedOrientations = SupportedPageOrientation.Portrait;
                else if (App.Instance.Build.PlatformOptions.Contains(PlatformOption.ForceLandscape))
                    SupportedOrientations = SupportedPageOrientation.Landscape;
            }

            // signalling orientation change
            Core.System.OnOrientationChange((Orientation & PageOrientation.Portrait) == PageOrientation.Portrait);

            // registering with core
            Core.RegisterPage(this);

            // attaching event handlers
            Core.ConnectionCompletelyFailed += new EventHandler(Core_ConnectionCompletelyFailed);
            Core.StreamResponseReceived += new EventHandler<StreamResponseEventArgs>(Core_StreamResponseReceived);
            Core.LocationChanged += new EventHandler(Core_LocationChanged);
            Core.LocationUnavailable += new EventHandler(Core_LocationUnavailable);

            // clean navigation journal (just in case)
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();

            // first-time only navigation
            if (firstNavigation)
            {
                firstNavigation = false;

                // renew background task if needed
                BackgroundHelper.MaintainBackgroundTask();

                // determine app to launch
                int appIndex = 0;
                string appIndexString = null;

                if (NavigationContext.QueryString.TryGetValue("app", out appIndexString))
                    appIndex = DataHelper.StringToInt(appIndexString, 0);

                ThreadHelper.Sync(() => Core.Start(App.Instance.Build.Applications[appIndex]));
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Core.ConnectionCompletelyFailed -= Core_ConnectionCompletelyFailed;
            Core.StreamResponseReceived -= Core_StreamResponseReceived;
            Core.LocationChanged -= Core_LocationChanged;
            Core.LocationUnavailable -= Core_LocationUnavailable;

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Core.UnregisterPage(this);

            base.OnNavigatedFrom(e);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (PageState == MainPageState.Overlay)
            {
                IOverlayPlugin ctrl = OverlayPlugin;

                if (ctrl != null)
                    e.Cancel = ctrl.GoBack();
            }
            else if (rootView != null)
            {
                if (!IgnoreBackButton)
                {
                    bool backActionHandled = false;

                    // find view to handle back anchor
                    View handler = rootView.FindBackKeyDelegate();

                    if (handler == null)
                        handler = rootView;

                    // ask subscribed block to handle the back anchor
                    backActionHandled = handler.HandleBackKey();

                    // go back (or not)
                    e.Cancel = backActionHandled ? true : GoBack();
                }
                else
                    e.Cancel = true;
            }
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);
            
            Core.System.OnOrientationChange((e.Orientation & PageOrientation.Portrait) == PageOrientation.Portrait);
        }

        #region View management and interaction

        public View this[long viewID]
        {
            get { return FindView(viewID); }
        }

        public void RegisterView(View view)
        {
            if (view != null)
                views[view.ID] = view;
        }

        public void UnregisterView(long id)
        {
            if (views.ContainsKey(id))
                views.Remove(id);
        }

        #endregion

        #region View signals

        public void SignalViewNavigationStart(long viewID)
        {
            View view = FindView(viewID);

            if (view != null)
                view.SignalNavigationStart();
        }

        public void SignalViewNavigationSuccess(long viewID, NodeData data)
        {
            View view = FindView(viewID);

            if (view != null)
                view.SignalNavigationSuccess(data);
        }

        public void SignalViewNavigationFailure(long viewID)
        {
            View view = FindView(viewID);

            if (view != null)
                view.SignalNavigationFailure();
        }

        #endregion

        #region Navigation

        public void LoadApplication(WaveApplication app)
        {
            HideOverlay();

            ThreadHelper.Sync(() => Core.Start(app));
        }

        private bool GoBack()
        {
            if (rootView != null)
                return rootView.GoBack();
            else
                return false;
        }

        #endregion

        #region Overlay management

        public void ShowOverlay(UserControl plugin)
        {
            if ((plugin != null) && (plugin is IOverlayPlugin) && (PageState == MainPageState.Root))
            {
                Overlay.Child = plugin;
                PageState = MainPageState.Overlay;

                ThreadHelper.Sync(() => ((IOverlayPlugin)plugin).SignalStart());
            }
        }

        public void HideOverlay()
        {
            if (PageState == MainPageState.Overlay)
            {
                IOverlayPlugin ctrl = OverlayPlugin;

                if (ctrl != null)
                    ctrl.SignalClosure();

                PageState = MainPageState.Root;
                Overlay.Child = null;
            }
        }

        #endregion

        #region Event handlers

        private void rootView_InitialNodeLoad(object sender, EventArgs e)
        {
            // reset background brush and related settings
            Root.Background = null;
            
            if (rootView != null)
                rootView.IsProgressScreenShaded = true;
            
            // apply system tray settings
            SystemTray.IsVisible = !App.Instance.Build.PlatformOptions.Contains(PlatformOption.NoSystemTray);
        }

        private void Core_ConnectionCompletelyFailed(object sender, EventArgs e)
        {
            UIHelper.Message("Data connection failed.");
        }

        private void Core_StreamResponseReceived(object sender, StreamResponseEventArgs e)
        {
            MediaHelper.PlayFullScreen(e.StreamURL);
        }

        //TODO: allow to target frames in here
        private void Core_LocationChanged(object sender, EventArgs e)
        {
            if ((rootView != null) && (rootView.CurrentNode != null))
                rootView.CurrentNode.OnLocationChanged();
        }

        private void Core_LocationUnavailable(object sender, EventArgs e)
        {
            if ((rootView != null) && (rootView.CurrentNode != null))
                rootView.CurrentNode.OnLocationUnavailable();
        }

        #endregion

        #region Helper methods

        private void SetupSplashScreen()
        {
            try
            {
                StreamResourceInfo splashInfo = Application.GetResourceStream(new Uri(SplashScreenPath, UriKind.Relative));

                if (splashInfo != null)
                {
                    // create and apply splash screen image
                    ImageBrush splash = new ImageBrush();
                    splash.Stretch = Stretch.None;
                    splash.ImageSource = new BitmapImage(new Uri(SplashScreenPath, UriKind.Relative));

                    Root.Background = splash;

                    // temporarily disable shading of progress layer
                    if (rootView != null)
                        rootView.IsProgressScreenShaded = false;
                }
            }
            catch
            {
            }
        }

        private View FindView(long viewID)
        {
            View res = null;

            views.TryGetValue(viewID, out res);

            return res;
        }

        #endregion
    }

    public enum MainPageState
    {
        Root,
        Overlay
    }
}