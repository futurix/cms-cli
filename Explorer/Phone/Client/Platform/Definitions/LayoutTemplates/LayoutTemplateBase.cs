using System.IO;
using Wave.Services;

namespace Wave.Platform
{
    public abstract class LayoutTemplateBase : ICacheable
    {
        public LayoutTemplateBase()
        {
        }

        #region ICacheable implementation

        public virtual CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public virtual void Persist(Stream str)
        {
            str.WriteByte(0);
        }

        public virtual void Restore(Stream str)
        {
            str.ReadByte();
        }

        #endregion
    }
}
