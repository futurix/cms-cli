using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class ImageBackground : WaveControl
    {
        protected Image imageControl = null;
        
        public ImageBackground(DisplayData data)
        {
            if ((data != null) && ((data.DisplayType == DisplayType.ContentReference) || (data.DisplayType == DisplayType.MediaMetaData)))
            {
                imageControl = new Image();
                imageControl.HorizontalAlignment = HorizontalAlignment.Left;
                imageControl.VerticalAlignment = VerticalAlignment.Top;
                imageControl.Stretch = Stretch.None;

                UpdateImage(data);
                data.Updated += new EventHandler(data_Updated);

                Children.Add(imageControl);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            imageControl.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            imageControl.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            
            return finalSize;
        }

        private void UpdateImage(DisplayData data)
        {
            if (data != null)
            {
                ContentReference cref = null;

                if ((data.DisplayType == DisplayType.ContentReference) && (data.Data is ContentReference) &&
                    (((ContentReference)data.Data).MediaType == MediaPrimitiveType.Image))
                    cref = data.Data as ContentReference;

                if ((data.DisplayType == DisplayType.MediaMetaData) && (data.Data is MediaMetaData))
                {
                    ContentReference candidate = ((MediaMetaData)data.Data)[Core.System.CurrentDeviceGroup];

                    if ((candidate != null) && (candidate.MediaType == MediaPrimitiveType.Image))
                        cref = candidate;
                }

                if (cref != null)
                {
                    BitmapSource img = cref.ToBitmap(data.DownloadedData as byte[]);

                    if (img != null)
                        imageControl.Source = img;
                }
            }
        }

        private void data_Updated(object sender, EventArgs e)
        {
            UpdateImage(sender as DisplayData);
        }
    }
}
