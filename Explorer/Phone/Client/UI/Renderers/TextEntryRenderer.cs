using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wave.UI
{
    public class TextEntryRenderer : RendererBase
    {
        protected TextBox textControl = null;

        private bool hasTextHint = false;
        
        public TextEntryRenderer(AtomicBlockBase atomic)
            : base(atomic)
        {
            textControl = new TextBox();
            textControl.Text = Atomic.FormSubmissionHint ?? String.Empty;
            textControl.GotFocus += new RoutedEventHandler(textControl_GotFocus);
            textControl.TextChanged += new TextChangedEventHandler(textControl_TextChanged);

            hasTextHint = (Atomic.FormSubmissionHint != null);

            Children.Add(textControl);
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if ((availableSize.Width == 0) || (availableSize.Height == 0))
                return new Size(0, 0);

            textControl.Measure(availableSize);

            return textControl.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if ((finalSize.Width == 0) || (finalSize.Height == 0))
            {
                textControl.Arrange(new Rect(0, 0, 0, 0));

                return finalSize;
            }

            textControl.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

            return finalSize;
        }

        #endregion

        #region Event handlers

        private void textControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            Atomic.FormSubmissionValue = textControl.Text;
        }

        private void textControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (hasTextHint)
            {
                hasTextHint = false;

                textControl.Text = String.Empty;
            }
        }

        #endregion
    }
}
