using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;
using Wave.UI;

namespace Wave.Platform
{
    public abstract class ContainerBlockDefinition : BlockDefinition, ICacheable
    {
        public PaintStyle? Background { get; protected set; }
        public short FirstFocus { get; protected set; }

        public int MarginLeft { get; set; }
        public int MarginTop { get; set; }
        public int MarginRight { get; set; }
        public int MarginBottom { get; set; }

        public int PaddingLeft { get; set; }
        public int PaddingTop { get; set; }
        public int PaddingRight { get; set; }
        public int PaddingBottom { get; set; }

        /// <summary>
        /// Orientation of the container's children.
        /// </summary>
        public bool IsHorizontal { get; protected set; }

        /// <summary>
        /// If true, only one element can be selected at the same time (radio button).
        /// </summary>
        public bool EnforceRadioBehaviour { get; protected set; }

        public ContainerBlockDefinition()
            : base()
        {
        }

        protected void UnpackMarginsAndPadding(FieldList source)
        {
            MarginLeft = source[DefAgentFieldID.LeftMargin2].AsShort() ?? -1;
            MarginTop = source[DefAgentFieldID.TopMargin2].AsShort() ?? -1;
            MarginRight = source[DefAgentFieldID.RightMargin].AsShort() ?? 0;
            MarginBottom = source[DefAgentFieldID.BottomMargin].AsShort() ?? 0;
            PaddingLeft = source[DefAgentFieldID.LeftPadding2].AsShort() ?? -1;
            PaddingTop = source[DefAgentFieldID.TopPadding2].AsShort() ?? -1;
            PaddingRight = source[DefAgentFieldID.RightPadding].AsShort() ?? 0;
            PaddingBottom = source[DefAgentFieldID.BottomPadding].AsShort() ?? 0;

            if (MarginLeft == -1)
                MarginLeft = source[DefAgentFieldID.LeftMargin].AsShort() ?? 0;

            if (MarginTop == -1)
                MarginTop = source[DefAgentFieldID.TopMargin].AsShort() ?? 0;

            if (PaddingLeft == -1)
                PaddingLeft = source[DefAgentFieldID.LeftPadding].AsShort() ?? 0;

            if (PaddingTop == -1)
                PaddingTop = source[DefAgentFieldID.TopPadding].AsShort() ?? 0;
        }

        public void CopyMarginsAndPaddingToBlock(BlockBase block)
        {
            if (block != null)
            {
                block.Margin = new Spacing(MarginLeft, MarginTop, MarginRight, MarginBottom);
                block.Padding = new Spacing(PaddingLeft, PaddingTop, PaddingRight, PaddingBottom);
            }
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

            // background
            str.WriteBool(Background.HasValue);

            if (Background.HasValue)
                Background.Value.Persist(str);

            // first focus
            str.WriteShort(FirstFocus);

            // margins
            str.WriteInteger(MarginLeft);
            str.WriteInteger(MarginTop);
            str.WriteInteger(MarginRight);
            str.WriteInteger(MarginBottom);

            // paddings
            str.WriteInteger(PaddingLeft);
            str.WriteInteger(PaddingTop);
            str.WriteInteger(PaddingRight);
            str.WriteInteger(PaddingBottom);

            // other things
            str.WriteBool(IsHorizontal);
            str.WriteBool(EnforceRadioBehaviour);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                // background
                if (str.ReadBool())
                {
                    PaintStyle bg = new PaintStyle();
                    bg.Restore(str);

                    Background = bg;
                }
                else
                    Background = null;

                // first focus
                FirstFocus = str.ReadShort();

                // margins
                MarginLeft = str.ReadInteger();
                MarginTop = str.ReadInteger();
                MarginRight = str.ReadInteger();
                MarginBottom = str.ReadInteger();

                // paddings
                PaddingLeft = str.ReadInteger();
                PaddingTop = str.ReadInteger();
                PaddingRight = str.ReadInteger();
                PaddingBottom = str.ReadInteger();

                // other things
                IsHorizontal = str.ReadBool();
                EnforceRadioBehaviour = str.ReadBool();
            }
        }

        #endregion
    }
}
