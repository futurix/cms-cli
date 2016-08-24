using System;
using System.Windows;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class AtomicBlock : AtomicBlockBase
    {
        #region Visual properties

        protected override PaintStyle? BackgroundStyle
        {
            get
            {
                if (Data != null)
                {
                    AtomicBlockStateData stateData = Data[currentState];

                    if (stateData != null)
                        return stateData.ComponentBackground;
                }

                return base.BackgroundStyle;
            }
        }

        protected override PaintStyle? ForegroundStyle
        {
            get
            {
                if (Data != null)
                {
                    AtomicBlockStateData stateData = Data[currentState];

                    if (stateData != null)
                        return stateData.ComponentForeground;
                }

                return base.ForegroundStyle;
            }
        }

        #endregion

        #region State properties

        public override bool CanCheck
        {
            get { return (Data != null) ? Data.IsCheckable : false; }
        }

        public override bool CanFocus
        {
            get { return (Data != null) ? Data.AcceptsFocus : false; }
        }

        #endregion

        #region Privates

        private AtomicBlockDefinition Data
        {
            get { return Definition as AtomicBlockDefinition; }
        }

        private DisplayDataCollection serverDisplayData = null;
        private TableLayoutTemplate currentLayout = null;

        #endregion

        public AtomicBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            // definition
            if (Data != null)
            {
                // register static display data
                Core.UI.ReferencedContent.Add(Data.StaticDisplayData);
                ParentNode.Register(Data.StaticDisplayData);

                // remember state availability
                hasNormalState = Data.HasState(BlockState.Normal);
                hasCheckedNormalState = Data.HasState(BlockState.CheckedNormal);
                hasFocusedState = Data.HasState(BlockState.Focused);
                hasCheckedFocusedState = Data.HasState(BlockState.CheckedFocused);
            }

            // content
            UnpackContent();
        }

        private void UnpackContent()
        {
            /*
            // set checked state
			m_fChecked = m_oDefinition.m_fCheckable && oContentFldListForBlock.getNumFieldOfType(CFieldList.c_nINT, CNaviAgent.c_fieldidIsChecked) > 0;
            */

            // parse server display data
            serverDisplayData = DisplayData.Parse(Content);

            Core.UI.ReferencedContent.Add(serverDisplayData);
            ParentNode.Register(serverDisplayData);

            // check if this block should have initial focus
            bool initialFocus = Content[NaviAgentFieldID.IsFocused].AsBoolean() ?? false;

            // set default state
            if (CanFocus && initialFocus)
                SwitchToState(BlockState.Focused);
            else
                SwitchToState(BlockState.Normal);
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if ((availableSize.Width == 0) || (availableSize.Height == 0) || (WaveChildren.Count == 0) || (currentLayout == null))
            {
                return new Size(0, 0);
            }
            else
            {
                MeasureBackground();
                
                return CalculateLayout(availableSize, null);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if ((finalSize.Width > 0) && (finalSize.Height > 0) && (WaveChildren.Count > 0) && (currentLayout != null))
            {
                Rect[] slotPositions = new Rect[WaveChildren.Count];

                Size calculatedSize = CalculateLayout(finalSize, slotPositions);

                for (int i = 0; i < WaveChildren.Count; i++)
                    WaveChildren[i].Arrange(slotPositions[i]);

                ArrangeBackground(finalSize);
            }

            return finalSize;
        }

        //TODO: optimise performance for single slot case
        private Size CalculateLayout(Size areaSize, Rect[] slotPositions)
        {
            int[] columnSizes = new int[currentLayout.ColumnCount];
            int[] rowSizes = new int[currentLayout.RowCount];
            bool[] stretchableRows = new bool[currentLayout.RowCount];
            TableLayoutPosition[] pss = new TableLayoutPosition[WaveChildren.Count];
            bool relayoutNeeded = false;

            // all rows are stretchable by default
            for (int i = 0; i < stretchableRows.Length; i++)
                stretchableRows[i] = true;

            // retrieve all stored slot position data and discover all stretchable rows
            for (int i = 0; i < WaveChildren.Count; i++)
            {
                pss[i] = TableLayout.GetSlotPosition(WaveChildren[i]);

                if ((((RendererBase)WaveChildren[i]).Crop & CropStrategy.Stretch) != CropStrategy.Stretch)
                    stretchableRows[pss[i].Row] = false;
            }

            // size fixed-size columns based on single-column slots
            for (int i = 0; i < WaveChildren.Count; i++)
            {
                if ((pss[i].ColumnSpan == 0) && (currentLayout.RelativeSizes[pss[i].Column] == 0))
                {
                    WaveChildren[i].Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

                    if ((int)WaveChildren[i].DesiredSize.Width > columnSizes[pss[i].Column])
                    {
                        if (columnSizes[pss[i].Column] > 0)
                            relayoutNeeded = true;

                        columnSizes[pss[i].Column] = (int)WaveChildren[i].DesiredSize.Width;
                    }
                }
            }

            // if width is undefined, we do not calculate relative columns - only absolute ones
            if (!Double.IsInfinity(areaSize.Width))
            {
                // calculate total remaining width for relative columns
                int remainingWidthForRelativeColumns = (int)areaSize.Width;

                foreach (int columnSize in columnSizes)
                    remainingWidthForRelativeColumns -= columnSize;

                // allocate remaining width to relative columns
                if (remainingWidthForRelativeColumns > 0)
                {
                    int totalFractions = 0;

                    for (int i = 0; i < currentLayout.RelativeSizes.Length; i++)
                        if (currentLayout.RelativeSizes[i] > 0)
                            totalFractions += currentLayout.RelativeSizes[i];

                    if (totalFractions > 0)
                    {
                        int fraction = remainingWidthForRelativeColumns / totalFractions;

                        if (fraction > 0)
                        {
                            for (int i = 0; i < currentLayout.RelativeSizes.Length; i++)
                                if (currentLayout.RelativeSizes[i] > 0)
                                    columnSizes[i] = fraction * currentLayout.RelativeSizes[i];
                        }
                    }
                }

                // add remaining width to last relative column
                remainingWidthForRelativeColumns = (int)areaSize.Width;

                for (int i = 0; i < columnSizes.Length; i++)
                    remainingWidthForRelativeColumns -= columnSizes[i];

                if (remainingWidthForRelativeColumns > 0)
                {
                    for (int i = currentLayout.RelativeSizes.Length - 1; i == 0; i--)
                    {
                        if (currentLayout.RelativeSizes[i] > 0)
                        {
                            columnSizes[i] += remainingWidthForRelativeColumns;
                            remainingWidthForRelativeColumns = 0;

                            break;
                        }
                    }
                }

                // if there are no relative columns, update the block width to match the combined fixed-size column widths
                if (remainingWidthForRelativeColumns > 0)
                {
                    //TODO: update the block width to match the combined fixed-size column widths
                }
            }

            // layout single-cell slots (both width and height)
            for (int i = 0; i < WaveChildren.Count; i++)
            {
                if ((pss[i].ColumnSpan == 0) && (pss[i].RowSpan == 0))
                {
                    if (columnSizes[pss[i].Column] > 0)
                    {
                        WaveChildren[i].Measure(new Size(columnSizes[pss[i].Column], Double.PositiveInfinity));

                        if (WaveChildren[i].DesiredSize.Height > rowSizes[pss[i].Row])
                            rowSizes[pss[i].Row] = (int)WaveChildren[i].DesiredSize.Height;
                    }
                }
            }

            // layout single row multi column slots
            for (int i = 0; i < WaveChildren.Count; i++)
            {
                if ((pss[i].ColumnSpan > 0) && (pss[i].RowSpan == 0))
                {
                    int startColumnIndex = pss[i].Column;
                    int endColumnIndex = startColumnIndex + pss[i].ColumnSpan;
                    int childWidth = 0;

                    for (int j = startColumnIndex; j <= endColumnIndex; j++)
                        childWidth += columnSizes[j];

                    childWidth += pss[i].ColumnSpan * currentLayout.Spacing;

                    if (childWidth > 0)
                    {
                        WaveChildren[i].Measure(new Size(childWidth, Double.PositiveInfinity));

                        if ((int)WaveChildren[i].DesiredSize.Height > rowSizes[pss[i].Row])
                        {
                            if (rowSizes[pss[i].Row] > 0)
                                relayoutNeeded = true;

                            rowSizes[pss[i].Row] = (int)WaveChildren[i].DesiredSize.Height;
                        }
                    }
                }
            }

            // layout multi row slots
            for (int i = 0; i < WaveChildren.Count; i++)
            {
                if (pss[i].RowSpan > 0)
                {
                    // calculate width for the slot
                    int slotWidth = 0;

                    if (pss[i].ColumnSpan > 0)
                    {
                        int startColumnIndex = pss[i].Column;
                        int endColumnIndex = startColumnIndex + pss[i].ColumnSpan;

                        for (int j = startColumnIndex; j <= endColumnIndex; j++)
                            slotWidth += columnSizes[j];

                        slotWidth += pss[i].ColumnSpan * currentLayout.Spacing;
                    }
                    else
                        slotWidth = columnSizes[pss[i].Column];
                    
                    // calculate space available for the slot
                    int startRowIndex = pss[i].Row;
                    int endRowIndex = startRowIndex + pss[i].RowSpan;
                    int availableHeightForSlot = 0;

                    for (int j = startRowIndex; j <= endRowIndex; j++)
                        availableHeightForSlot += rowSizes[j];

                    // get slot's preferred height
                    WaveChildren[i].Measure(new Size(slotWidth, Double.PositiveInfinity));

                    // make adjustments
                    if (WaveChildren[i].DesiredSize.Height > availableHeightForSlot)
                    {
                        relayoutNeeded = true;
                        
                        int numberOfStretchableRows = 0;

                        for (int j = startRowIndex; j <= endRowIndex; j++)
                        {
                            if (stretchableRows[j])
                                numberOfStretchableRows++;
                        }

                        if (numberOfStretchableRows > 0)
                        {
                            int[] stretchableRowIndices = new int[numberOfStretchableRows];
                            int[] stretchableRowWeights = new int[numberOfStretchableRows];
                            int stretchableWeightsSum = 0;

                            for (int j = startRowIndex; j <= endRowIndex; j++)
                            {
                                if (stretchableRows[j])
                                {
                                    stretchableRowIndices[j - startRowIndex] = j;
                                    stretchableRowWeights[j - startRowIndex] = (currentLayout.StretchWeights != null) ? currentLayout.StretchWeights[j] : 1;

                                    stretchableWeightsSum += stretchableRowWeights[j - startRowIndex];
                                }
                            }

                            if ((WaveChildren[i].DesiredSize.Height - availableHeightForSlot) >= 1)
                            {
                                int sizeToRedistribute = (int)(WaveChildren[i].DesiredSize.Height - availableHeightForSlot);
                                int singleAddonUnit = 0;

                                if (stretchableWeightsSum == 0)
                                {
                                    singleAddonUnit = sizeToRedistribute / numberOfStretchableRows;

                                    if (singleAddonUnit > 0)
                                    {
                                        for (int k = 0; k < stretchableRowIndices.Length; k++)
                                            rowSizes[stretchableRowIndices[k]] += singleAddonUnit;
                                    }
                                }
                                else
                                {
                                    singleAddonUnit = sizeToRedistribute / stretchableWeightsSum;

                                    if (singleAddonUnit > 0)
                                    {
                                        for (int k = 0; k < stretchableRowIndices.Length; k++)
                                        {
                                            if (stretchableRowWeights[k] > 0)
                                                rowSizes[stretchableRowIndices[k]] += stretchableRowWeights[k] * singleAddonUnit;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //TODO: if in any of the previous 3 steps stretch happened - relayout all slots
            if (relayoutNeeded)
            {
                /////////////
            }

            // prepare slot positions if requested
            if (slotPositions != null)
            {
                int x = Padding.Left, y = Padding.Top;
                int[] columnXs = new int[currentLayout.ColumnCount];
                int[] rowYs = new int[currentLayout.RowCount];
                
                // calculate column X coordinates
                for (int i = 0; i < columnXs.Length; i++)
                {
                    columnXs[i] = x;

                    x += columnSizes[i];
                    x += currentLayout.Spacing;
                }

                // calculate row Y coordinates
                for (int i = 0; i < rowYs.Length; i++)
                {
                    rowYs[i] = y;

                    y += rowSizes[i];
                    y += currentLayout.Spacing;
                }

                // calculate actual slot positions
                for (int i = 0; i < WaveChildren.Count; i++)
                {
                    double slotWidth = 0, slotHeight = 0;
                    
                    if (pss[i].ColumnSpan > 0)
                    {
                        int startColumnIndex = pss[i].Column;
                        int endColumnIndex = startColumnIndex + pss[i].ColumnSpan;

                        for (int j = startColumnIndex; j <= endColumnIndex; j++)
                            slotWidth += columnSizes[j];

                        slotWidth += pss[i].ColumnSpan * currentLayout.Spacing;
                    }
                    else
                        slotWidth = columnSizes[pss[i].Column];

                    if (pss[i].RowSpan > 0)
                    {
                        int startRowIndex = pss[i].Row;
                        int endRowIndex = startRowIndex + pss[i].RowSpan;

                        for (int j = startRowIndex; j <= endRowIndex; j++)
                            slotHeight += rowSizes[j];

                        slotHeight += pss[i].RowSpan * currentLayout.Spacing;
                    }
                    else
                        slotHeight = rowSizes[pss[i].Row];

                    slotPositions[i] = new Rect(
                        columnXs[pss[i].Column], rowYs[pss[i].Row], 
                        slotWidth, slotHeight);
                }
            }

            // calculate final control size
            double finalWidth = 0, finalHeight = 0;

            foreach (int columnSize in columnSizes)
                finalWidth += columnSize;

            finalWidth += (columnSizes.Length - 1) * currentLayout.Spacing;
            finalWidth += Padding.Left;
            finalWidth += Padding.Right;

            foreach (int rowSize in rowSizes)
                finalHeight += rowSize;

            finalHeight += (rowSizes.Length - 1) * currentLayout.Spacing;
            finalHeight += Padding.Top;
            finalHeight += Padding.Bottom;

            if (!Double.IsInfinity(areaSize.Width) && (finalWidth > areaSize.Width))
                finalWidth = areaSize.Width;

            if (!Double.IsInfinity(areaSize.Height) && (finalHeight > areaSize.Height))
                finalHeight = areaSize.Height;

            return new Size(finalWidth, finalHeight);
        }

        #endregion

        #region State management

        protected override void SwitchToState(BlockState state)
        {
            if (state == currentState)
                return;

            if (((state == BlockState.Normal) && !hasNormalState) || ((state == BlockState.Focused) && !hasFocusedState) || 
                ((state == BlockState.CheckedNormal) && !hasCheckedNormalState) || ((state == BlockState.CheckedFocused) && !hasCheckedFocusedState))
                return;

            // save old state
            BlockState oldState = currentState;
            
            // set current state
            currentState = state;

            // clean-up
            WaveChildren.Clear();
            Children.Clear();

            // try to create new state
            AtomicBlockStateData stateData = Data[state];

            if (stateData != null)
            {
                // background
                if (stateData.ComponentBackground.HasValue)
                {
                    PaintStyleResult bgRes = ResolvePaintStyle(stateData.ComponentBackground.Value);

                    if (bgRes.Brush != null)
                        Background = bgRes.Brush;
                }

                // foreground
                if (stateData.ComponentForeground.HasValue)
                {
                    PaintStyleResult fgRes = ResolvePaintStyle(stateData.ComponentForeground.Value);

                    if (fgRes.Brush != null)
                        Foreground = fgRes.Brush;
                }
                
                // margins and paddings
                Margin = new Spacing(stateData.MarginLeft, stateData.MarginTop, stateData.MarginRight, stateData.MarginBottom);
                Padding = new Spacing(stateData.PaddingLeft, stateData.PaddingTop, stateData.PaddingRight, stateData.PaddingBottom);

                // setup layout
                TableLayoutTemplate table = stateData.LayoutTemplate as TableLayoutTemplate;

                if (table != null)
                {
                    // save this layout for layout passes
                    currentLayout = table;
                    
                    // create slots
                    for (int i = 0; i < stateData.SlotInfo.Count; i++)
                    {
                        int slotIndex = stateData.SlotInfo[i].SlotIndex ?? i;
                        RendererBase renderer = null;

                        // find layout information for the slot
                        TableLayoutItemInfo slotLayout = null;

                        if ((table.LayoutItems != null) && (i < table.LayoutItems.Count))
                            slotLayout = table.LayoutItems[i];

                        // find display data for the slot
                        DisplayData dd = null;

                        if (Data.StaticDisplayData != null)
                        {
                            if (slotIndex < Data.StaticDisplayData.Count)
                            {
                                // static display data
                                dd = Data.StaticDisplayData[slotIndex];
                            }
                            else if (serverDisplayData != null)
                            {
                                // server display data (with modified index)
                                dd = serverDisplayData[slotIndex - Data.StaticDisplayData.Count];
                            }
                        }
                        else if (serverDisplayData != null)
                        {
                            // server display data
                            dd = serverDisplayData[slotIndex];
                        }

                        if ((dd != null) && (i < table.LayoutItems.Count))
                        {
                            renderer = Core.UIFactory.CreateRenderer(this, dd, stateData.SlotInfo[i], null, slotLayout);
                            
                            if (renderer != null)
                            {
                                TableLayout.SetSlotPosition(
                                    renderer,
                                    new TableLayoutPosition()
                                    {
                                        Column = table.LayoutItems[i].SlotX,
                                        Row = table.LayoutItems[i].SlotY,
                                        ColumnSpan = table.LayoutItems[i].SlotXEnd - table.LayoutItems[i].SlotX,
                                        RowSpan = table.LayoutItems[i].SlotYEnd - table.LayoutItems[i].SlotY
                                    });

                                renderer.HorizontalAlignment = HorizontalAlignment.Stretch;
                                renderer.VerticalAlignment = VerticalAlignment.Stretch;

                                WaveChildren.Add(renderer);
                                Children.Add(renderer);
                            }
                        }
                    }
                }

                // fire required anchors
                switch (oldState)
                {
                    case BlockState.Normal:
                        {
                            switch (state)
                            {
                                case BlockState.Focused:
                                    FireAction(Anchor.OnFocused);
                                    break;
                                
                                case BlockState.CheckedNormal:
                                    FireAction(Anchor.OnChecked);
                                    break;
                                
                                case BlockState.CheckedFocused:
                                    FireAction(Anchor.OnChecked);
                                    FireAction(Anchor.OnFocused);
                                    break;
                            }
                            
                            break;
                        }

                    case BlockState.Focused:
                        {
                            switch (state)
                            {
                                case BlockState.Normal:
                                    FireAction(Anchor.OnUnfocused);
                                    break;

                                case BlockState.CheckedNormal:
                                    FireAction(Anchor.OnChecked);
                                    FireAction(Anchor.OnUnfocused);
                                    break;

                                case BlockState.CheckedFocused:
                                    FireAction(Anchor.OnChecked);
                                    break;
                            }
                            
                            break;
                        }

                    case BlockState.CheckedNormal:
                        {
                            switch (state)
                            {
                                case BlockState.Normal:
                                    FireAction(Anchor.OnUnchecked);
                                    break;

                                case BlockState.Focused:
                                    FireAction(Anchor.OnUnchecked);
                                    FireAction(Anchor.OnFocused);
                                    break;

                                case BlockState.CheckedFocused:
                                    FireAction(Anchor.OnFocused);
                                    break;
                            }
                            
                            break;
                        }

                    case BlockState.CheckedFocused:
                        {
                            switch (state)
                            {
                                case BlockState.Normal:
                                    FireAction(Anchor.OnUnchecked);
                                    FireAction(Anchor.OnUnfocused);
                                    break;

                                case BlockState.Focused:
                                    FireAction(Anchor.OnUnchecked);
                                    break;

                                case BlockState.CheckedNormal:
                                    FireAction(Anchor.OnUnfocused);
                                    break;
                            }
                            
                            break;
                        }
                }
            }
        }

        #endregion
    }
}
