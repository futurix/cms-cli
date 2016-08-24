using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class SlotStyleData : ICacheable
    {
        public bool IsUnpacked { get; private set; }
        
        public short Font { get; set; }
        public PaintStyle? Foreground { get; set; }
        public PaintStyle? Background { get; set; }

        public CropStrategy Crop { get; set; }

        public int MarginLeft { get; set; }
        public int MarginTop { get; set; }
        public int MarginRight { get; set; }
        public int MarginBottom { get; set; }
        
        public SlotStyleData()
        {
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;
            
            Foreground = new PaintStyle(source, DefAgentFieldID.ForegroundPaintStyle);
            Background = new PaintStyle(source, DefAgentFieldID.BackgroundPaintStyle);
            Font = source[DefAgentFieldID.FontReference].AsShort() ?? 0;

            Crop = (CropStrategy)(source[DefAgentFieldID.CropStrategy].AsNumber() ?? 0);

            MarginLeft = source[DefAgentFieldID.LeftMargin2].AsShort() ?? 0;
            MarginTop = source[DefAgentFieldID.TopMargin2].AsShort() ?? 0;
            MarginRight = source[DefAgentFieldID.RightMargin].AsShort() ?? 0;
            MarginBottom = source[DefAgentFieldID.BottomMargin].AsShort() ?? 0;

            // done
            IsUnpacked = true;
        }

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);
            str.WriteBool(IsUnpacked);
            str.WriteShort(Font);

            if (Foreground.HasValue)
            {
                str.WriteByte(1);
                Foreground.Value.Persist(str);
            }
            else
                str.WriteByte(0);

            if (Background.HasValue)
            {
                str.WriteByte(1);
                Background.Value.Persist(str);
            }
            else
                str.WriteByte(0);

            str.WriteShort((short)Crop);

            str.WriteInteger(MarginLeft);
            str.WriteInteger(MarginTop);
            str.WriteInteger(MarginRight);
            str.WriteInteger(MarginBottom);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                IsUnpacked = str.ReadBool();
                Font = str.ReadShort();

                if (str.ReadByte() == 1)
                {
                    PaintStyle fg = new PaintStyle();
                    fg.Restore(str);

                    Foreground = fg;
                }
                else
                    Foreground = null;

                if (str.ReadByte() == 1)
                {
                    PaintStyle bg = new PaintStyle();
                    bg.Restore(str);

                    Background = bg;
                }
                else
                    Background = null;

                Crop = (CropStrategy)str.ReadShort();

                MarginLeft = str.ReadInteger();
                MarginTop = str.ReadInteger();
                MarginRight = str.ReadInteger();
                MarginBottom = str.ReadInteger();
            }
        }

        #endregion
    }
}
