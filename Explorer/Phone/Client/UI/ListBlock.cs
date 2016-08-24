using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class ListBlock : ContainerBlock
    {
        public Size LatestAvailableSize = Size.Empty;
        
        private ListBlockDefinition Data
        {
            get { return Definition as ListBlockDefinition; }
        }

        private ScrollViewer wrapper = null;
        private ListLayoutPanel panel = null;

        public ListBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            wrapper = new ScrollViewer();
            wrapper.BorderThickness = new Thickness(0);

            if (Data != null)
            {
                panel = new ListLayoutPanel(Data, this, wrapper);
                wrapper.Content = panel;
            }

            Children.Add(wrapper);

            UnpackDefinition();
            UnpackContent();
        }

        private void UnpackDefinition()
        {
            if (Data != null)
            {
                if (Data.IsHorizontal)
                {
                    wrapper.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    wrapper.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                else
                {
                    wrapper.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    wrapper.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                }

                if (Data.Background.HasValue)
                {
                    PaintStyleResult bgRes = ResolvePaintStyle(Data.Background.Value);

                    if (bgRes.Brush != null)
                        Background = bgRes.Brush;
                }

                ////
            }
        }

        private void UnpackContent()
        {
            ////
            
            // creating children
            List<FieldList> children = Content.GetItems<FieldList>(NaviAgentFieldID.BlockData);

            for (int i = 0; i < children.Count; i++)
            {
                int childDefinitionID = children[i][MessageOutFieldID.DefinitionID].AsInteger() ?? 0;
                DefinitionBase definition = Core.Definitions.Find(ParentNode.ApplicationID, childDefinitionID, true);
                
                if ((definition != null) && (definition is BlockDefinition))
                {
                    BlockBase newBlock = Core.UIFactory.CreateAndInitialiseBlock(Host, ParentNode, this, definition as BlockDefinition, children[i], false);

                    if (newBlock != null)
                        AddChildBlock(newBlock);
                }
            }
        }

        protected override void AddSynchronisedBlock(BlockBase childBlock)
        {
            panel.Children.Add(childBlock);
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            LatestAvailableSize = availableSize;
            
            if ((availableSize.Width == 0) || (availableSize.Height == 0))
                return new Size(0, 0);
            
            double width = Double.IsInfinity(availableSize.Width) ? availableSize.Width : availableSize.Width - Padding.Left - Padding.Right;
            double height = Double.IsInfinity(availableSize.Height) ? availableSize.Height : availableSize.Height - Padding.Top - Padding.Bottom;

            if ((width > 0) && (height > 0))
            {
                wrapper.Measure(new Size(width, height));

                MeasureBackground();

                return new Size(wrapper.DesiredSize.Width + Padding.Left + Padding.Right, wrapper.DesiredSize.Height + Padding.Top + Padding.Bottom);
            }
            else
                return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if ((finalSize.Width > 0) && (finalSize.Height > 0))
            {
                double width = finalSize.Width - Padding.Left - Padding.Right;
                double height = finalSize.Height - Padding.Top - Padding.Bottom;

                if ((width > 0) && (height > 0))
                    wrapper.Arrange(new Rect(Padding.Left, Padding.Top, width, height));
                else
                    wrapper.Arrange(new Rect(0, 0, 0, 0));

                ArrangeBackground(finalSize);
            }

            return finalSize;
        }

        #endregion
    }
}
