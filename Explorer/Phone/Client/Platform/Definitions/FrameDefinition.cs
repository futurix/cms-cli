using System.IO;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class FrameDefinition : ContainerBlockDefinition, ICacheable
    {
        public FrameDefinition()
            : base()
        {
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            UnpackDefinitionID(source);

            // unpacking common attributes
            Background = new PaintStyle(source, DefAgentFieldID.BackgroundPaintStyle);
            UnpackMarginsAndPadding(source);

            // rendering hints for the block
            UnpackBlockHints(source);

            // done
            IsUnpacked = true;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.FrameDefinition; }
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
