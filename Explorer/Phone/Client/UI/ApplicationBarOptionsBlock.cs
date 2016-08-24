using System.Windows;
using System.Windows.Media;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.UI
{
    public class ApplicationBarOptionsBlock : BlockBase
    {
        public Color? ForegroundColour { get; private set; }
        public Color? BackgroundColour { get; private set; }

        public override bool IsHidden { get { return true; } }

        private AtomicBlockDefinition Data
        {
            get { return Definition as AtomicBlockDefinition; }
        }

        public ApplicationBarOptionsBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            BackgroundColour = null;
            ForegroundColour = null;

            UnpackDefinition();
        }

        private void UnpackDefinition()
        {
            // extract colours
            if (Data != null)
            {
                AtomicBlockStateData stateData = Data[BlockState.Normal];
                
                if (stateData != null)
                {
                    if (stateData.ComponentForeground.HasValue)
                    {
                        PaintStyleResult res = ResolvePaintStyle(stateData.ComponentForeground.Value);

                        if ((res.Brush != null) && (res.Brush is SolidColorBrush))
                            ForegroundColour = ((SolidColorBrush)res.Brush).Color;
                    }

                    if (stateData.ComponentBackground.HasValue)
                    {
                        PaintStyleResult res = ResolvePaintStyle(stateData.ComponentBackground.Value);

                        if ((res.Brush != null) && (res.Brush is SolidColorBrush))
                            BackgroundColour = ((SolidColorBrush)res.Brush).Color;
                    }
                }
            }
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
