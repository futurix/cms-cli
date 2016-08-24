using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Shell;
using Wave.Services;

namespace Wave.UI
{
    public partial class ActionSheet : UserControl, IOverlayPlugin
    {
        public const string SystemTrayBackgroundBrushName = "PhoneChromeBrush";
        
        private List<ActionSheetItem> items = null;
        private ActionSheetCallback resultCallback = null;

        private ApplicationBar bar = null;

        public ActionSheet(string caption, Dictionary<string, int> acts, ActionSheetCallback callback)
        {
            InitializeComponent();

            // saving title
            if (!String.IsNullOrWhiteSpace(caption))
                ApplicationTitle.Text = caption.ToUpper();
            else
                TitlePanel.Visibility = Visibility.Collapsed;

            // saving actions and indices
            if ((acts != null) && (acts.Count > 0))
            {
                items = new List<ActionSheetItem>(acts.Count);

                foreach (var item in acts)
                    items.Add(new ActionSheetItem() { Caption = item.Key.ToLower(), Index = item.Value });

                MainListBox.ItemsSource = items;
            }
            
            // saving callback
            resultCallback = callback;

            // preparing application bar
            bar = new ApplicationBar();
            bar.IsVisible = false;
        }

        private void Action_Tap(object sender, GestureEventArgs e)
        {
            if (resultCallback != null)
            {
                // launch the action
                ActionSheetItem selection = MainListBox.SelectedItem as ActionSheetItem;

                if (selection != null)
                    resultCallback(selection.Index);

                // hide the pop-up
                Core.UI.HideOverlay();
            }
        }

        #region IOverlayPlugin implementation

        public bool GoBack()
        {
            Core.UI.HideOverlay();

            return true;
        }

        public void SignalStart()
        {
            // setting up system tray
            Core.UI.SystemTray.Set(
                SystemTrayManager.FindSystemBrushColour(SystemTrayBackgroundBrushName),
                SystemTrayManager.FindSystemBrushColour(SystemTrayManager.ForegroundBrushName),
                SystemTrayManager.DefaultOpacity);

            // setting up application bar
            Core.UI.ApplicationBar.ApplyTemporaryBar(bar);
        }

        public void SignalClosure()
        {
            Core.UI.ApplicationBar.RevertTemporaryBar();
            Core.UI.SystemTray.Reset();
        }

        public void SignalOrientationChange()
        {
        }

        #endregion

        #region Private types

        public class ActionSheetItem
        {
            public string Caption { get; set; }
            public int Index { get; set; }
        }

        #endregion
    }
}
