using System;
using System.Windows.Input;
using System.Windows.Media;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public abstract class AtomicBlockBase : BlockBase
    {
        public Brush Foreground { get; protected set; }

        public string Caption
        {
            get
            {
                if ((WaveChildren.Count > 0) && (WaveChildren[0] is TextRenderer))
                    return ((TextRenderer)WaveChildren[0]).Caption;
                else
                    return String.Empty;
            }
        }

        protected BlockState currentState = BlockState.NotLaidOut;

        protected bool hasNormalState = false;
        protected bool hasCheckedNormalState = false;
        protected bool hasFocusedState = false;
        protected bool hasCheckedFocusedState = false;

        private CommonAtomicBlockDefinition Data
        {
            get { return Definition as CommonAtomicBlockDefinition; }
        }

        public AtomicBlockBase(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            // setup events
            Tap += new EventHandler<GestureEventArgs>(AtomicBlock_Tap);
            Hold += new EventHandler<GestureEventArgs>(AtomicBlock_Hold);
        }

        #region Visibility

        public override void SignalHide()
        {
            base.SignalHide();

            bool needsToLoseFocus = false;

            for (int i = 0; i < WaveChildren.Count; i++)
            {
                if (WaveChildren[i] is TextEntryRenderer)
                {
                    needsToLoseFocus = true;
                    break;
                }
            }

            if (needsToLoseFocus)
                Core.UI.Focus();
        }

        #endregion

        #region State management

        public override void SetState(bool isFocused, bool isChecked)
        {
            BlockState newState = BlockState.NotLaidOut;

            if (isFocused && isChecked)
                newState = BlockState.CheckedFocused;
            else if (isFocused)
                newState = BlockState.Focused;
            else if (isChecked)
                newState = BlockState.CheckedNormal;
            else
                newState = BlockState.Normal;

            if ((newState != currentState) && (newState != BlockState.NotLaidOut))
                SwitchToState(newState);

            IsFocused = isFocused;
            IsChecked = isChecked;
        }

        protected abstract void SwitchToState(BlockState state);

        #endregion

        #region Forms

        public override string ProcessSubmissionValue()
        {
            if ((Data != null) && Data.IsCheckable && !IsChecked)
                return String.Empty;
            else
                return base.ProcessSubmissionValue();
        }

        #endregion

        #region Interactivity

        private void AtomicBlock_Tap(object sender, GestureEventArgs e)
        {
            if (ParentBlock != null)
                ParentBlock.SignalTap(this);

            e.Handled = FireAction(Anchor.Fire);
        }

        private void AtomicBlock_Hold(object sender, GestureEventArgs e)
        {
            if (ParentBlock != null)
            {
                ParentBlock.SignalTapAndHold(this);

                e.Handled = true;
            }
            else
                e.Handled = false;
        }

        #endregion
    }
}
