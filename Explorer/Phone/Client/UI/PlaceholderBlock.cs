using System;
using System.Windows;
using System.Windows.Media;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.UI
{
    public class PlaceholderBlock : AtomicBlockBase
    {
        private int width = 1;
        private int height = 1;
        
        public PlaceholderBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            UnpackContent();
        }

        private void UnpackContent()
        {
            Background = new SolidColorBrush(Colors.Transparent);
            
            if (Definition != null)
            {
                string mode = Definition.RenderingHints[RenderingHintKey.Mode];

                if (!String.IsNullOrEmpty(mode))
                {
                    switch (mode.ToLowerInvariant())
                    {
                        case "systemtray":
                            height = 32;
                            break;

                        case "appbar":
                            height = 72;
                            break;

                        case "appbarmini":
                            height = 30;
                            break;
                    }
                }
            }
        }

        protected override void SwitchToState(BlockState state)
        {
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            int resX = 0, resY = 0;

            if (width <= availableSize.Width)
                resX = width;

            if (height <= availableSize.Height)
                resY = height;

            return new Size(resX, resY);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        #endregion
    }
}
