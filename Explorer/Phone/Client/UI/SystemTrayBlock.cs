using System;
using System.Windows;
using System.Windows.Media;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class SystemTrayBlock : AtomicBlockBase
    {
        public override bool IsHidden { get { return true; } }

        private AtomicBlockDefinition Data
        {
            get { return Definition as AtomicBlockDefinition; }
        }

        private bool isVisible = true;
        private Color? background = null;
        private Color? foreground = null;
        private double? opacity = null;

        private bool oldState = false;
        
        public SystemTrayBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            UnpackContent();
        }

        private void UnpackContent()
        {
            if ((Data != null) && Data.HasState(BlockState.Normal))
            {
                // visibility
                isVisible = !Data.RenderingHints[RenderingHintKey.Visibility].Equals(WaveConstant.False, StringComparison.InvariantCultureIgnoreCase);

                // opacity
                string opData = Data.RenderingHints[RenderingHintKey.Opacity];

                if (!String.IsNullOrEmpty(opData))
                {
                    double temp = 0;

                    if (Double.TryParse(opData, out temp))
                        opacity = temp;
                }

                // try to get colours
                AtomicBlockStateData state = Data[BlockState.Normal];

                if (state != null)
                {
                    if (state.ComponentBackground.HasValue && (state.ComponentBackground.Value.StyleType == PaletteEntryType.Colour))
                    {
                        PaintStyleResult res = ResolvePaintStyle(state.ComponentBackground.Value);

                        if ((res.Context == PaintStyleResult.Result.Colour) && (res.Brush is SolidColorBrush))
                            background = ((SolidColorBrush)res.Brush).Color;
                    }

                    if (state.ComponentForeground.HasValue && (state.ComponentForeground.Value.StyleType == PaletteEntryType.Colour))
                    {
                        PaintStyleResult res = ResolvePaintStyle(state.ComponentForeground.Value);

                        if ((res.Context == PaintStyleResult.Result.Colour) && (res.Brush is SolidColorBrush))
                            foreground = ((SolidColorBrush)res.Brush).Color;
                    }
                }
            }
        }

        protected override void SwitchToState(BlockState state)
        {
        }

        public override void SignalShow()
        {
            base.SignalShow();

            oldState = Core.UI.SystemTray.IsVisible;

            Core.UI.SystemTray.Set(background, foreground, opacity);
            Core.UI.SystemTray.IsVisible = isVisible;
        }

        public override void SignalHide()
        {
            base.SignalHide();

            Core.UI.SystemTray.IsVisible = oldState;
            Core.UI.SystemTray.Reset();
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        #endregion
    }
}
