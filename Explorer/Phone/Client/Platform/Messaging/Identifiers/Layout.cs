using System;

namespace Wave.Platform.Messaging
{
    [Flags]
    public enum CropStrategy
    {
        // crop strategy to tick slot's content
        Tick = 1 << 0,

        // crop strategy to wrap the slot's content (only applicable to text)
        Wrap = 1 << 1,
        CropLeft = 1 << 2,
        CropRight = 1 << 3,
        CropHCenter = 1 << 4,
        CropTop = 1 << 5,
        CropBottom = 1 << 6,
        CropVCenter = 1 << 7,
        Stretch = 1 << 8,

        // alignment
        AlignLeft = 1 << 9,
        AlignHCenter = 1 << 10,
        AlignRight = 1 << 11,
        AlignTop = 1 << 12,
        AlignVCenter = 1 << 13,
        AlignBottom = 1 << 14,

        // new values for CSL4+ (where crop strategy is int32)
        StretchToFit = 1 << 16,
        PreserveAspectFit = 1 << 17,
        PreserveAspectCrop = 1 << 18,
        Tile = 1 << 19,
        TileVertically = 1 << 20,
        TileHorizontally = 1 << 21
    }
}
