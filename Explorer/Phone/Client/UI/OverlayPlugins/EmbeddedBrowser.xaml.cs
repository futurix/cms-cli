using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using Wave.Explorer;
using Wave.Services;

namespace Wave.UI
{
    public partial class EmbeddedBrowser : UserControl, IOverlayPlugin
    {
        private MainPage hostPage = null;
        private string originalUrl = null;

        private ApplicationBar bar = null;

        private Stack<Uri> history = new Stack<Uri>();

        public EmbeddedBrowser(MainPage host, string newURL)
        {
            // saving arguments
            hostPage = host;
            originalUrl = newURL;

            // system initialisation
            InitializeComponent();

            // setting up application bar
            bar = new ApplicationBar();
            Core.UI.ApplicationBar.ApplyDefaults(bar);

            ApplicationBarIconButton btn = new ApplicationBarIconButton();
            btn.IconUri = new Uri("/" + SettingsAgent.ResidentIconsPath + "cancel.png", UriKind.Relative);
            btn.Text = "Close";
            btn.Click += new EventHandler(Close_Click);

            bar.Buttons.Add(btn);
        }

        private void webBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            Uri last = null;

            if (history.Count > 0)
                last = history.Peek();

            if (last != e.Uri)
                history.Push(e.Uri);
        }

        #region IOverlayPlugin implementation

        public bool GoBack()
        {
            if (history.Count > 1)
            {
                history.Pop();
                
                webBrowser.Navigate(history.Pop());
            }
            else
                Core.UI.HideOverlay();

            return true;
        }

        public void SignalStart()
        {
            Core.UI.ApplicationBar.ApplyTemporaryBar(bar);
            
            webBrowser.Navigate(new Uri(originalUrl, UriKind.Absolute));
        }

        public void SignalClosure()
        {
            Core.UI.ApplicationBar.RevertTemporaryBar();
        }

        public void SignalOrientationChange()
        {
        }

        #endregion

        #region Event handlers

        private void Close_Click(object sender, EventArgs e)
        {
            Core.UI.HideOverlay();
        }

        #endregion
    }
}
