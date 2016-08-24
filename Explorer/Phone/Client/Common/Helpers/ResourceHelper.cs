using System.Windows;

namespace Wave.Common
{
    public static class ResourceHelper
    {
        public static object Find(string name)
        {
            if (Application.Current.Resources.Contains(name))
                return Application.Current.Resources[name];
            else
                return null;
        }
    }
}
