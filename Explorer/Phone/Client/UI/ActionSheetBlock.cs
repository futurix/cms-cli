using System;
using System.Collections.Generic;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class ActionSheetBlock : ContainerBlock
    {
        public override bool IsHidden { get { return true; } }

        private AtomicBlock titleBlock = null;
        private List<AtomicBlock> actionBlocks = null;

        public ActionSheetBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            actionBlocks = new List<AtomicBlock>();
            
            UnpackContent();

            hostView.RegisterActionSheet(this);
        }

        private void UnpackContent()
        {
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
                    {
                        AtomicBlock newAtomic = newBlock as AtomicBlock;
                        AtomicBlockDefinition newDefinition = definition as AtomicBlockDefinition;

                        if ((newAtomic != null) && (newDefinition != null))
                        {
                            if (newDefinition.HintedType == UIHintedType.ActionSheetTitle)
                                titleBlock = newAtomic;
                            else
                                actionBlocks.Add(newAtomic);
                        }
                    }
                }
            }
        }

        public void Launch()
        {
            // caption
            string caption = (titleBlock != null) ? titleBlock.Caption : String.Empty;
            
            // generating list of actions
            Dictionary<string, int> data = null;

            if (actionBlocks.Count > 0)
            {
                data = new Dictionary<string, int>(actionBlocks.Count);
                
                for (int i = 0; i < actionBlocks.Count; i++)
                {
                    if (!String.IsNullOrWhiteSpace(actionBlocks[i].Caption))
                        data.Add(actionBlocks[i].Caption, i);
                }
            }

            Core.UI.ShowOverlay(new ActionSheet(caption, data, SheetCallback));
        }

        private void SheetCallback(int selectionIndex)
        {
            if ((selectionIndex >= 0) && (selectionIndex < actionBlocks.Count))
                actionBlocks[selectionIndex].FireAction(Anchor.Fire);
        }
    }

    public delegate void ActionSheetCallback(int selectionIndex);
}
