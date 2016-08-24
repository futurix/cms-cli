using System;
using Microsoft.Phone.Tasks;
using Wave.UI;

namespace Wave.Services
{
    public static class BrowserHelper
    {
        public static void Launch(string uri)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(uri, UriKind.Absolute);
            
            webBrowserTask.Show();
        }

        public static void LaunchEmbedded(string uri)
        {
            Core.UI.ShowOverlay(new EmbeddedBrowser(Core.UI, uri));
        }
    }
}
