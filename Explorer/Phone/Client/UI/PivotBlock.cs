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
    public class PivotBlock : ContainerBlock
    {
        private ContainerBlockDefinition Data
        {
            get { return Definition as ContainerBlockDefinition; }
        }
        
        private Pivot pivot = null;

        private bool loadOnDemand = false;

        public PivotBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            UnpackContent();
        }

        private void UnpackContent()
        {
            // parse settings
            if (Data != null)
                loadOnDemand = Data.RenderingHints[RenderingHintKey.OnDemand].Equals(WaveConstant.True, StringComparison.InvariantCultureIgnoreCase);
            
            // creating children
            List<FieldList> children = Content.GetItems<FieldList>(NaviAgentFieldID.BlockData);

            AtomicBlock title = null;
            List<AtomicBlock> itemTitles = new List<AtomicBlock>();
            List<FrameBlock> items = new List<FrameBlock>();

            for (int i = 0; i < children.Count; i++)
            {
                int childDefinitionID = children[i][MessageOutFieldID.DefinitionID].AsInteger() ?? 0;
                BlockDefinition definition = Core.Definitions.Find(ParentNode.ApplicationID, childDefinitionID, true) as BlockDefinition;

                if (definition != null)
                {
                    if (definition is FrameDefinition)
                    {
                        items.Add(new FrameBlock(Host, ParentNode, this, definition as FrameDefinition, children[i], false, !loadOnDemand));
                    }
                    else
                    {
                        AtomicBlock newBlock = Core.UIFactory.CreateAndInitialiseBlock(Host, ParentNode, this, definition, children[i], false) as AtomicBlock;

                        if (newBlock != null)
                        {
                            if (definition.HintedType == UIHintedType.PivotTitle)
                                title = newBlock;
                            else
                                itemTitles.Add(newBlock);
                        }
                    }
                }
            }

            if ((title != null) && !String.IsNullOrWhiteSpace(title.Caption) && (itemTitles.Count > 0) && (items.Count > 0) && (itemTitles.Count == items.Count))
            {
                pivot = new Pivot();
                pivot.Title = title.Caption;
                pivot.LoadingPivotItem += new EventHandler<PivotItemEventArgs>(pivot_LoadingPivotItem);

                for (int i = 0; i < items.Count; i++)
                {
                    PivotItem item = new PivotItem();

                    item.Header = !String.IsNullOrWhiteSpace(itemTitles[i].Caption) ? itemTitles[i].Caption : WaveConstant.UnknownText;
                    item.Content = items[i];
                    
                    pivot.Items.Add(item);
                }

                if ((ContainerDefinition != null) && ContainerDefinition.Background.HasValue)
                {
                    PaintStyleResult bgRes = ResolvePaintStyle(ContainerDefinition.Background.Value);

                    if (bgRes.Brush != null)
                        pivot.Background = bgRes.Brush;
                }

                Children.Add(pivot);

                for (int i = 0; i < items.Count; i++)
                    WaveChildren.Add(items[i]);
            }
        }

        private void pivot_LoadingPivotItem(object sender, PivotItemEventArgs e)
        {
            if ((e.Item != null) && (e.Item.Content is FrameBlock) && !((FrameBlock)e.Item.Content).HasInitiatedNavigation)
                ((FrameBlock)e.Item.Content).StartNavigation();
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if (pivot != null)
            {
                pivot.Measure(availableSize);

                return pivot.DesiredSize;
            }
            else
                return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (pivot != null)
                pivot.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            
            return finalSize;
        }

        #endregion
    }
}
