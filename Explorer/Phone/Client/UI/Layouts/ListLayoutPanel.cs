using System;
using System.Windows;
using System.Windows.Controls;
using Wave.Platform;

namespace Wave.UI
{
    public class ListLayoutPanel : Panel
    {
        private ListBlockDefinition definition = null;

        private ListBlock parentBlock = null;
        private ScrollViewer parentControl = null;

        public ListLayoutPanel(ListBlockDefinition def, ListBlock block, ScrollViewer scroll)
        {
            definition = def;
            parentBlock = block;
            parentControl = scroll;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if ((availableSize.Width == 0) || (availableSize.Height == 0))
                return new Size(0, 0);
            
            if (definition.IsHorizontal)
            {
                double totalWidth = 0, maximumHeight = 0;
                double widthHint = Double.PositiveInfinity;

                // if specified in definition - let's size all items based on total visible width of the parent control
                if (definition.NumberOfFullyVisibleChildrenInHorizontalMode > 0)
                {
                    double totalSpacingAndPadding = definition.Spacing * (Children.Count - 1) + definition.PaddingLeft + definition.PaddingRight;

                    if ((parentBlock.LatestAvailableSize.Width - totalSpacingAndPadding) >= definition.NumberOfFullyVisibleChildrenInHorizontalMode)
                        widthHint = (parentBlock.LatestAvailableSize.Width - totalSpacingAndPadding) / definition.NumberOfFullyVisibleChildrenInHorizontalMode;
                }

                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Measure(
                        new Size(
                            (Children[i] is SingleSlotBlock) ? Double.PositiveInfinity : widthHint,
                            availableSize.Height));

                    if (Children[i].DesiredSize.Height > maximumHeight)
                        maximumHeight = Children[i].DesiredSize.Height;

                    totalWidth += Children[i].DesiredSize.Width;

                    if (i != (Children.Count - 1))
                        totalWidth += definition.Spacing;
                }

                return new Size(totalWidth, maximumHeight);
            }
            else
            {
                double maximumWidth = 0, totalHeight = 0;
                
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Measure(new Size(availableSize.Width, Double.PositiveInfinity));

                    if (Children[i].DesiredSize.Width > maximumWidth)
                        maximumWidth = Children[i].DesiredSize.Width;

                    totalHeight += Children[i].DesiredSize.Height;

                    if (i != (Children.Count - 1))
                        totalHeight += definition.Spacing;
                }

                return new Size(maximumWidth, totalHeight);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (definition.IsHorizontal)
            {
                double x = 0;

                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Arrange(new Rect(x, 0, Children[i].DesiredSize.Width, finalSize.Height));

                    x += Children[i].DesiredSize.Width;

                    if (i != (Children.Count - 1))
                        x += definition.Spacing;
                }
            }
            else
            {
                double y = 0;

                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Arrange(new Rect(0, y, finalSize.Width, Children[i].DesiredSize.Height));

                    y += Children[i].DesiredSize.Height;

                    if (i != (Children.Count - 1))
                        y += definition.Spacing;
                }
            }
            
            return finalSize;
        }
    }
}
