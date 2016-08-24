using System.IO.IsolatedStorage;
using Microsoft.Phone.Scheduler;
using Wave.Agent;
using System;

namespace Wave.Services
{
    public static class BackgroundHelper
    {
        public const string BackgroundTaskID = "WaveExplorerAgent";
        public const string BackgroundTaskDescription = "Updates live tile for Wave Explorer.";

        public static void EnableBackgroundTask(string url, string login = null, string pass = null)
        {
            Core.Settings[BackgroundAgentSettingKey.Enabled] = true;
            Core.Settings[BackgroundAgentSettingKey.URL] = url;
            Core.Settings[BackgroundAgentSettingKey.Login] = login;
            Core.Settings[BackgroundAgentSettingKey.Password] = pass;
        }

        public static void DisableBackgroundTask()
        {
            Core.Settings[BackgroundAgentSettingKey.Enabled] = false;
            Core.Settings[BackgroundAgentSettingKey.URL] = null;
            Core.Settings[BackgroundAgentSettingKey.Login] = null;
            Core.Settings[BackgroundAgentSettingKey.Password] = null;
        }

        public static void MaintainBackgroundTask()
        {
            object timestamp = Core.Settings[BackgroundAgentSettingKey.Maintenance];

            if ((timestamp == null) || ((timestamp is DateTime) && ((DateTime.Now - (DateTime)timestamp) > TimeSpan.FromDays(1))))
            {
                Core.Settings[BackgroundAgentSettingKey.Maintenance] = DateTime.Now;
                
                if ((bool)(Core.Settings[BackgroundAgentSettingKey.Enabled] ?? false) == true)
                    AddOrRenewBackgroundTask();
                else
                    RemoveBackgroundTask();
            }
        }

        public static void AddOrRenewBackgroundTask()
        {
            PeriodicTask task = new PeriodicTask(BackgroundTaskID);
            task.Description = BackgroundTaskDescription;

            try
            {
                if (ScheduledActionService.Find(BackgroundTaskID) == null)
                    ScheduledActionService.Add(task);
                else
                    ScheduledActionService.Replace(task);
            }
            catch
            {
            }
        }

        public static void RemoveBackgroundTask()
        {
            if (ScheduledActionService.Find(BackgroundTaskID) != null)
                ScheduledActionService.Remove(BackgroundTaskID);
        }
    }
}
