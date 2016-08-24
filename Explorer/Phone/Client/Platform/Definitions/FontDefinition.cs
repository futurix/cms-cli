using System.IO;
using System.Windows;
using System.Windows.Controls;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    /// <summary>
    /// My name is German Bold Italic. I am a typeface.
    /// </summary>
    public class FontDefinition : DefinitionBase, ICacheable
    {
        public FontSize Size { get; private set; }
        public bool Bold { get; private set; }
        public bool Italic { get; private set; }

        public FontDefinition()
            : this(FontSize.Small, false, false)
        {
        }

        public FontDefinition(FontSize size, bool isBold, bool isItalic)
            : base()
        {
            Initialise(size, isBold, isItalic);
        }

        private void Initialise(FontSize size, bool isBold, bool isItalic)
        {
            Size = size;
            Bold = isBold;
            Italic = isItalic;
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            UnpackDefinitionID(source);

            // getting raw font data
            byte[] fontData = source[DefAgentFieldID.FontData].AsByteArray();

            if ((fontData != null) && (fontData.Length > 0))
            {
                FontValue fontValue = (FontValue)(fontData[0] & ValueMask);
                bool hasExtendedFontTypes = ((fontData[0] & MaskUseFullSet) == MaskUseFullSet);

                if (hasExtendedFontTypes)
                {
                    bool isItalic = ((fontData[0] & MaskUseItalic) == MaskUseItalic);
                    bool isBold = ((fontData[0] & MaskUseBold) == MaskUseBold);

                    switch (fontValue)
                    {
                        case FontValue.FullSetSmall:
                            Initialise(FontSize.Small, isBold, isItalic);
                            break;

                        case FontValue.FullSetMedium:
                            Initialise(FontSize.Medium, isBold, isItalic);
                            break;

                        case FontValue.FullSetLarge:
                            Initialise(FontSize.Large, isBold, isItalic);
                            break;

                        case FontValue.FullSetExtraSmall:
                            Initialise(FontSize.ExtraSmall, isBold, isItalic);
                            break;

                        case FontValue.FullSetExtraLarge:
                            Initialise(FontSize.ExtraLarge, isBold, isItalic);
                            break;

                        case FontValue.FullSetHuge:
                            Initialise(FontSize.Huge, isBold, isItalic);
                            break;

                        default:
                            DebugHelper.Out("Unknown specific font.");
                            goto case FontValue.FullSetSmall;
                    }
                }
                else
                {
                    switch (fontValue)
                    {
                        case FontValue.DefaultNormal:
                            Initialise(FontSize.Small, false, false);
                            break;

                        case FontValue.DefaultBold:
                            Initialise(FontSize.Small, true, false);
                            break;

                        case FontValue.DefaultLarge:
                            Initialise(FontSize.Medium, false, false);
                            break;
                        
                        default:
                            DebugHelper.Out("Unsupported default font.");
                            goto case FontValue.DefaultNormal;
                    }
                }
            }

            // done
            IsUnpacked = true;
        }

        public void Apply(Control ctrl)
        {
            FontData data = PrepareFontData();

            if (data != null)
            {
                ctrl.FontSize = data.Size;
                ctrl.FontWeight = data.Weight;
                ctrl.FontStyle = data.Style;
            }
        }

        public void Apply(TextBlock ctrl)
        {
            FontData data = PrepareFontData();

            if (data != null)
            {
                ctrl.FontSize = data.Size;
                ctrl.FontWeight = data.Weight;
                ctrl.FontStyle = data.Style;
            }
        }

        private FontData PrepareFontData()
        {
            FontData res = null;

            if (IsUnpacked)
            {
                res = new FontData();

                if (Core.IsMetro)
                {
                    switch (Size)
                    {
                        case FontSize.ExtraSmall:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeMedium");
                            break;

                        case FontSize.Small:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeMediumLarge");
                            break;

                        case FontSize.Medium:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeLarge");
                            break;

                        case FontSize.Large:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeExtraLarge");
                            break;

                        case FontSize.ExtraLarge:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeExtraExtraLarge");
                            break;

                        case FontSize.Huge:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeHuge");
                            break;
                    }
                }
                else
                {
                    switch (Size)
                    {
                        case FontSize.ExtraSmall:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeSmall");
                            break;

                        case FontSize.Small:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeNormal");
                            break;

                        case FontSize.Medium:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeMedium");
                            break;

                        case FontSize.Large:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeLarge");
                            break;

                        case FontSize.ExtraLarge:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeExtraLarge");
                            break;

                        case FontSize.Huge:
                            res.Size = (double)ResourceHelper.Find("PhoneFontSizeHuge");
                            break;
                    }
                }

                res.Weight = Bold ? FontWeights.Bold : FontWeights.Normal;
                res.Style = Italic ? FontStyles.Italic : FontStyles.Normal;
            }

            return res;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.FontDefinition; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteShort((short)Size);
            str.WriteBool(Bold);
            str.WriteBool(Italic);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                short fontSize = str.ReadShort();

                if (fontSize != -1)
                {
                    Size = (FontSize)fontSize;
                    Bold = str.ReadBool();
                    Italic = str.ReadBool();
                }
            }
        }

        #endregion

        #region Private types

        private class FontData
        {
            public double Size { get; set; }
            public FontWeight Weight { get; set; }
            public FontStyle Style { get; set; }
        }

        #region Unpacking values

        private enum FontValue
        {
            // 0000 0000
            DefaultNormal = 0,

            // 0000 0001
            DefaultBold = 1,

            // 0000 0010
            DefaultLarge = 2,

            // 0000 0000
            FullSetSmall = 0,

            // 0000 0001
            FullSetMedium = 1,

            // 0000 0010
            FullSetLarge = 2,

            // 0000 0011
            FullSetExtraSmall = 3,

            // 0000 0100
            FullSetExtraLarge = 4,

            // 0000 0101
            FullSetHuge = 5
        }

        private const int MaskUseFullSet = 0x40; // 0100 0000
        private const int MaskUseItalic = 0x20; // 0010 0000
        private const int MaskUseBold = 0x10; // 0001 0000

        private const byte ValueMask = (byte)(0x0F); // 0000 1111

        #endregion

        #endregion
    }

    public enum FontSize : short
    {
        ExtraSmall = 0,
        Small = 1,
        Medium = 2,
        Large = 3,
        ExtraLarge = 4,
        Huge = 5
    }
}
