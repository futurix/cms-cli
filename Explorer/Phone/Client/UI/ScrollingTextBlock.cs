using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.UI
{
    public class ScrollingTextBlock : BlockBase
    {
        public const int DefaultPadding = 4;
        
        public string Caption { get; private set; }
        
        private ScrollingTextBlockDefinition Data
        {
            get { return Definition as ScrollingTextBlockDefinition; }
        }

        private ScrollViewer wrapper = null;
        private StackPanel panel = null;

        public ScrollingTextBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            Caption = String.Empty;
            
            // parse content
            if (Content != null)
            {
                DisplayDataCollection ddc = DisplayData.Parse(Content);

                if ((ddc != null) && (ddc.Count > 0) && (ddc[0].DisplayType == DisplayType.String))
                    Caption = (string)ddc[0].Data;
            }

            // creating controls
            panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;

            wrapper = new ScrollViewer();
            wrapper.Content = panel;

            // apply definition
            Brush fg = null;
            FontDefinition fd = null;

            if (Data != null)
            {
                // background
                if (Data.Background.HasValue)
                {
                    PaintStyleResult bgRes = ResolvePaintStyle(Data.Background.Value);

                    if (bgRes.Brush != null)
                        Background = bgRes.Brush;
                }

                // foreground
                if (Data.Foreground.HasValue)
                {
                    PaintStyleResult fgRes = ResolvePaintStyle(Data.Foreground.Value);
                    
                    fg = fgRes.Brush;
                }

                // apply font
                if (Data.Font.HasValue && (Data.Font.Value != -1))
                {
                    FontReferencePaletteEntry fr = FindCascadedPaletteItem(Data.Font.Value) as FontReferencePaletteEntry;

                    if (fr != null)
                        fd = fr.Resolve(ParentNode.ApplicationID);
                }

                // padding for text
                panel.Margin = new Thickness(DefaultPadding);
            }

            // apply text
            string[] parts = Caption.Split('\n');

            foreach (string part in parts)
            {
                TextBlock paragraph = new TextBlock();

                if (fg != null)
                    paragraph.Foreground = fg;

                if (fd != null)
                    fd.Apply(paragraph);

                paragraph.TextWrapping = TextWrapping.Wrap;

                if (!String.IsNullOrEmpty(part))
                    paragraph.Text = part;
                else
                    paragraph.Text = " ";

                panel.Children.Add(paragraph);
            }

            // display text
            Children.Add(wrapper);
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if (!availableSize.IsEmpty && !Double.IsInfinity(availableSize.Width) && !Double.IsInfinity(availableSize.Height))
            {
                wrapper.Measure(availableSize);

                return availableSize;
            }
            else
                return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (!finalSize.IsEmpty)
            {
                wrapper.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

                return finalSize;
            }
            else
                return new Size(0, 0);
        }

        #endregion
    }
}
