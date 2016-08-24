using System;
using Microsoft.Phone.Tasks;

namespace Wave.Common
{
    public static class MediaHelper
    {
        public static void PlayFullScreen(string uri)
        {
            MediaPlayerLauncher mediaPlayerLauncher = new MediaPlayerLauncher();

            mediaPlayerLauncher.Media = new Uri(uri, UriKind.Absolute);
            mediaPlayerLauncher.Controls = MediaPlaybackControls.All;
            mediaPlayerLauncher.Location = MediaLocationType.Data;

            mediaPlayerLauncher.Show();
        }
    }
}
