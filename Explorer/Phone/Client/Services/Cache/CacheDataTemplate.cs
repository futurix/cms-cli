using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class CacheDataTemplate
    {
        public object Data { get; set; }
        public byte[] DataBinary { get; set; }

        public CacheItemID? CIID { get; set; }
        public byte[] Key { get; set; }

        public CacheEntityType EntityType { get; set; }
        public CacheMode? Mode { get; set; }
        public byte CacheHint { get; set; }

        public CacheDataTemplate()
        {
            Data = null;
            DataBinary = null;

            CIID = null;
            Key = null;

            EntityType = CacheEntityType.Definition;
            Mode = null;
            CacheHint = 0;
        }

        public CacheDataTemplate(object data, CacheItemID? ciid, byte[] key, CacheEntityType entityType, byte cacheHint = 0)
        {
            Data = data;
            DataBinary = null;

            CIID = null;
            Key = key;

            EntityType = entityType;
            Mode = null;
            CacheHint = cacheHint;
        }
    }
}
