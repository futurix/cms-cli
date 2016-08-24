using System;

namespace Wave.Platform.Messaging
{
    public enum CacheAgentMessageID : short
    {
        CacheHashCheck = 1,
        RequestCacheHash = 2,
        DeleteItemsFromCache = 3,
        ClearCache = 4,
        CacheAck = 5,
        SendWABCacheCIID = 6
    }
    
    public enum CacheAgentFieldID : short
    {
        CacheAck = 1, // byte
        CacheHash = 258, // binary
        CacheHashCompressed = 259 // binary
    }

    [Flags]
    public enum CacheHint : byte
    {
        // cache hint constants

        // session cache (lost when the session is terminated)
        SessionStore_MustNotCache = 0x00, // 0000 (0000)
        SessionStore_MayCache = 0x10, // 0001 (0000)
        SessionStore_ShouldCache = 0x20, // 0010 (0000)

        // persistant cache (kept between sessions)
        PersistStore_MustNotCache = 0x00, // (0000) 0000
        PersistStore_MayCache = 0x01, // (0000) 0001
        PersistStore_ShouldCache = 0x02, // (0000) 0010

        // guaranteed cache (must not be removed on cache clearance)
        GuaranteedStore = 0x04, // (0000) 0100
        BackStackOnly = 0x08, // (0000) 1000

        // cache ack constants

        // session cache 
        SessionStore_DidNotCache = 0x00, // 0000 (0000)
        SessionStore_HaveCached = 0x10, // 0001 (0000)

        // persistant cache 
        PersistStore_DidNotCache = 0x00, // (0000) 0000
        PersistStore_HaveCached = 0x01 // (0000) 0001
    }

    public enum CacheEntityType : short
    {
		Definition = 0,
		Node = 1,
		Content = 2
    }

    public enum CacheMode : short
    {
        Session = 0,
		Persistant = 1
    }
}
