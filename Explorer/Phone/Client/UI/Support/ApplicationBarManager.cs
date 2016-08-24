using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Wave.UI
{
    public class ApplicationBarManager
    {
        public const ApplicationBarMode DefaultMode = ApplicationBarMode.Default;
        public const double DefaultOpacity = 1.0;
        public static Color DefaultColour = Color.FromArgb(0, 0, 0, 0);
        
        // references to main page
        private PhoneApplicationPage managedPage = null;
        private IApplicationBar managedBar = null;

        // default values
        private ApplicationBarMode defaultMode = DefaultMode;
        private Color defaultBackground = DefaultColour;
        private Color defaultForeground = DefaultColour;
        private double defaultOpacity = DefaultOpacity;

        public ApplicationBarManager(PhoneApplicationPage host)
        {
            managedPage = host;
            managedBar = host.ApplicationBar;
        }

        public void SetDefaults(ApplicationBarMode? mode, Color? background, Color? foreground, double? opacity = null, bool apply = true)
        {
            defaultMode = mode ?? ApplicationBarMode.Default;
            defaultBackground = background ?? DefaultColour;
            defaultForeground = foreground ?? DefaultColour;
            defaultOpacity = opacity ?? DefaultOpacity;

            if (apply && (managedBar != null))
            {
                managedBar.Mode = defaultMode;
                managedBar.BackgroundColor = defaultBackground;
                managedBar.ForegroundColor = defaultForeground;
                managedBar.Opacity = defaultOpacity;
            }
        }

        public void ApplyDefaults(ApplicationBar target)
        {
            target.BackgroundColor = defaultBackground;
            target.ForegroundColor = defaultForeground;
            target.Opacity = defaultOpacity;
        }

        public void Set(
            List<ApplicationBarIconButton> buttons, List<ApplicationBarMenuItem> menuItems = null, 
            ApplicationBarMode? mode = null, Color? background = null, Color? foreground = null, double? opacity = null)
        {
            if (managedBar != null)
            {
                // setting buttons
                managedBar.Buttons.Clear();

                if ((buttons != null) && (buttons.Count > 0))
                {
                    foreach (ApplicationBarIconButton button in buttons)
                        managedBar.Buttons.Add(button);
                }

                // setting menu items
                managedBar.MenuItems.Clear();

                if ((menuItems != null) && (menuItems.Count > 0))
                {
                    foreach (ApplicationBarMenuItem menuItem in menuItems)
                        managedBar.MenuItems.Add(menuItem);

                    managedBar.IsMenuEnabled = true;
                }
                else
                    managedBar.IsMenuEnabled = false;

                // size
                if (mode.HasValue)
                    managedBar.Mode = mode.Value;
                else
                    managedBar.Mode = defaultMode;

                // colours

                if (background.HasValue)
                    managedBar.BackgroundColor = background.Value;
                else
                    managedBar.BackgroundColor = defaultBackground;

                if (foreground.HasValue)
                    managedBar.ForegroundColor = foreground.Value;
                else
                    managedBar.ForegroundColor = defaultForeground;

                // opacity
                if (opacity.HasValue)
                    managedBar.Opacity = opacity.Value;
                else
                    managedBar.Opacity = defaultOpacity;

                managedBar.IsVisible = ((managedBar.Buttons.Count > 0) || (managedBar.MenuItems.Count > 0));
            }
        }

        public void Reset(bool keepVisible = false)
        {
            if (managedBar != null)
            {
                managedBar.Buttons.Clear();
                
                managedBar.MenuItems.Clear();
                managedBar.IsMenuEnabled = false;

                managedBar.Mode = defaultMode;
                managedBar.BackgroundColor = defaultBackground;
                managedBar.ForegroundColor = defaultForeground;
                managedBar.Opacity = defaultOpacity;

                managedBar.IsVisible = keepVisible;
            }
        }

        public void ApplyTemporaryBar(ApplicationBar bar)
        {
            managedPage.ApplicationBar = bar;
        }

        public void RevertTemporaryBar()
        {
            managedPage.ApplicationBar = managedBar;
        }
    }
}
