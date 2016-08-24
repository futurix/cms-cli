using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class GridBlockDefinition : ContainerBlockDefinition, ICacheable
    {
        public short Font { get; private set; }
        public PaintStyle? Foreground { get; private set; }

        public GridSelectionMode BlockSelectionMode { get; private set; }
        public short DefaultSlotDefaultPaletteEntryIndex { get; private set; }

        public int MinimumColumnWidthPercent { get; private set; }
        public int MaximumColumnWidthPercent { get; private set; }

        public bool AreColumnsHeadersFocusable { get; private set; }
        public bool AreRowHeadersFocusable { get; private set; }
        public bool KeepColumnHeadersVisible { get; private set; }
        public bool KeepRowHeadersVisible { get; private set; }
        public bool ForceContentsToGridWidth { get; private set; }

        public short Spacing { get; private set; }

        public ScrollIndication HorizontalScrollIndicationType { get; private set; }
        public ScrollIndication VerticalScrollIndicationType { get; private set; }
        
        public GridBlockDefinition()
            : base()
        {
            BlockSelectionMode = GridSelectionMode.Cell;

            DefaultSlotDefaultPaletteEntryIndex = -1;
            MinimumColumnWidthPercent = 20;
            MaximumColumnWidthPercent = 50;

            HorizontalScrollIndicationType = ScrollIndication.Bar;
            VerticalScrollIndicationType = ScrollIndication.Bar;
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            UnpackDefinitionID(source);

            // unpack paint styles
            Foreground = new PaintStyle(source, DefAgentFieldID.ForegroundPaintStyle);
            Background = new PaintStyle(source, DefAgentFieldID.BackgroundPaintStyle);

            // font
            Font = source[DefAgentFieldID.FontReference].AsShort() ?? 0;

            BlockSelectionMode = (GridSelectionMode)(source[DefAgentFieldID.SelectionMode].AsByte() ?? (byte)GridSelectionMode.Cell);
            DefaultSlotDefaultPaletteEntryIndex = source[DefAgentFieldID.DefaultSlotDefaultPaletteEntry].AsShort() ?? -1;

            MinimumColumnWidthPercent = source[DefAgentFieldID.MinimumColumnWidth].AsByte() ?? 20;
            MaximumColumnWidthPercent = source[DefAgentFieldID.MaximumColumnWidth].AsByte() ?? 50;
            
            Spacing = source[DefAgentFieldID.Spacing].AsShort() ?? 0;
            
            AreColumnsHeadersFocusable = source[DefAgentFieldID.ColumnsHeadersFocusable].AsBoolean() ?? false;
            AreRowHeadersFocusable = source[DefAgentFieldID.RowHeadersFocusable].AsBoolean() ?? false;

            KeepColumnHeadersVisible = source[DefAgentFieldID.KeepColumnsHeadersVisible].AsBoolean() ?? false;
            KeepRowHeadersVisible = source[DefAgentFieldID.KeepRowHeadersVisible].AsBoolean() ?? false;

            // scroll indication
            HorizontalScrollIndicationType = (ScrollIndication)(source[DefAgentFieldID.HorizontalScrollIndicationType].AsByte() ?? (byte)ScrollIndication.Bar);
            VerticalScrollIndicationType = (ScrollIndication)(source[DefAgentFieldID.VerticalScrollIndicationType].AsByte() ?? (byte)ScrollIndication.Bar);

            UnpackMarginsAndPadding(source);
			
			ForceContentsToGridWidth = source[DefAgentFieldID.ForceContentToWidth].AsBoolean() ?? false;

            UnpackBlockHints(source);

            // done
            IsUnpacked = true;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.GridBlockDefinition; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteShort(Font);

            if (Foreground.HasValue)
            {
                str.WriteByte(1);
                Foreground.Value.Persist(str);
            }
            else
                str.WriteByte(0);

            str.WriteByte((byte)BlockSelectionMode);
            str.WriteShort(DefaultSlotDefaultPaletteEntryIndex);
            
            str.WriteInteger(MinimumColumnWidthPercent);
            str.WriteInteger(MaximumColumnWidthPercent);

            str.WriteBool(AreColumnsHeadersFocusable);
            str.WriteBool(AreRowHeadersFocusable);
            str.WriteBool(KeepColumnHeadersVisible);
            str.WriteBool(KeepRowHeadersVisible);
            str.WriteBool(ForceContentsToGridWidth);

            str.WriteShort(Spacing);
            str.WriteShort((short)HorizontalScrollIndicationType);
            str.WriteShort((short)VerticalScrollIndicationType);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                Font = str.ReadShort();

                if (str.ReadByte() == 1)
                {
                    PaintStyle fg = new PaintStyle();
                    fg.Restore(str);

                    Foreground = fg;
                }
                else
                    Foreground = null;

                BlockSelectionMode = (GridSelectionMode)str.ReadByte();
                DefaultSlotDefaultPaletteEntryIndex = str.ReadShort();

                MinimumColumnWidthPercent = str.ReadInteger();
                MaximumColumnWidthPercent = str.ReadInteger();

                AreColumnsHeadersFocusable = str.ReadBool();
                AreRowHeadersFocusable = str.ReadBool();
                KeepColumnHeadersVisible = str.ReadBool();
                KeepRowHeadersVisible = str.ReadBool();
                ForceContentsToGridWidth = str.ReadBool();

                Spacing = str.ReadShort();
                HorizontalScrollIndicationType = (ScrollIndication)str.ReadShort();
                VerticalScrollIndicationType = (ScrollIndication)str.ReadShort();
            }
        }

        #endregion
    }
}
