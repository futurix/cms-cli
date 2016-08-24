namespace Wave.Common
{
    public static class WaveConstant
    {
        public const string True = "true";
        public const string False = "false";

        public const string UnknownText = "???";
        public const string InvalidPath = "/InvalidPath.invalid";
    }

    public static class SettingKey
    {
        public const string InstallationID = "InstallationID";
        public const string CacheCIID = "CacheCIID";
        public const string ApplicationID = "ApplicationID";
        public const string ApplicationList = "ApplicationList";
        public const string FileCacheIteration = "FileCacheIteration";
    }

    public static class PlatformOption
    {
        public const string NoEncryption = "DisableEncryption";
        public const string NoFavourites = "SkipFavouritesSelection";
        public const string ForcePortrait = "ForcePortrait";
        public const string ForceLandscape = "ForceLandscape";
        public const string NoSystemTray = "DisableSystemTray";
    }

    public static class ClientOption
    {
        public const string SystemTray = "SystemTray";
        public const string SystemTrayColours = "SystemTrayOptions";
        public const string ApplicationBarColours = "ApplicationBarOptions";
        public const string MetroOptimisations = "Metro";
    }
}
