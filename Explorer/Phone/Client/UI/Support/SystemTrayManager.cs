using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Wave.Common;

namespace Wave.UI
{
    public class SystemTrayManager
    {
        public const string ForegroundBrushName = "PhoneForegroundBrush";
        public const double DefaultOpacity = 1.0;

        public bool IsVisible
        {
            get { return SystemTray.GetIsVisible(managedPage); }
            set { SystemTray.SetIsVisible(managedPage, value); }
        }

        public bool IsInProgress
        {
            get { return (indicator != null) && indicator.IsVisible; }
            set
            {
                if (indicator != null)
                    indicator.IsVisible = value;
            }
        }

        private PhoneApplicationPage managedPage = null;

        private Color background = ApplicationBarManager.DefaultColour;
        private Color foreground = ApplicationBarManager.DefaultColour;
        private double opacity = DefaultOpacity;

        private ProgressIndicator indicator = null;

        public SystemTrayManager(PhoneApplicationPage host)
        {
            managedPage = host;

            indicator = new ProgressIndicator();
            indicator.IsIndeterminate = true;
            indicator.IsVisible = false;

            SystemTray.SetProgressIndicator(managedPage, indicator);
        }

        public void SetDefaults(Color? bg, Color? fg, double? op, bool apply = true)
        {
            if (bg.HasValue)
                background = bg.Value;
            else
                background = Colors.Transparent;

            if (fg.HasValue)
                foreground = fg.Value;
            else
                foreground = FindSystemBrushColour(ForegroundBrushName);

            if (op.HasValue)
                opacity = op.Value;
            else
                opacity = DefaultOpacity;
            
            if (apply)
                Reset();
        }

        public void SaveDefaults()
        {
            background = SystemTray.GetBackgroundColor(managedPage);
            foreground = SystemTray.GetForegroundColor(managedPage);
            opacity = SystemTray.GetOpacity(managedPage);
        }

        public void Set(Color? bg, Color? fg, double? op)
        {
            if (bg.HasValue)
                SystemTray.SetBackgroundColor(managedPage, bg.Value);

            if (fg.HasValue)
                SystemTray.SetForegroundColor(managedPage, fg.Value);

            if (op.HasValue)
                SystemTray.SetOpacity(managedPage, op.Value);
        }

        public void Reset()
        {
            SystemTray.SetBackgroundColor(managedPage, background);
            SystemTray.SetForegroundColor(managedPage, foreground);
            SystemTray.SetOpacity(managedPage, opacity);
        }

        public static Color FindSystemBrushColour(string brushName)
        {
            SolidColorBrush brush = ResourceHelper.Find(brushName) as SolidColorBrush;

            if (brush != null)
                return brush.Color;
            else
                return Colors.Transparent;
        }
    }
}
