using System;
using System.Windows;

namespace Wave.Common
{
    public static class ThreadHelper
    {
        public static void Sync(Action a)
        {
            Deployment.Current.Dispatcher.BeginInvoke(a);
        }

        public static void Sync(Delegate d, params object[] args)
        {
            Deployment.Current.Dispatcher.BeginInvoke(d, args);
        }
    }
}
