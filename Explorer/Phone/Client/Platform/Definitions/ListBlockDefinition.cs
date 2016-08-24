using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class ListBlockDefinition : ContainerBlockDefinition, ICacheable
    {
        /// <summary>
        /// Default maximum number of children.
        /// </summary>
        private const int DefaultMaximumNumberOfChildren = 40;
        
        /// <summary>
        /// Set by VisibleChilren field in flow layout definition.
        /// </summary>
        public int NumberOfFullyVisibleChildrenInHorizontalMode { get; private set; }
        
        public ScrollIndication ScrollIndicationType { get; private set; }
        public int ScrollBarSize { get; private set; }
        
        /// <summary>
        /// Sent from the server in the form of a hint: focusPosOffset=x. Currently only used for horizontal flow 
        /// when block is a list block.
        /// </summary>
        public int FocusPositionOffset { get; private set; }
        public FocusPosition BlockFocusPosition { get; private set; }
        public FocusBehaviour BlockFocusBehaviour { get; private set; }
        
        /// <summary>
        /// If true new blocks will be added from the top when a block proxy update is received.
        /// </summary>
        public bool InsertFromTheTop { get; private set; }

        public int TickerPauseTimeOnItem { get; private set; }
        public TickerSpeed BlockTickerSpeed { get; private set; }
        public TickerFlowDirection TickerDirection { get; private set; }
        public int TicksPerUnit { get; private set; }
        public int PixelsPerUnit { get; private set; }
        public bool AllowAutoScroll { get; private set; }

        /// <summary>
        /// True if the list should scroll a set amount with each scroll event.
        /// </summary>
        public bool FixedScrolling { get; private set; }
        /// <summary>
        /// The fixed amount to scroll with each scroll event. If 0, the block should calculate based on FixedScrollingJumps.
        /// </summary>
        public int FixedScrollingSize { get; private set; }
        /// <summary>
        /// The number of scroll jumps required to scroll through a screen of content. So if 4, it would take 4 scroll 
        /// movements to move fully to the second page of content.
        /// Ignored if FixedScrollingSize is non-zero.
        /// </summary>
        public int FixedScrollingJumps { get; private set; }

        /// <summary>
        /// Space between children.
        /// </summary>
        public int Spacing { get; private set; }

        public int MaximumNumberOfChildren { get; private set; }

        public List<short> Stripes { get; private set; }

        public bool Paginate { get; private set; }
        public bool IsFloating { get; private set; }

        public bool ScrollableBackground { get; private set; }
        
        public ListBlockDefinition()
            : base()
        {
            NumberOfFullyVisibleChildrenInHorizontalMode = 4;
            
            ScrollIndicationType = ScrollIndication.Bar;
            ScrollBarSize = 8;

            BlockFocusPosition = FocusPosition.Free;
            BlockFocusBehaviour = FocusBehaviour.Normal;

            FixedScrolling = false;
            FixedScrollingSize = 0;
            FixedScrollingJumps = 6;

            Stripes = new List<short>();
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            UnpackDefinitionID(source);

            Background = new PaintStyle(source, DefAgentFieldID.BackgroundPaintStyle);

            UIFlowDirection fd = (UIFlowDirection)(source[DefAgentFieldID.FlowDirection].AsByte() ?? (byte)UIFlowDirection.Down);
            IsHorizontal = (fd == UIFlowDirection.Right);

            BackgroundImageCrop = (CropStrategy)(source[DefAgentFieldID.BackgroundCropStrategy].AsNumber() ?? 0);
            SizeToBackgroundImage = source[DefAgentFieldID.BlockSizeToBackground].AsBoolean() ?? false;
            ScrollableBackground = source[DefAgentFieldID.BackgroundScrollable].AsBoolean() ?? false;

            NumberOfFullyVisibleChildrenInHorizontalMode = source[DefAgentFieldID.VisibleChildren].AsShort() ?? 4;

            AllowAutoScroll = source.GetItemCount(DefAgentFieldID.AllowAutoscroll) > 0;
            EnforceRadioBehaviour = source.GetItemCount(DefAgentFieldID.EnforceRadio) > 0;

            ScrollIndicationType = (ScrollIndication)(source[DefAgentFieldID.ScrollIndicationType].AsByte() ?? (byte)ScrollIndication.Bar);

            Spacing = source[DefAgentFieldID.Spacing].AsShort() ?? 0;
			
			UnpackMarginsAndPadding(source);

            BlockFocusBehaviour = (FocusBehaviour)(source[DefAgentFieldID.FocusBehaviour].AsByte() ?? (byte)FocusBehaviour.Normal);
            BlockFocusPosition = (FocusPosition)(source[DefAgentFieldID.FocusPosition].AsByte() ?? (byte)FocusPosition.Free);

            MaximumNumberOfChildren = source[DefAgentFieldID.MaximumChildren].AsShort() ?? DefaultMaximumNumberOfChildren;

            InsertFromTheTop = !(source[DefAgentFieldID.FromBottom].AsBoolean() ?? false);
            
            BlockTickerSpeed = (TickerSpeed)(source[DefAgentFieldID.TickerSpeed].AsByte() ?? (byte)TickerSpeed.Medium);

            switch (BlockTickerSpeed)
            {
                case TickerSpeed.Slow:
                    TicksPerUnit = 1;
                    PixelsPerUnit = 1;
                    break;

                case TickerSpeed.Medium:
                default:
                    TicksPerUnit = 0;
                    PixelsPerUnit = 1;
                    break;

                case TickerSpeed.Fast:
                    TicksPerUnit = 0;
                    PixelsPerUnit = 2;
                    break;
            }

            TickerDirection = (TickerFlowDirection)(source[DefAgentFieldID.TickerDirection].AsByte() ?? (byte)TickerFlowDirection.Forwards);

            TickerItemPause tickerPauseSetting = (TickerItemPause)(source[DefAgentFieldID.PauseBetweenItem].AsByte() ?? (byte)TickerItemPause.None);

            switch (tickerPauseSetting)
            {
                case TickerItemPause.Long:
                    TickerPauseTimeOnItem = 300;
                    break;

                case TickerItemPause.Medium:
                    TickerPauseTimeOnItem = 150;
                    break;

                case TickerItemPause.Short:
                    TickerPauseTimeOnItem = 50;
                    break;

                case TickerItemPause.None:
                default:
                    TickerPauseTimeOnItem = 0;
                    break;
            }

            Paginate = source[DefAgentFieldID.UsePagination].AsBoolean() ?? false;

            // get list of striping palette references
            FieldList stripes = source[DefAgentFieldID.StripeStyles] as FieldList;

            if (stripes != null)
            {
                List<Int16Field> stripesData = source.GetItems<Int16Field>(DefAgentFieldID.PaletteEntryIndex);

                foreach (Int16Field field in stripesData)
                    Stripes.Add(field.Data);
            }

            IsFloating = source[DefAgentFieldID.FloatBehaviour].AsBoolean() ?? false;

            UnpackBlockHints(source);

            // done
            IsUnpacked = true;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.ListBlockDefinition; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteInteger(NumberOfFullyVisibleChildrenInHorizontalMode);
            str.WriteShort((short)ScrollIndicationType);
            str.WriteInteger(ScrollBarSize);
            str.WriteInteger(FocusPositionOffset);
            str.WriteByte((byte)BlockFocusPosition);
            str.WriteByte((byte)BlockFocusBehaviour);
            str.WriteBool(InsertFromTheTop);
            str.WriteInteger(TickerPauseTimeOnItem);
            str.WriteByte((byte)BlockTickerSpeed);
            str.WriteByte((byte)TickerDirection);
            str.WriteInteger(TicksPerUnit);
            str.WriteInteger(PixelsPerUnit);
            str.WriteBool(AllowAutoScroll);
            str.WriteBool(FixedScrolling);
            str.WriteInteger(FixedScrollingSize);
            str.WriteInteger(FixedScrollingJumps);
            str.WriteInteger(Spacing);
            str.WriteInteger(MaximumNumberOfChildren);

            str.WriteShort((short)Stripes.Count);
            foreach (short stripe in Stripes)
                str.WriteShort(stripe);

            str.WriteBool(Paginate);
            str.WriteBool(IsFloating);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                NumberOfFullyVisibleChildrenInHorizontalMode = str.ReadInteger();
                ScrollIndicationType = (ScrollIndication)str.ReadShort();
                ScrollBarSize = str.ReadInteger();
                FocusPositionOffset = str.ReadInteger();
                BlockFocusPosition = (FocusPosition)str.ReadByte();
                BlockFocusBehaviour = (FocusBehaviour)str.ReadByte();
                InsertFromTheTop = str.ReadBool();
                TickerPauseTimeOnItem = str.ReadInteger();
                BlockTickerSpeed = (TickerSpeed)str.ReadByte();
                TickerDirection = (TickerFlowDirection)str.ReadByte();
                TicksPerUnit = str.ReadInteger();
                PixelsPerUnit = str.ReadInteger();
                AllowAutoScroll = str.ReadBool();
                FixedScrolling = str.ReadBool();
                FixedScrollingSize = str.ReadInteger();
                FixedScrollingJumps = str.ReadInteger();
                Spacing = str.ReadInteger();
                MaximumNumberOfChildren = str.ReadInteger();

                short numberOfStripes = str.ReadShort();

                if (numberOfStripes > 0)
                {
                    for (int i = 0; i < numberOfStripes; i++)
                        Stripes.Add(str.ReadShort());
                }

                Paginate = str.ReadBool();
                IsFloating = str.ReadBool();
            }
        }

        #endregion
    }
}
