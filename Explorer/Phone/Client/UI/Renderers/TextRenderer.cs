using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class TextRenderer : RendererBase
    {
        public string Caption
        {
            get { return textControl.Text; }
            protected set { textControl.Text = value; }
        }

        protected TextBlock textControl = null;

        public TextRenderer(AtomicBlockBase atomic)
            : base(atomic)
        {
            textControl = new TextBlock();

            Children.Add(textControl);
        }

        public override void ApplyFormattingAndLayout(SlotData slotData, SlotStyleData slotStyleData, TableLayoutItemInfo layout)
        {
            base.ApplyFormattingAndLayout(slotData, slotStyleData, layout);

            Brush background = null;
            Brush foreground = null;
            short fontID = -1;

            //IMPLEMENT: the rest of the properties in SlotData, SlotStyleData, and TableLayoutItemInfo

            if (layout != null)
            {
                // margins and paddings
                Margin = new Spacing(layout.LeftMargin, layout.TopMargin, layout.RightMargin, layout.BottomMargin);
                Padding = new Spacing(layout.LeftPadding, layout.TopPadding, layout.RightPadding, layout.BottomPadding);
                
                // cropping
                Crop = layout.CropStrategy;
            }

            if (slotData != null)
            {
                // background
                if (slotData.Background.HasValue)
                {
                    PaintStyleResult bgRes = ResolvePaintStyle(slotData.Background.Value, PaintStyleUse.Background);
                        
                    if (bgRes.Brush != null)
                        background = bgRes.Brush;
                }

                // foreground
                if (slotData.Foreground.HasValue)
                {
                    PaintStyleResult fgRes = ResolvePaintStyle(slotData.Foreground.Value, PaintStyleUse.Foreground);
                    
                    if (fgRes.Brush != null)
                        foreground = fgRes.Brush;
                }
                
                // get font
                fontID = slotData.Font ?? -1;
            }

            if (slotStyleData != null)
            {
                // cropping
                Crop = slotStyleData.Crop;

                // background
                if (slotStyleData.Background.HasValue)
                {
                    PaintStyleResult bgRes = ResolvePaintStyle(slotStyleData.Background.Value, PaintStyleUse.Background);

                    if (bgRes.Brush != null)
                        background = bgRes.Brush;
                }

                // foreground
                if (slotStyleData.Foreground.HasValue)
                {
                    PaintStyleResult fgRes = ResolvePaintStyle(slotStyleData.Foreground.Value, PaintStyleUse.Foreground);

                    if (fgRes.Brush != null)
                        foreground = fgRes.Brush;
                }

                // get font
                fontID = slotStyleData.Font;
            }

            // use parent foreground if needed
            if (foreground == null)
                foreground = Atomic.Foreground;

            // apply brushes
            Background = background;
            textControl.Foreground = foreground;

            // apply font
            if (fontID != -1)
            {
                FontReferencePaletteEntry fr = Atomic.FindCascadedPaletteItem(fontID) as FontReferencePaletteEntry;

                if (fr != null)
                {
                    FontDefinition fd = fr.Resolve(Atomic.ParentNode.ApplicationID);

                    if (fd != null)
                        fd.Apply(textControl);
                }
            }

            // apply cropping, alignment, tick
            if ((Crop & CropStrategy.Wrap) == CropStrategy.Wrap)
            {
                textControl.TextWrapping = TextWrapping.Wrap;

                if ((Crop & CropStrategy.Tick) == CropStrategy.Tick)
                    textControl.TextTrimming = TextTrimming.WordEllipsis;
                else
                    textControl.TextTrimming = TextTrimming.None;
            }
            else if ((Crop & CropStrategy.Tick) == CropStrategy.Tick)
            {
                textControl.TextWrapping = TextWrapping.NoWrap;
                textControl.TextTrimming = TextTrimming.WordEllipsis;
            }
            else
            {
                textControl.TextWrapping = TextWrapping.NoWrap;
                textControl.TextTrimming = TextTrimming.None;
            }

            // set horizontal alignment (vertical one is done in layout)
            if ((Crop & CropStrategy.AlignLeft) == CropStrategy.AlignLeft)
                textControl.TextAlignment = TextAlignment.Left;
            else if ((Crop & CropStrategy.AlignHCenter) == CropStrategy.AlignHCenter)
                textControl.TextAlignment = TextAlignment.Center;
            else if ((Crop & CropStrategy.AlignRight) == CropStrategy.AlignRight)
                textControl.TextAlignment = TextAlignment.Right;
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if ((availableSize.Width == 0) || (availableSize.Height == 0))
                return new Size(0, 0);
            
            if (textControl.TextWrapping == TextWrapping.Wrap)
            {
                double availableWidth = Double.IsInfinity(availableSize.Width) ? availableSize.Width : availableSize.Width - Padding.Left - Padding.Right;
                double availableHeight = Double.IsInfinity(availableSize.Height) ? availableSize.Height : availableSize.Height - Padding.Top - Padding.Bottom;

                if (availableWidth < 0)
                    availableWidth = 0;

                if (availableHeight < 0)
                    availableHeight = 0;

                textControl.Measure(new Size(availableWidth, availableHeight));

                double width = 0, height = 0;

                if (Double.IsInfinity(availableSize.Width) || ((textControl.DesiredSize.Width + Padding.Left + Padding.Right) <= availableSize.Width))
                    width = textControl.DesiredSize.Width + Padding.Left + Padding.Right;
                else
                    width = availableSize.Width;

                if (Double.IsInfinity(availableSize.Height) || ((textControl.DesiredSize.Height + Padding.Top + Padding.Bottom) <= availableSize.Height))
                    height = textControl.DesiredSize.Height + Padding.Top + Padding.Bottom;
                else
                    height = availableSize.Height;

                return new Size(width, height);
            }
            else
            {
                textControl.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

                double width = 0, height = 0, idealWidth = 0, idealHeight = 0;

                idealWidth = textControl.DesiredSize.Width + Padding.Left + Padding.Right;
                idealHeight = textControl.DesiredSize.Height + Padding.Top + Padding.Bottom;

                if (Double.IsInfinity(availableSize.Width) || (idealWidth <= availableSize.Width))
                    width = idealWidth;
                else
                    width = availableSize.Width;

                if (Double.IsInfinity(availableSize.Height) || (idealHeight <= availableSize.Height))
                    height = idealHeight;
                else
                    height = availableSize.Height;

                return new Size(width, height);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if ((finalSize.Width == 0) || (finalSize.Height == 0))
            {
                textControl.Arrange(new Rect(0, 0, 0, 0));

                return finalSize;
            }

            Rect finalArea = new Rect(
                (finalSize.Width > Padding.Left) ? Padding.Left : 0,
                (finalSize.Height > Padding.Top) ? Padding.Top : 0,
                (finalSize.Width > (Padding.Left + Padding.Right)) ? finalSize.Width - Padding.Left - Padding.Right : finalSize.Width,
                (finalSize.Height > (Padding.Top + Padding.Bottom)) ? finalSize.Height - Padding.Top - Padding.Bottom : finalSize.Height);
            Size textSize = textControl.DesiredSize;

            if (textControl.TextWrapping == TextWrapping.Wrap)
            {
                // wrap
                textControl.Arrange(finalArea);
            }
            else
            {
                double x = 0, y = 0, width = 0, height = 0;
                Rect? clip = null;

                if (textControl.TextTrimming == TextTrimming.WordEllipsis)
                {
                    // tick
                    width = finalArea.Width;
                    x = finalArea.X;

                    if (textSize.Height > finalArea.Height)
                    {
                        height = finalArea.Height;
                        y = finalArea.Y;
                    }
                    else
                    {
                        height = textSize.Height;

                        if ((Crop & CropStrategy.AlignTop) == CropStrategy.AlignTop)
                            y = finalArea.Y;
                        else if ((Crop & CropStrategy.AlignVCenter) == CropStrategy.AlignVCenter)
                            y = finalArea.Y + ((finalArea.Height - textSize.Height) / 2);
                        else if ((Crop & CropStrategy.AlignBottom) == CropStrategy.AlignBottom)
                            y = finalArea.Y + finalArea.Height - textSize.Height;
                    }
                }
                else
                {
                    // crop
                    bool applyClip = false;
                    double clipX = 0, clipY = 0;

                    if (textSize.Width > finalArea.Width)
                    {
                        width = textSize.Width;
                        applyClip = true;

                        if ((Crop & CropStrategy.CropLeft) == CropStrategy.CropLeft)
                        {
                            x = -(textSize.Width - finalSize.Width);
                            clipX = textSize.Width - finalSize.Width - Padding.Right;
                        }
                        else if ((Crop & CropStrategy.CropRight) == CropStrategy.CropRight)
                        {
                            x = 0;
                            clipX = Padding.Left;
                        }
                        else
                        {
                            x = -((textSize.Width - finalSize.Width) / 2);
                            clipX = ((textSize.Width - finalSize.Width) / 2) + Padding.Left;
                        }
                    }
                    else
                    {
                        width = finalArea.Width;
                        x = Padding.Left;
                    }

                    if (textSize.Height > finalArea.Height)
                    {
                        height = textSize.Height;
                        applyClip = true;

                        if ((Crop & CropStrategy.CropTop) == CropStrategy.CropTop)
                        {
                            y = -(textSize.Height - finalSize.Height);
                            clipY = textSize.Height - finalSize.Height - Padding.Bottom;
                        }
                        else if ((Crop & CropStrategy.CropBottom) == CropStrategy.CropBottom)
                        {
                            y = 0;
                            clipY = Padding.Top;
                        }
                        else
                        {
                            y = -((textSize.Height - finalSize.Height) / 2);
                            clipY = ((textSize.Height - finalSize.Height) / 2) + Padding.Top;
                        }
                    }
                    else if (textSize.Height < finalArea.Height)
                    {
                        height = textSize.Height;

                        if ((Crop & CropStrategy.AlignTop) == CropStrategy.AlignTop)
                            y = finalArea.Y;
                        else if ((Crop & CropStrategy.AlignVCenter) == CropStrategy.AlignVCenter)
                            y = finalArea.Y + ((finalArea.Height - textSize.Height) / 2);
                        else if ((Crop & CropStrategy.AlignBottom) == CropStrategy.AlignBottom)
                            y = finalArea.Y + finalArea.Height - textSize.Height;
                        else
                            y = Padding.Top;
                    }
                    else
                    {
                        height = finalArea.Height;
                        y = finalArea.Y;
                    }

                    if (applyClip)
                        clip = new Rect(clipX, clipY, finalArea.Width, finalArea.Height);
                }

                textControl.Arrange(new Rect(x, y, width, height));

                // apply padding if needed
                if (clip.HasValue)
                {
                    RectangleGeometry clipGeometry = new RectangleGeometry();
                    clipGeometry.Rect = clip.Value;

                    textControl.Clip = clipGeometry;
                }
            }

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
            string caption = null;
            
            if (data != null)
            {
                if ((data.DisplayType == DisplayType.String) && (data.Data is string))
                {
                    caption = data.Data as string;
                }
                else
                {
                    ContentReference cref = null;

                    if ((data.DisplayType == DisplayType.ContentReference) && (((ContentReference)data.Data).MediaType == MediaPrimitiveType.String))
                    {
                        cref = data.Data as ContentReference;
                    }
                    else if (data.DisplayType == DisplayType.MediaMetaData)
                    {
                        ContentReference candidate = ((MediaMetaData)data.Data)[Core.System.CurrentDeviceGroup];

                        if ((candidate != null) && (candidate.MediaType == MediaPrimitiveType.String))
                            cref = candidate;
                    }

                    if (cref != null)
                        caption = cref.ToText(data.DownloadedData as byte[]);
                }
            }

            if (caption != null)
                Caption = caption;
            else
                Caption = String.Empty;
        }

        #endregion
    }
}
