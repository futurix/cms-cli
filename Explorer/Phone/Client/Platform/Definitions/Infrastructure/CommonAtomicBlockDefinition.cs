using System.IO;
using Wave.Services;

namespace Wave.Platform
{
    public abstract class CommonAtomicBlockDefinition : BlockDefinition, ICacheable
    {
        public bool AcceptsFocus { get; protected set; }
        public int SortIndex { get; protected set; }
        public bool IsCheckable { get; protected set; }
        
        public CommonAtomicBlockDefinition()
            : base()
        {
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
            }
        }

        #endregion
    }
}
