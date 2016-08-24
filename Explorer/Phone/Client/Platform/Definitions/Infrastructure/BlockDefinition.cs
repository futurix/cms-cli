using System;
using System.IO;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public abstract class BlockDefinition : DefinitionBase, ICacheable
    {
        public HintsDictionary RenderingHints { get; protected set; }
        public UIHintedType HintedType { get { return RenderingHints.HintedType; } }

        public CropStrategy BackgroundImageCrop { get; protected set; }
        public bool SizeToBackgroundImage { get; protected set; }

        public BlockDefinition()
            : base()
        {
            RenderingHints = new HintsDictionary();

            BackgroundImageCrop = 0;
            SizeToBackgroundImage = false;
        }

        protected void UnpackBlockHints(FieldList source)
        {
            string renderingHintsSource = source[DefAgentFieldID.Hint].AsString();

            if (!String.IsNullOrEmpty(renderingHintsSource))
                RenderingHints.Parse(renderingHintsSource);
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

            RenderingHints.Persist(str);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
                RenderingHints.Restore(str);
        }

        #endregion
    }
}
