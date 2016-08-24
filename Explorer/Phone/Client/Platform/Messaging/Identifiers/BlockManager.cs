namespace Wave.Platform.Messaging
{
    public enum DeviceGroup : short
    {
        Default = 0,

        LowResolution_Portrait = 1,
        MediumResolution_Portrait = 2,
        HighResolution_Portrait = 3,
        VeryHighResolution_Portrait = 4,
        HVGAPortrait = 8,

        LowResolution_Square = 101,
        MediumResolution_Square = 102,
        HighResolution_Square = 103,

        MediumResolution_Landscape = 202,
        HighResolution_Landscape = 203,

        Res_240x260 = 208,
        Res_480x320 = 209,
        Res_360x480 = 210,

        Res_800x480 = 213,
        Res_480x800 = 214,
        Res_854x480 = 215,
        Res_480x854 = 216,

        Res_480x360 = 218,
        Res_360x400 = 219,

        Res_600x1024 = 220,
        Res_1024x600 = 221,
        Res_800x1280 = 222,
        Res_1280x800 = 223,
        Res_960x540 = 224,
        Res_540x960 = 225,

        VeryLowMemoryDevice = 303,

        HighestDeviceGroup = 303
    }
}
