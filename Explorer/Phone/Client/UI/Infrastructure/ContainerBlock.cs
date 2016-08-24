using System;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.UI
{
    public abstract class ContainerBlock : BlockBase
    {
        protected ContainerBlockDefinition ContainerDefinition
        {
            get { return Definition as ContainerBlockDefinition; }
        }

        private BlockBase checkedChild = null;

        public ContainerBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            if (ContainerDefinition != null)
                ContainerDefinition.CopyMarginsAndPaddingToBlock(this);
        }

        public override void AttachFormData(short formID, FieldList storage)
        {
            base.AttachFormData(formID, storage);

            foreach (WaveControl child in WaveChildren)
            {
                if (child is BlockBase)
                    ((BlockBase)child).AttachFormData(formID, storage);
            }
        }

        public override void SignalTap(BlockBase block)
        {
            base.SignalTap(block);

            bool changeFocus = (block.CanFocus && !block.IsFocused);
            bool changeCheck = (block.CanCheck && !block.IsChecked);
            bool doRadio = (ContainerDefinition != null) ? ContainerDefinition.EnforceRadioBehaviour : false;

            checkedChild = null;

            if (changeFocus || changeCheck)
            {
                foreach (BlockBase child in WaveChildren)
                {
                    if (child == block)
                        child.SetState(
                            changeFocus ? true : child.IsFocused,
                            changeCheck ? true : child.IsChecked);
                    else
                        child.SetState(
                            changeFocus ? false : child.IsFocused,
                            changeCheck && doRadio ? false : child.IsChecked);

                    if (child.IsChecked)
                        checkedChild = child;
                }

                if (ParentBlock != null)
                    ParentBlock.SignalFocus(this);
            }
        }

        public override void SignalTapAndHold(BlockBase block)
        {
            base.SignalTapAndHold(block);

            if (block.CanFocus && !block.IsFocused)
            {
                foreach (BlockBase child in WaveChildren)
                {
                    if (child == block)
                        child.SetState(true, child.IsChecked);
                    else
                        child.SetState(false, child.IsChecked);
                }

                if (ParentBlock != null)
                    ParentBlock.SignalFocus(this);
            }
        }

        public override void SignalFocus(BlockBase block)
        {
            base.SignalFocus(block);

            foreach (BlockBase child in WaveChildren)
            {
                if (child != block)
                    child.SetState(false, child.IsChecked);
            }

            if (ParentBlock != null)
                ParentBlock.SignalFocus(this);
        }

        public override void SetState(bool isFocused, bool isChecked)
        {
            base.SetState(isFocused, isChecked);

            if (!isFocused)
            {
                foreach (BlockBase child in WaveChildren)
                    child.SetState(false, child.IsChecked);
            }
        }

        public override void SignalShow()
        {
            base.SignalShow();

            if (WaveChildren.Count > 0)
                foreach (BlockBase block in WaveChildren)
                    block.SignalShow();
        }

        public override void SignalHide()
        {
            base.SignalHide();

            if (WaveChildren.Count > 0)
                foreach (BlockBase block in WaveChildren)
                    block.SignalHide();
        }

        public override string ProcessSubmissionValue()
        {
            if (checkedChild != null)
                return checkedChild.ProcessSubmissionValue();
            else
                return String.Empty;
        }
    }
}
