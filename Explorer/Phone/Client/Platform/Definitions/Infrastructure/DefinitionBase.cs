using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public abstract class DefinitionBase : ICacheable
    {
        public bool IsUnpacked { get; protected set; }
        public int DefinitionID { get; protected set; }

        public DefinitionBase()
        {
            DefinitionID = -1;
        }

        protected void UnpackDefinitionID(FieldList source)
        {
            DefinitionID = source[MessageOutFieldID.DefinitionID].AsInteger() ?? 0;
        }

        #region ICacheable implementation

        public virtual CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public virtual void Persist(Stream str)
        {
            str.WriteByte(0);
            str.WriteBool(IsUnpacked);
            str.WriteInteger(DefinitionID);
        }

        public virtual void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                IsUnpacked = str.ReadBool();
                DefinitionID = str.ReadInteger();
            }
        }

        #endregion
    }
}
