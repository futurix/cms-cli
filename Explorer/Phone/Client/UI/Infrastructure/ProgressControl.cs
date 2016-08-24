using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Wave.Common;

namespace Wave.UI
{
    public class ProgressControl : Panel
    {
        public bool IsEnabled
        {
            get { return (Visibility == Visibility.Visible); }
            set
            {
                Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                progress.IsIndeterminate = value;
            }
        }

        public bool IsShaded
        {
            get { return background.Fill != null; }
            set { background.Fill = value ? (Brush)ResourceHelper.Find("PhoneSemitransparentBrush") : null; }
        }

        private Rectangle background = null;
        private PerformanceProgressBar progress = null;

        public ProgressControl()
        {
            // creating background
            background = new Rectangle();
            background.StrokeThickness = 0;
            background.Stretch = Stretch.Fill;
            background.HorizontalAlignment = HorizontalAlignment.Stretch;
            background.VerticalAlignment = VerticalAlignment.Stretch;

            // creating progress bar
            progress = new PerformanceProgressBar();
            progress.IsIndeterminate = true;
            progress.HorizontalAlignment = HorizontalAlignment.Stretch;
            progress.VerticalAlignment = VerticalAlignment.Center;

            // enable shading by default
            IsShaded = true;

            // ready to go
            Children.Add(background);
            Children.Add(progress);
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if (!availableSize.IsEmpty && !Double.IsInfinity(availableSize.Width) && !Double.IsInfinity(availableSize.Height))
            {
                background.Measure(availableSize);
                progress.Measure(availableSize);
                
                return availableSize;
            }
            else
                return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (!finalSize.IsEmpty)
            {
                background.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
                progress.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }
            
            return finalSize;
        }

        #endregion
    }
}
