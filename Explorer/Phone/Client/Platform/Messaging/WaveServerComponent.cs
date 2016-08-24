using System;

namespace Wave.Platform.Messaging
{
    public enum WaveServerComponent
    {
        VectorAgent = 1,
        UpdateAgent = 2,
        EPGAgent = 3,
        MediaAgent = 4,
        Admo = 5,
        NavigationAgent = 8,
        DefinitionsAgent = 9,
        CacheAgent = 12,
        PatchManager = 13,
        BlockProxyAgent = 14,
        UserManager = 16,
        AggregatedMessageAgent = 17,

        Unknown = Byte.MaxValue
    }
}
