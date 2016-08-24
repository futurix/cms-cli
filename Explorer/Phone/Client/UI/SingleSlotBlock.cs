using System;
using System.Windows;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class SingleSlotBlock : AtomicBlockBase
    {
        private SingleSlotBlockDefinition Data
        {
            get { return Definition as SingleSlotBlockDefinition; }
        }

        #region Private fields

        private DisplayDataCollection displayData = null;
        private RendererBase slot = null;

        #endregion

        public SingleSlotBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            if (Data != null)
            {
                // remember state availability
                hasNormalState = Data.HasState(BlockState.Normal);
                hasCheckedNormalState = Data.HasState(BlockState.CheckedNormal);
                hasFocusedState = Data.HasState(BlockState.Focused);
                hasCheckedFocusedState = Data.HasState(BlockState.CheckedFocused);
            }

            UnpackContent();
        }

        private void UnpackContent()
        {
            /*
			// set checked state
			m_fChecked = m_oDefinition.m_fCheckable && oContentFldList.getNumFieldOfType(CFieldList.c_nINT, CNaviAgent.c_fieldidIsChecked) > 0;
            */

            // parse display data
            displayData = DisplayData.Parse(Content);

            Core.UI.ReferencedContent.Add(displayData);
            ParentNode.Register(displayData);

            // check if this block should have initial focus
            bool initialFocus = Content[NaviAgentFieldID.IsFocused].AsBoolean() ?? false;

            // set default state
            //if (CanFocus && initialFocus)
            //    Focus();
            //else
                SwitchToState(BlockState.Normal);
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if ((slot == null) || (availableSize.Width == 0) || (availableSize.Height == 0))
                return new Size(0, 0);

            double width = Double.IsInfinity(availableSize.Width) ? availableSize.Width : availableSize.Width - Padding.Left - Padding.Right;
            double height = Double.IsInfinity(availableSize.Height) ? availableSize.Height : availableSize.Height - Padding.Top - Padding.Bottom;

            if ((width > 0) && (height > 0))
            {
                slot.Measure(new Size(width, height));

                MeasureBackground();

                return new Size(slot.DesiredSize.Width + Padding.Left + Padding.Right, slot.DesiredSize.Height + Padding.Top + Padding.Bottom);
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
                    slot.Arrange(new Rect(Padding.Left, Padding.Top, width, height));
                else
                    slot.Arrange(new Rect(0, 0, 0, 0));

                ArrangeBackground(finalSize);
            }

            return finalSize;
        }

        #endregion

        #region State management

        protected override void SwitchToState(BlockState state)
        {
            if (state == currentState)
                return;

            // clean-up
            WaveChildren.Clear();
            Children.Clear();

            // try to create new state
            SingleSlotBlockStateData stateData = Data[state];

            if (stateData != null)
            {
                // margins and paddings
                Margin = new Spacing(stateData.MarginLeft, stateData.MarginTop, stateData.MarginRight, stateData.MarginBottom);
                Padding = new Spacing(stateData.PaddingLeft, stateData.PaddingTop, stateData.PaddingRight, stateData.PaddingBottom);

                // create slot
                slot = Core.UIFactory.CreateRenderer(this, displayData[stateData.SlotIndex], null, stateData.SlotStyle, null);

                if (slot != null)
                {
                    WaveChildren.Add(slot);
                    Children.Add(slot);
                }

                // fire required anchors
                switch (currentState)
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

                // and finally save current state
                currentState = state;
            }
        }

        #endregion
    }
}
