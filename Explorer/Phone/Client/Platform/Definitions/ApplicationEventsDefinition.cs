using System.IO;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class ApplicationEventsDefinition : DefinitionBase, ICacheable
    {
        public ActionSetCollection Events { get; private set; }
        
        public ApplicationEventsDefinition()
            : base()
        {
            Events = new ActionSetCollection();
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            UnpackDefinitionID(source);

            // loading actions
            Events.Load(source, DefAgentFieldID.ApplicationEventActionSet);

            // done
            IsUnpacked = true;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.ApplicationEvents; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteByte(0); //HACK: no actual serialisation yet!!
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                if (str.ReadByte() == 1)
                {
                }
            }
        }

        #endregion
    }
}
