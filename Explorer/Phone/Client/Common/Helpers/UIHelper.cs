using System.Windows;
using Wave.Services;

namespace Wave.Common
{
    public static class UIHelper
    {
        public static MessageBoxResult Message(string text, string title = Core.ApplicationName, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            return MessageBox.Show(text, title, buttons);
        }
    }
}
