using System;
using System.Collections.Generic;
using System.Windows;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class BoxLayoutBlock : ContainerBlock
    {
        private BoxLayoutBlockDefinition Data
        {
            get { return Definition as BoxLayoutBlockDefinition; }
        }

        public BoxLayoutBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            UnpackDefinition();
            UnpackContent();
        }

        private void UnpackDefinition()
        {
            if (Data != null)
            {
                // applying definition
                if (Data.Background.HasValue)
                {
                    PaintStyleResult bgRes = ResolvePaintStyle(Data.Background.Value);

                    if (bgRes.Brush != null)
                        Background = bgRes.Brush;
                }
                
                //IMPLEMENT: rest of box layout definition
            }
        }

        private void UnpackContent()
        {
			//m_nChildInFocus = m_oDefinition.m_nFirstFocus;
            
            // unpack palettes
            List<Int32Field> paletteIDs = Content.GetItems<Int32Field>(NaviAgentFieldID.PaletteID);

            if (paletteIDs.Count > 0)
            {
                // application palette
                if (ParentNode != null)
                    ParentNode.ApplicationPalette = Core.Definitions.Find(ParentNode.ApplicationID, paletteIDs[0].Data) as PaletteDefinition;

                // other palette(s)
                if (paletteIDs.Count > 1)
                {
                    for (int i = 1; i < paletteIDs.Count; i++)
                        Palette = Core.Definitions.Find(ParentNode.ApplicationID, paletteIDs[i].Data) as PaletteDefinition;
                }
            }

            // unpack children
            List<FieldList> children = Content.GetItems<FieldList>(NaviAgentFieldID.BlockData);

            foreach (FieldList childData in children)
            {
                int childDefinitionID = childData[MessageOutFieldID.DefinitionID].AsInteger() ?? 0;
                DefinitionBase definition = Core.Definitions.Find(ParentNode.ApplicationID, childDefinitionID, true);

                if ((definition != null) && (definition is BlockDefinition))
                {
                    BlockBase newBlock = Core.UIFactory.CreateAndInitialiseBlock(Host, ParentNode, this, definition as BlockDefinition, childData, false);

                    if (newBlock != null)
                        AddChildBlock(newBlock);
                }
            }
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if ((Data == null) || (availableSize.Width == 0) || (availableSize.Height == 0))
            {
                return new Size(0, 0);
            }
            else if (Double.IsInfinity(availableSize.Width) || Double.IsInfinity(availableSize.Height))
            {
                return CalculateUnboundedLayout(availableSize, true);
            }
            else
            {
                // calculate size without padding
                Size totalSize = new Size(
                    availableSize.Width - Padding.Left - Padding.Right,
                    availableSize.Height - Padding.Top - Padding.Left);

                // update layout in children
                CalculateBoundedLayout(totalSize, true);

                // background
                MeasureBackground();

                // always take all possible area if bounded
                return availableSize;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if ((finalSize.Width > 0) && (finalSize.Height > 0) && (WaveChildren.Count > 0) && (Data != null) && (Data.SizeRatios.Count > 0))
            {
                // calculate final size without padding
                Size totalSize = new Size(
                    finalSize.Width - Padding.Left - Padding.Right,
                    finalSize.Height - Padding.Top - Padding.Left);

                // calculate the layout
                int[] sizes = CalculateBoundedLayout(totalSize, false);

                // background
                ArrangeBackground(finalSize);

                // do the actual layout
                if (Data.IsHorizontal)
                {
                    // horizontal
                    double x = Padding.Left;

                    for (int i = 0; i < sizes.Length; i++)
                    {
                        if (i < WaveChildren.Count)
                        {
                            WaveChildren[i].Arrange(new Rect(x, 0, sizes[i], totalSize.Height));

                            x += sizes[i];
                        }
                    }
                }
                else
                {
                    // vertical
                    double y = Padding.Top;

                    for (int i = 0; i < sizes.Length; i++)
                    {
                        if (i < WaveChildren.Count)
                        {
                            WaveChildren[i].Arrange(new Rect(0, y, totalSize.Width, sizes[i]));

                            y += sizes[i];
                        }
                    }
                }
            }

            return finalSize;
        }

        private int[] CalculateBoundedLayout(Size totalSize, bool isMeasure)
        {
            int flexibleRemainder = 0, finalRemainder = 0, fraction = 0, fractions = 0;
            double childWidth = 0, childHeight = 0;
            int[] sizes = new int[Data.SizeRatios.Count];

            // prepare test values
            if (Data.IsHorizontal)
            {
                flexibleRemainder = (int)totalSize.Width;
                childWidth = Double.PositiveInfinity;
                childHeight = totalSize.Height;
            }
            else
            {
                flexibleRemainder = (int)totalSize.Height;
                childWidth = totalSize.Width;
                childHeight = Double.PositiveInfinity;
            }

            // calculate absolute sizes and total number of relative fractions
            for (int i = 0; i < Data.SizeRatios.Count; i++)
            {
                if (i < WaveChildren.Count)
                {
                    if (Data.SizeRatios[i] == 0)
                    {
                        WaveChildren[i].Measure(new Size(childWidth, childHeight));
                        sizes[i] = (int)(Data.IsHorizontal ? WaveChildren[i].DesiredSize.Width : WaveChildren[i].DesiredSize.Height);

                        flexibleRemainder -= sizes[i];
                    }
                    else
                        fractions += Data.SizeRatios[i];
                }
            }

            // calculate size of single fraction
            if (flexibleRemainder > 0)
            {
                if (fractions > 0)
                    fraction = flexibleRemainder / fractions;
                else
                    fraction = flexibleRemainder;
            }
            else
                fraction = 0;

            // convert from relative sizes to absolute ones
            for (int i = 0; i < Data.SizeRatios.Count; i++)
            {
                if (Data.SizeRatios[i] != 0)
                    sizes[i] = fraction * Data.SizeRatios[i];
            }

            // check for remaining pixels and add them to the last relative item :-)
            finalRemainder = (int)(Data.IsHorizontal ? totalSize.Width : totalSize.Height);

            for (int i = 0; i < sizes.Length; i++)
                finalRemainder -= sizes[i];

            if (finalRemainder > 0)
            {
                for (int i = Data.SizeRatios.Count - 1; i >= 0; i--)
                {
                    if (Data.SizeRatios[i] != 0)
                    {
                        sizes[i] += finalRemainder;
                        finalRemainder = 0;

                        break;
                    }
                }
            }

            // if there was no relative items, add to the last item regardless of its ratio
            if (finalRemainder > 0)
                sizes[sizes.Length - 1] += finalRemainder;

            // measure relative children (this is needed to calculate the layout of their nested children)
            if (isMeasure)
            {
                for (int i = 0; i < Data.SizeRatios.Count; i++)
                {
                    if ((i < WaveChildren.Count) && (Data.SizeRatios[i] != 0))
                    {
                        if (Data.IsHorizontal)
                            WaveChildren[i].Measure(new Size(sizes[i], childHeight));
                        else
                            WaveChildren[i].Measure(new Size(childWidth, sizes[i]));
                    }
                }
            }

            return sizes;
        }

        private Size CalculateUnboundedLayout(Size availableSize, bool isMeasure)
        {
            if (Double.IsInfinity(availableSize.Width) && Double.IsInfinity(availableSize.Height))
                return new Size(0, 0); // only layouts unbounded in one direction are supported

            if ((Data.IsHorizontal && Double.IsInfinity(availableSize.Height)) || (!Data.IsHorizontal && Double.IsInfinity(availableSize.Width)))
            {
                // unbounded in secondary direction
                int flexibleRemainder = 0, otherDimension = 0, finalRemainder = 0, fraction = 0, fractions = 0;
                int[] sizes = new int[Data.SizeRatios.Count];

                if (Data.IsHorizontal)
                    flexibleRemainder = (int)availableSize.Width;
                else
                    flexibleRemainder = (int)availableSize.Height;

                // calculate absolute sizes and total number of relative fractions
                for (int i = 0; i < Data.SizeRatios.Count; i++)
                {
                    if (i < WaveChildren.Count)
                    {
                        if (Data.SizeRatios[i] == 0)
                        {
                            WaveChildren[i].Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                            sizes[i] = (int)(Data.IsHorizontal ? WaveChildren[i].DesiredSize.Width : WaveChildren[i].DesiredSize.Height);

                            if ((Data.IsHorizontal ? WaveChildren[i].DesiredSize.Height : WaveChildren[i].DesiredSize.Width) > otherDimension)
                                otherDimension = (int)(Data.IsHorizontal ? WaveChildren[i].DesiredSize.Height : WaveChildren[i].DesiredSize.Width);

                            flexibleRemainder -= sizes[i];
                        }
                        else
                            fractions += Data.SizeRatios[i];
                    }
                }

                // calculate size of single fraction
                if (flexibleRemainder > 0)
                {
                    if (fractions > 0)
                        fraction = flexibleRemainder / fractions;
                    else
                        fraction = flexibleRemainder;
                }
                else
                    fraction = 0;

                // convert from relative sizes to absolute ones
                for (int i = 0; i < Data.SizeRatios.Count; i++)
                {
                    if (Data.SizeRatios[i] != 0)
                        sizes[i] = fraction * Data.SizeRatios[i];
                }

                // check for remaining pixels and add them to the last relative item
                finalRemainder = (int)(Data.IsHorizontal ? availableSize.Width : availableSize.Height);

                for (int i = 0; i < sizes.Length; i++)
                    finalRemainder -= sizes[i];

                if (finalRemainder > 0)
                {
                    for (int i = Data.SizeRatios.Count - 1; i >= 0; i--)
                    {
                        if (Data.SizeRatios[i] != 0)
                        {
                            sizes[i] += finalRemainder;
                            finalRemainder = 0;

                            break;
                        }
                    }
                }

                // if there was no relative items, add to the last item regardless of its ratio
                if (finalRemainder > 0)
                    sizes[sizes.Length - 1] += finalRemainder;

                // measure relative children (this is needed to calculate the layout of their nested children)
                if (isMeasure)
                {
                    for (int i = 0; i < Data.SizeRatios.Count; i++)
                    {
                        if ((i < WaveChildren.Count) && (Data.SizeRatios[i] != 0))
                        {
                            if (Data.IsHorizontal)
                                WaveChildren[i].Measure(new Size(sizes[i], Double.PositiveInfinity));
                            else
                                WaveChildren[i].Measure(new Size(Double.PositiveInfinity, sizes[i]));

                            if ((Data.IsHorizontal ? WaveChildren[i].DesiredSize.Height : WaveChildren[i].DesiredSize.Width) > otherDimension)
                                otherDimension = (int)(Data.IsHorizontal ? WaveChildren[i].DesiredSize.Height : WaveChildren[i].DesiredSize.Width);
                        }
                    }
                }

                // calculate control size
                if (Data.IsHorizontal)
                    return new Size(availableSize.Width, otherDimension);
                else
                    return new Size(otherDimension, availableSize.Height);
            }
            else
                return new Size(0, 0); // box layout unbounded in primary direction is unsupported
        }

        #endregion
    }
}
