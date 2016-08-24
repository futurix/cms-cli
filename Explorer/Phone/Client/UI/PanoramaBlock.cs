using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Phone.Controls;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class PanoramaBlock : ContainerBlock
    {
        private Panorama panorama = null;

        public PanoramaBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            UnpackContent();
        }

        private void UnpackContent()
        {
            // creating children
            List<FieldList> children = Content.GetItems<FieldList>(NaviAgentFieldID.BlockData);

            AtomicBlock title = null;
            List<AtomicBlock> itemTitles = new List<AtomicBlock>();
            List<FrameBlock> items = new List<FrameBlock>();

            for (int i = 0; i < children.Count; i++)
            {
                int childDefinitionID = children[i][MessageOutFieldID.DefinitionID].AsInteger() ?? 0;
                DefinitionBase definition = Core.Definitions.Find(ParentNode.ApplicationID, childDefinitionID, true);

                if ((definition != null) && (definition is BlockDefinition))
                {
                    BlockBase newBlock = Core.UIFactory.CreateAndInitialiseBlock(Host, ParentNode, this, definition as BlockDefinition, children[i], false);

                    if (newBlock != null)
                    {
                        if (newBlock is AtomicBlock)
                        {
                            if (((BlockDefinition)definition).HintedType == UIHintedType.PanoramaTitle)
                                title = (AtomicBlock)newBlock;
                            else
                                itemTitles.Add((AtomicBlock)newBlock);
                        }
                        else if (newBlock is FrameBlock)
                            items.Add((FrameBlock)newBlock);
                    }
                }
            }

            if ((title != null) && !String.IsNullOrWhiteSpace(title.Caption) && (itemTitles.Count > 0) && (items.Count > 0) && (itemTitles.Count == items.Count))
            {
                panorama = new Panorama();
                panorama.Title = title.Caption;
                
                for (int i = 0; i < items.Count; i++)
                {
                    PanoramaItem item = new PanoramaItem();

                    item.Header = !String.IsNullOrWhiteSpace(itemTitles[i].Caption) ? itemTitles[i].Caption : WaveConstant.UnknownText;
                    item.Content = items[i];

                    panorama.Items.Add(item);
                }
                
                if ((ContainerDefinition != null) && ContainerDefinition.Background.HasValue)
                {
                    PaintStyleResult bgRes = ResolvePaintStyle(ContainerDefinition.Background.Value);

                    if (bgRes.Brush != null)
                        panorama.Background = bgRes.Brush;
                }

                Children.Add(panorama);

                for (int i = 0; i < items.Count; i++)
                    WaveChildren.Add(items[i]);
            }
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if (panorama != null)
            {
                panorama.Measure(availableSize);

                return panorama.DesiredSize;
            }
            else
                return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (panorama != null)
                panorama.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

            return finalSize;
        }

        #endregion
    }
}
