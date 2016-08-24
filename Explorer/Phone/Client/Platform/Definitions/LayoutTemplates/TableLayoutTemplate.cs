using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class TableLayoutTemplate : LayoutTemplateBase, ICacheable
    {
        public UIFlowDirection Flow = UIFlowDirection.Down;

        public byte ColumnCount = 0;
        public byte RowCount = 0;

        public short[] RelativeSizes = null;
        public short[] StretchWeights = null;

        public short Spacing = 0;

        public List<TableLayoutItemInfo> LayoutItems = new List<TableLayoutItemInfo>();
        
        public TableLayoutTemplate()
            : base()
        {
        }

        public TableLayoutTemplate(FieldList source)
            : this()
        {
            Flow = (UIFlowDirection)(source[DefAgentFieldID.FlowDirection].AsByte() ?? 0);

            ColumnCount = source[DefAgentFieldID.NumberOfColumns].AsByte() ?? 0;
            RowCount = source[DefAgentFieldID.NumberOfRows].AsByte() ?? 0;

            // relative sizes
            RelativeSizes = new short[ColumnCount];
            List<Int16Field> sizes = source.GetItems<Int16Field>(DefAgentFieldID.RelativeSize);

            for (int i = 0; i < sizes.Count; i++)
                RelativeSizes[i] = sizes[i].Data;

            // stretch weights
            StretchWeights = new short[RowCount];
            List<Int16Field> weights = source.GetItems<Int16Field>(DefAgentFieldID.StretchWeight);

            for (int i = 0; i < weights.Count; i++)
                StretchWeights[i] = weights[i].Data;

            // spacing
            Spacing = source[DefAgentFieldID.Spacing].AsShort() ?? 0;

            // layout items
            List<FieldList> layoutItems = source.GetItems<FieldList>(DefAgentFieldID.LayoutItem);

            foreach (FieldList item in layoutItems)
            {
                TableLayoutItemInfo li = new TableLayoutItemInfo();

                // compulsory bits
                li.SlotX = item[DefAgentFieldID.XPosition].AsShort() ?? 0;
                li.SlotY = item[DefAgentFieldID.YPosition].AsShort() ?? 0;
                li.SlotXEnd = (short)((item[DefAgentFieldID.Width].AsShort() ?? 0) + li.SlotX - 1);
                li.SlotYEnd = (short)((item[DefAgentFieldID.Height].AsShort() ?? 0) + li.SlotY - 1);

                li.CropStrategy = (CropStrategy)(item[DefAgentFieldID.CropStrategy].AsNumber() ?? 0);

                // optional bits
                li.LeftMargin = item[DefAgentFieldID.LeftMargin2].AsShort() ?? 0;
                li.TopMargin = item[DefAgentFieldID.TopMargin2].AsShort() ?? 0;
                li.RightMargin = item[DefAgentFieldID.RightMargin].AsShort() ?? 0;
                li.BottomMargin = item[DefAgentFieldID.BottomMargin].AsShort() ?? 0;

                li.LeftPadding = item[DefAgentFieldID.LeftPadding2].AsShort() ?? 0;
                li.TopPadding = item[DefAgentFieldID.TopPadding2].AsShort() ?? 0;
                li.RightPadding = item[DefAgentFieldID.RightPadding].AsShort() ?? 0;
                li.BottomPadding = item[DefAgentFieldID.BottomPadding].AsShort() ?? 0;

                li.MaximumChars = item[DefAgentFieldID.MaximumChars].AsShort() ?? 0;
                li.MaximumLines = item[DefAgentFieldID.MaximumLines].AsShort() ?? 0;
                li.MinimumLines = item[DefAgentFieldID.MinimumLines].AsShort() ?? 0;

                LayoutItems.Add(li);
            }
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.TableLayoutTemplate; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteShort((short)Flow);
            str.WriteByte(ColumnCount);
            str.WriteByte(RowCount);

            if ((RelativeSizes != null) && (RelativeSizes.Length > 0))
            {
                str.WriteShort((short)RelativeSizes.Length);

                foreach (short relSize in RelativeSizes)
                    str.WriteShort(relSize);
            }
            else
                str.WriteShort(0);

            if ((StretchWeights != null) && (StretchWeights.Length > 0))
            {
                str.WriteShort((short)StretchWeights.Length);

                foreach (short stretch in StretchWeights)
                    str.WriteShort(stretch);
            }
            else
                str.WriteShort(0);

            str.WriteShort(Spacing);

            str.WriteInteger(LayoutItems.Count);

            if (LayoutItems.Count > 0)
                foreach (TableLayoutItemInfo layoutItem in LayoutItems)
                    layoutItem.Persist(str);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                Flow = (UIFlowDirection)str.ReadShort();
                ColumnCount = (byte)str.ReadByte();
                RowCount = (byte)str.ReadByte();

                short numberOfRelSizes = str.ReadShort();

                if (numberOfRelSizes > 0)
                {
                    RelativeSizes = new short[numberOfRelSizes];

                    for (int i = 0; i < numberOfRelSizes; i++)
                        RelativeSizes[i] = str.ReadShort();
                }
                else
                    RelativeSizes = null;

                short numberOfStretches = str.ReadShort();

                if (numberOfStretches > 0)
                {
                    StretchWeights = new short[numberOfStretches];

                    for (int i = 0; i < numberOfStretches; i++)
                        StretchWeights[i] = str.ReadShort();
                }
                else
                    StretchWeights = null;

                Spacing = str.ReadShort();

                int numberOfLayoutItems = str.ReadInteger();

                if (numberOfLayoutItems > 0)
                {
                    for (int i = 0; i < numberOfLayoutItems; i++)
                    {
                        TableLayoutItemInfo layoutItem = new TableLayoutItemInfo();
                        layoutItem.Restore(str);

                        LayoutItems.Add(layoutItem);
                    }
                }
            }
        }

        #endregion
    }

    public class TableLayoutItemInfo : ICacheable
    {
        public short SlotX { get; set; }
        public short SlotY { get; set; }
        public short SlotXEnd { get; set; }
        public short SlotYEnd { get; set; }

        public CropStrategy CropStrategy { get; set; }

        public short MaximumChars { get; set; }
        public short MaximumLines { get; set; }
        public short MinimumLines { get; set; }

        public short LeftMargin { get; set; }
        public short TopMargin { get; set; }
        public short RightMargin { get; set; }
        public short BottomMargin { get; set; }

        public short LeftPadding { get; set; }
        public short TopPadding { get; set; }
        public short RightPadding { get; set; }
        public short BottomPadding { get; set; }

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);

            str.WriteShort(SlotX);
            str.WriteShort(SlotY);
            str.WriteShort(SlotXEnd);
            str.WriteShort(SlotYEnd);

            str.WriteShort((short)CropStrategy);

            str.WriteShort(MaximumChars);
            str.WriteShort(MaximumLines);
            str.WriteShort(MinimumLines);

            str.WriteShort(LeftMargin);
            str.WriteShort(TopMargin);
            str.WriteShort(RightMargin);
            str.WriteShort(BottomMargin);

            str.WriteShort(LeftPadding);
            str.WriteShort(TopPadding);
            str.WriteShort(RightPadding);
            str.WriteShort(BottomPadding);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                SlotX = str.ReadShort();
                SlotY = str.ReadShort();
                SlotXEnd = str.ReadShort();
                SlotYEnd = str.ReadShort();

                CropStrategy = (CropStrategy)str.ReadShort();

                MaximumChars = str.ReadShort();
                MaximumLines = str.ReadShort();
                MinimumLines = str.ReadShort();

                LeftMargin = str.ReadShort();
                TopMargin = str.ReadShort();
                RightMargin = str.ReadShort();
                BottomMargin = str.ReadShort();

                LeftPadding = str.ReadShort();
                TopPadding = str.ReadShort();
                RightPadding = str.ReadShort();
                BottomPadding = str.ReadShort();
            }
        }

        #endregion
    }
}
