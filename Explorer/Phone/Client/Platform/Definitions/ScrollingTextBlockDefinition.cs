using System.IO;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class ScrollingTextBlockDefinition : BlockDefinition, ICacheable
    {
        public short? Font { get; private set; }
        public PaintStyle? Foreground { get; private set; }
        public PaintStyle? Background { get; private set; }
        
        public ScrollingTextBlockDefinition()
            : base()
        {
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            UnpackDefinitionID(source);

            // unpacking common attributes
            BackgroundImageCrop = (CropStrategy)(source[DefAgentFieldID.BackgroundCropStrategy].AsNumber() ?? 0);
            SizeToBackgroundImage = source[DefAgentFieldID.BlockSizeToBackground].AsBoolean() ?? false;

            // unpacking visuals
            FieldList stateData = source[DefAgentFieldID.DataPerComponentState] as FieldList;

            if (stateData != null)
            {
                // set paint styles
                Foreground = new PaintStyle(stateData, DefAgentFieldID.ForegroundPaintStyle);
                Background = new PaintStyle(stateData, DefAgentFieldID.BackgroundPaintStyle);

                // font reference
                Font = stateData[DefAgentFieldID.FontReference].AsShort() ?? 0;
            }

            // rendering hints for the block
            UnpackBlockHints(source);

            // done
            IsUnpacked = true;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.ScrollingTextBlockDefinition; }
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
