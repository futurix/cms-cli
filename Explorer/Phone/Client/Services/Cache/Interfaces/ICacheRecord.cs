using Wave.Platform.Messaging;

namespace Wave.Services
{
    public interface ICacheRecord
    {
        CacheItemID? CIID { get; set; }
        byte[] Key { get; set; }

        CacheEntityType EntityType { get; set; }
        CacheMode CacheMode { get; set; }

        byte[] Data { get; set; }
    }
}
