using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class ImageRenderer : RendererBase
    {
        protected Image imageControl = null;

        protected Size ExpectedSize { get; set; }

        public ImageRenderer(AtomicBlockBase atomic)
            : base(atomic)
        {
            ExpectedSize = Size.Empty;
            
            imageControl = new Image();
            imageControl.Stretch = Stretch.None;
            imageControl.ImageOpened += (s, e) =>
            {
                ExpectedSize = new Size(imageControl.ActualWidth, imageControl.ActualHeight);
            };

            Children.Add(imageControl);
        }

        public override void ApplyFormattingAndLayout(SlotData slotData, SlotStyleData slotStyleData, TableLayoutItemInfo layout)
        {
            base.ApplyFormattingAndLayout(slotData, slotStyleData, layout);

            //IMPLEMENT: the rest of the properties in SlotData, SlotStyleData, and TableLayoutItemInfo

            if (layout != null)
            {
                Margin = new Spacing(layout.LeftMargin, layout.TopMargin, layout.RightMargin, layout.BottomMargin);
                Padding = new Spacing(layout.LeftPadding, layout.TopPadding, layout.RightPadding, layout.BottomPadding);
                Crop = layout.CropStrategy;
            }

            if (slotData != null)
            {
                // background
                if (slotData.Background.HasValue)
                {
                    PaintStyleResult bgRes = ResolvePaintStyle(slotData.Background.Value, PaintStyleUse.Background);

                    if (bgRes.Brush != null)
                        Background = bgRes.Brush;
                }
            }

            if (slotStyleData != null)
            {
                Crop = slotStyleData.Crop;

                // background
                if (slotStyleData.Background.HasValue)
                {
                    PaintStyleResult bgRes = ResolvePaintStyle(slotStyleData.Background.Value, PaintStyleUse.Background);

                    if (bgRes.Brush != null)
                        Background = bgRes.Brush;
                }
            }
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if ((availableSize.Width == 0) || (availableSize.Height == 0))
                return new Size(0, 0);
            
            imageControl.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

            Size imageSize = new Size(
                (imageControl.DesiredSize.Width >= 0) ? imageControl.DesiredSize.Width : ExpectedSize.Width,
                (imageControl.DesiredSize.Height >= 0) ? imageControl.DesiredSize.Height : ExpectedSize.Height
            );

            double width = 0, height = 0;

            if (Double.IsInfinity(availableSize.Width) || ((imageSize.Width + Padding.Left + Padding.Right) <= availableSize.Width))
                width = imageSize.Width + Padding.Left + Padding.Right;
            else
                width = availableSize.Width;

            if (Double.IsInfinity(availableSize.Height) || ((imageSize.Height + Padding.Top + Padding.Bottom) <= availableSize.Height))
                height = imageSize.Height + Padding.Top + Padding.Bottom;
            else
                height = availableSize.Height;

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if ((finalSize.Width == 0) || (finalSize.Height == 0))
            {
                imageControl.Arrange(new Rect(0, 0, 0, 0));

                return finalSize;
            }

            Rect finalArea = new Rect(
                (finalSize.Width > Padding.Left) ? Padding.Left : 0,
                (finalSize.Height > Padding.Top) ? Padding.Top : 0,
                (finalSize.Width > (Padding.Left + Padding.Right)) ? finalSize.Width - Padding.Left - Padding.Right : finalSize.Width,
                (finalSize.Height > (Padding.Top + Padding.Bottom)) ? finalSize.Height - Padding.Top - Padding.Bottom : finalSize.Height);
            Size imageSize = imageControl.DesiredSize;
            double x = 0, y = 0, clipX = 0, clipY = 0;
            bool applyClip = false;

            if (imageSize.Width > finalArea.Width)
            {
                applyClip = true;

                if ((Crop & CropStrategy.CropLeft) == CropStrategy.CropLeft)
                {
                    x = -(imageSize.Width - finalSize.Width);
                    clipX = imageSize.Width - finalSize.Width - Padding.Right;
                }
                else if ((Crop & CropStrategy.CropRight) == CropStrategy.CropRight)
                {
                    x = 0;
                    clipX = Padding.Left;
                }
                else
                {
                    x = -((imageSize.Width - finalSize.Width) / 2);
                    clipX = ((imageSize.Width - finalSize.Width) / 2) + Padding.Left;
                }
            }
            else
            {
                if ((Crop & CropStrategy.AlignLeft) == CropStrategy.AlignLeft)
                    x = finalArea.X;
                else if ((Crop & CropStrategy.AlignHCenter) == CropStrategy.AlignHCenter)
                    x = finalArea.X + ((finalArea.Width - imageSize.Width) / 2);
                else if ((Crop & CropStrategy.AlignRight) == CropStrategy.AlignRight)
                    x = finalArea.X + finalArea.Width - imageSize.Width;
                else
                    x = Padding.Left;
            }

            if (imageSize.Height > finalArea.Height)
            {
                applyClip = true;

                if ((Crop & CropStrategy.CropTop) == CropStrategy.CropTop)
                {
                    y = -(imageSize.Height - finalSize.Height);
                    clipY = imageSize.Height - finalSize.Height - Padding.Bottom;
                }
                else if ((Crop & CropStrategy.CropBottom) == CropStrategy.CropBottom)
                {
                    y = 0;
                    clipY = Padding.Top;
                }
                else
                {
                    y = -((imageSize.Height - finalSize.Height) / 2);
                    clipY = ((imageSize.Height - finalSize.Height) / 2) + Padding.Top;
                }
            }
            else if (imageSize.Height < finalArea.Height)
            {
                if ((Crop & CropStrategy.AlignTop) == CropStrategy.AlignTop)
                    y = finalArea.Y;
                else if ((Crop & CropStrategy.AlignVCenter) == CropStrategy.AlignVCenter)
                    y = finalArea.Y + ((finalArea.Height - imageSize.Height) / 2);
                else if ((Crop & CropStrategy.AlignBottom) == CropStrategy.AlignBottom)
                    y = finalArea.Y + finalArea.Height - imageSize.Height;
                else
                    y = Padding.Top;
            }
            else
            {
                y = finalArea.Y;
            }

            imageControl.Arrange(new Rect(x, y, imageSize.Width, imageSize.Height));

            if (applyClip)
            {
                RectangleGeometry clipGeometry = new RectangleGeometry();
                clipGeometry.Rect = new Rect(clipX, clipY, finalArea.Width, finalArea.Height);

                imageControl.Clip = clipGeometry;
            }
            else
                imageControl.Clip = null;

            return finalSize;
        }

        #endregion

        #region Data management

        protected override void OnDataInitialised(DisplayData data)
        {
            base.OnDataInitialised(data);

            SetData(data);
        }

        protected override void OnDataUpdated(DisplayData data)
        {
            base.OnDataUpdated(data);

            SetData(data);
        }

        private void SetData(DisplayData data)
        {
            ExpectedSize = Size.Empty;

            if (data != null)
            {
                ContentReference cref = null;
                
                if ((data.DisplayType == DisplayType.ContentReference) && (data.Data is ContentReference) &&
                    ((((ContentReference)data.Data).MediaType == MediaPrimitiveType.Image) || (((ContentReference)data.Data).MediaType == MediaPrimitiveType.ImageStrip)))
                    cref = data.Data as ContentReference;

                if ((data.DisplayType == DisplayType.MediaMetaData) && (data.Data is MediaMetaData))
                {
                    ContentReference candidate = ((MediaMetaData)data.Data)[Core.System.CurrentDeviceGroup];

                    if ((candidate != null) && ((candidate.MediaType == MediaPrimitiveType.Image) || (candidate.MediaType == MediaPrimitiveType.ImageStrip)))
                        cref = candidate;
                }

                if (cref != null)
                {
                    ExpectedSize = new Size(cref.MediaWidth, cref.MediaHeight);

                    BitmapSource img = cref.ToBitmap(data.DownloadedData as byte[]);

                    if (img != null)
                    {
                        ExpectedSize = new Size(img.PixelWidth, img.PixelHeight);

                        imageControl.Source = img;
                    }
                }
            }
        }

        #endregion
    }
}
