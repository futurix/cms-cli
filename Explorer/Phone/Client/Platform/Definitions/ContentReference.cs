using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class ContentReference : ICacheable
    {
        public byte[] ContentID { get; private set; }
        public MediaPrimitiveType MediaType { get; private set; }
        public MediaDelivery Delivery { get; private set; }

        public string FileName { get; private set; }

        public int DownloadSize { get; private set; }
        public string DownloadURL { get; private set; }

        public short MediaWidth { get; private set; }
        public short MediaHeight { get; private set; }

        public short ImageStripIndex { get; private set; }
        public short ImageStripTotal { get; private set; }

        public DeviceGroup DeviceGroup { get; private set; }

        public MediaFormat VectorMediaFormat { get; private set; }

        public ContentReference()
        {
        }

        public ContentReference(FieldList source)
        {
            Unpack(source);
        }

        public BitmapSource ToBitmap(byte[] imageData = null)
        {
            switch (MediaType)
            {
                case MediaPrimitiveType.Image:
                    {
                        BitmapImage res = null;

                        if ((Delivery == MediaDelivery.ViaHTTP) && !String.IsNullOrWhiteSpace(DownloadURL) && Uri.IsWellFormedUriString(DownloadURL, UriKind.Absolute))
                        {
                            res = new BitmapImage(new Uri(DownloadURL, UriKind.Absolute));
                        }
                        else if (Delivery == MediaDelivery.ResidentOnHandset)
                        {
                            string fileName = Core.Settings.ResolveResidentMedia(FileName, DeviceGroup);

                            if (!String.IsNullOrEmpty(fileName))
                                res = new BitmapImage(new Uri(fileName, UriKind.Relative));
                        }
                        else
                        {
                            // trying to find previously cached image first
                            BitmapImage img = null;
                            byte[] imgData = null;

                            if (imageData != null)
                                imgData = imageData;
                            else
                                imgData = Core.Cache.Server[ContentID] as byte[];

                            if (imgData != null)
                            {
                                using (MemoryStream ms = new MemoryStream(imgData))
                                {
                                    img = new BitmapImage();
                                    img.CreateOptions = BitmapCreateOptions.None;

                                    try
                                    {
                                        img.SetSource(ms);
                                    }
                                    catch
                                    {
                                        img = null;
                                    }
                                }
                            }

                            if (img != null)
                                res = img;
                        }

                        return res;
                    }

                case MediaPrimitiveType.ImageStrip:
                    {
                        byte[] rawImage = null;

                        if (Delivery == MediaDelivery.ResidentOnHandset)
                        {
                            string resName = Core.Settings.ResolveResidentMedia(FileName, DeviceGroup);

                            if (!String.IsNullOrEmpty(resName))
                            {
                                StreamResourceInfo templateConfig = Application.GetResourceStream(new Uri(resName, UriKind.Relative));

                                if ((templateConfig != null) && (templateConfig.Stream != null))
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        templateConfig.Stream.CopyTo(ms);

                                        rawImage = ms.ToArray();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (imageData != null)
                                rawImage = imageData;
                            else
                                rawImage = Core.Cache.Server[ContentID] as byte[];
                        }

                        if ((rawImage != null) && (rawImage.Length > 0) && (ImageStripTotal > 0))
                        {
                            WriteableBitmap temp = null;
                            
                            using (MemoryStream ms = new MemoryStream(rawImage))
                            {
                                BitmapImage bi = new BitmapImage();
                                bi.CreateOptions = BitmapCreateOptions.None;
                                bi.SetSource(ms);

                                temp = new WriteableBitmap(bi);
                            }

                            if (temp != null)
                            {
                                int width = temp.PixelWidth / ImageStripTotal;

                                if (width > 0)
                                    return temp.Crop(width * ImageStripIndex, 0, width, temp.PixelHeight);
                            }
                        }

                        return null;
                    }

                default:
                    return null;
            }
        }

        public string ToText(byte[] textData = null)
        {
            string res = null;

            if (Delivery == MediaDelivery.AutoDownload)
            {
                byte[] data = null;

                if (textData != null)
                    data = textData;
                else
                    data = Core.Cache.Server[ContentID] as byte[];

                if (data != null)
                    res = StringHelper.GetString(data);
            }

            //TODO: implement other relevant delivery types

            return res;
        }

        public override string ToString()
        {
            return String.Format("Content reference: media type -> {0}, delivery -> {1}", MediaType, Delivery);
        }

        private void Unpack(FieldList source)
        {
            // content reference ID
            ContentID = source[MessageOutFieldID.ContentReferenceID].AsByteArray();

            // media primitive type
            MediaType = (MediaPrimitiveType)(source[MessageOutFieldID.MediaPrimitiveType].AsShort() ?? (short)MediaPrimitiveType.Image);

            // media type format: we just need it for the vector layout
            VectorMediaFormat = (MediaFormat)(source[MessageOutFieldID.MediaTypeFormat].AsShort() ?? (short)MediaFormat.Unspecified);

            // delivery method
            Delivery = (MediaDelivery)(source[MessageOutFieldID.DeliveryMethod].AsShort() ?? (short)MediaDelivery.AutoDownload);

            switch (Delivery)
            {
                case MediaDelivery.ResidentOnHandset:
                    FileName = source[MessageOutFieldID.Filename].AsText();
                    break;

                case MediaDelivery.BackgroundDownload:
                case MediaDelivery.BulkRequestDownload:
                case MediaDelivery.ViaHTTP:
                case MediaDelivery.ViaHTTPBackground:
                    DownloadSize = source[MessageOutFieldID.DownloadSize].AsInteger() ?? 0;
                    DownloadURL = source[MessageOutFieldID.HTTPDeliveryURL].AsText();
                    break;
            }

            // presentational bits
            switch (MediaType)
            {
                case MediaPrimitiveType.ImageStrip:
                    ImageStripIndex = source[MessageOutFieldID.ImageStripIndex].AsShort() ?? 0;
                    ImageStripTotal = source[MessageOutFieldID.ImageStripTotal].AsShort() ?? 0;
                    goto case MediaPrimitiveType.Image;

                case MediaPrimitiveType.Image:
                case MediaPrimitiveType.Video:
                case MediaPrimitiveType.WVG:
                    MediaWidth = source[MessageOutFieldID.MediaActualWidth].AsShort() ?? 0;
                    MediaHeight = source[MessageOutFieldID.MediaActualHeight].AsShort() ?? 0;
                    break;
            }

            // device group
            DeviceGroup = (DeviceGroup)(source[NaviAgentFieldID.DeviceProfileGroup].AsShort() ?? (short)SystemAgent.DefaultDeviceGroup);
        }

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);

            if ((ContentID != null) && (ContentID.Length > 0))
            {
                str.WriteShort((short)ContentID.Length);
                str.WriteBytes(ContentID);
            }
            else
                str.WriteShort(0);

            str.WriteShort((short)MediaType);
            str.WriteShort((short)Delivery);
            BinaryHelper.WriteString(str, FileName);
            str.WriteInteger(DownloadSize);
            BinaryHelper.WriteString(str, DownloadURL);
            str.WriteShort(MediaWidth);
            str.WriteShort(MediaHeight);
            str.WriteShort(ImageStripIndex);
            str.WriteShort(ImageStripTotal);
            str.WriteShort((short)DeviceGroup);
            str.WriteShort((short)VectorMediaFormat);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                short contentIDSize = str.ReadShort();

                if (contentIDSize > 0)
                    ContentID = str.ReadBytes(contentIDSize);
                else
                    ContentID = null;

                MediaType = (MediaPrimitiveType)str.ReadShort();
                Delivery = (MediaDelivery)str.ReadShort();
                FileName = BinaryHelper.ReadString(str);
                DownloadSize = str.ReadInteger();
                DownloadURL = BinaryHelper.ReadString(str);
                MediaWidth = str.ReadShort();
                MediaHeight = str.ReadShort();
                ImageStripIndex = str.ReadShort();
                ImageStripTotal = str.ReadShort();
                DeviceGroup = (DeviceGroup)str.ReadShort();
                VectorMediaFormat = (MediaFormat)str.ReadShort();
            }
        }

        #endregion
    }
}
