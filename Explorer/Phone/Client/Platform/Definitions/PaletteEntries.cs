using System.IO;
using System.Windows.Media;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public abstract class PaletteEntryBase : ICacheable
    {
        public PaletteEntryType EntryType { get; private set; }

        public PaletteEntryBase(PaletteEntryType et)
        {
            EntryType = et;
        }

        public virtual Brush ToBrush()
        {
            return null;
        }

        #region ICacheable implementation

        public virtual CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public virtual void Persist(Stream str)
        {
            str.WriteByte(0);
            str.WriteShort((short)EntryType);
        }

        public virtual void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
                EntryType = (PaletteEntryType)str.ReadShort();
        }

        #endregion
    }

    public class InheritedPaletteEntry : PaletteEntryBase, ICacheable
    {
        public InheritedPaletteEntry()
            : base(PaletteEntryType.Inherited)
        {
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.InheritedPaletteEntry; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            str.ReadByte();
        }

        #endregion
    }

    public class DoNotPaintPaletteEntry : PaletteEntryBase, ICacheable
    {
        public DoNotPaintPaletteEntry()
            : base(PaletteEntryType.DoNotPaint)
        {
        }

        public override Brush ToBrush()
        {
            return new SolidColorBrush(Colors.Transparent);
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.DoNotPaintPaletteEntry; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            str.ReadByte();
        }

        #endregion
    }

    public class FontReferencePaletteEntry : PaletteEntryBase, ICacheable
    {
        private int fontDefinitionID = -1;
        private FontDefinition fontDefinition = null;
        private bool searchAttempted = false;

        public FontReferencePaletteEntry()
            : this(-1)
        {
        }

        public FontReferencePaletteEntry(int defID)
            : base(PaletteEntryType.FontReference)
        {
            fontDefinitionID = defID;
        }

        public FontDefinition Resolve(int appID)
        {
            if (fontDefinition != null)
                return fontDefinition;

            if (!searchAttempted && (fontDefinitionID != -1))
            {
                searchAttempted = true;
                fontDefinition = Core.Definitions.Find(appID, fontDefinitionID) as FontDefinition;

                return fontDefinition;
            }

            return null;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.FontReferencePaletteEntry; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteInteger(fontDefinitionID);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                fontDefinitionID = str.ReadInteger();

                searchAttempted = false;
                fontDefinition = null;
            }
        }

        #endregion
    }

    public class ColourPaletteEntry : PaletteEntryBase, ICacheable
    {
        public Color Colour { get; private set; }

        public ColourPaletteEntry()
            : this(0)
        {
        }

        public ColourPaletteEntry(int source)
            : base(PaletteEntryType.Colour)
        {
            Colour = DataHelper.IntToColour(source);
        }

        public override Brush ToBrush()
        {
            return new SolidColorBrush(Colour);
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.ColourPaletteEntry; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteColour(Colour);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);
            
            if (str.ReadByte() == 0)
                Colour = str.ReadColour();
        }

        #endregion
    }

    public class LinearGradientPaletteEntry : PaletteEntryBase, ICacheable
    {
        public bool IsHorizontal { get; private set; }

        public PairedList<byte, Color> Colours = new PairedList<byte, Color>();

        public LinearGradientPaletteEntry()
            : base(PaletteEntryType.LinearGradient)
        {
        }

        public LinearGradientPaletteEntry(FieldList source)
            : base(PaletteEntryType.LinearGradient)
        {
            PairedList<ByteField, Int32Field> searchResults = 
                source.GetPairedItems<ByteField, Int32Field>(DefAgentFieldID.GradientKeyPosition, DefAgentFieldID.GradientKeyColour);

            foreach (Pair<ByteField, Int32Field> res in searchResults)
                Colours.Add(new Pair<byte, Color>(res.First.Data, DataHelper.IntToColour(res.Second.Data)));

            IsHorizontal = ((UIFlowDirection)(source[DefAgentFieldID.FlowDirection].AsByte() ?? (byte)UIFlowDirection.Right) == UIFlowDirection.Right);
        }

        public override Brush ToBrush()
        {
            GradientStopCollection stops = new GradientStopCollection();
            
            foreach (Pair<byte, Color> pair in Colours)
            {
                GradientStop stop = new GradientStop();

                stop.Color = pair.Second;
                stop.Offset = (double)pair.First / (double)100;

                stops.Add(stop);
            }

            return new LinearGradientBrush(stops, IsHorizontal ? 0 : 90);
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.LinearGradientPaletteEntry; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteBool(IsHorizontal);
            str.WriteInteger(Colours.Count);

            if (Colours.Count > 0)
            {
                foreach (var pair in Colours)
                {
                    str.WriteByte(pair.First);
                    str.WriteColour(pair.Second);
                }
            }
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                IsHorizontal = str.ReadBool();
                int numberOfColours = str.ReadInteger();

                if (numberOfColours > 0)
                    for (int i = 0; i < numberOfColours; i++)
                        Colours.Add(new Pair<byte, Color>((byte)str.ReadByte(), str.ReadColour()));
            }
        }

        #endregion
    }

    public class ColourSequencePaletteEntry : PaletteEntryBase, ICacheable
    {
        public int Period { get; private set; }
        public bool IsLoop { get; private set; }

        public PairedList<byte, Color> Colours = new PairedList<byte, Color>();

        public ColourSequencePaletteEntry()
            : base(PaletteEntryType.ColourSequence)
        {
        }

        public ColourSequencePaletteEntry(FieldList source)
            : base(PaletteEntryType.ColourSequence)
        {
            PairedList<ByteField, Int32Field> searchResults =
                source.GetPairedItems<ByteField, Int32Field>(DefAgentFieldID.GradientKeyPosition, DefAgentFieldID.GradientKeyColour);

            foreach (Pair<ByteField, Int32Field> res in searchResults)
                Colours.Add(new Pair<byte, Color>(res.First.Data, DataHelper.IntToColour(res.Second.Data)));

            Period = source[DefAgentFieldID.TimePeriod].AsShort() ?? 0;
            IsLoop = source[DefAgentFieldID.Loop].AsBoolean() ?? false;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.ColourSequencePaletteEntry; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteInteger(Period);
            str.WriteBool(IsLoop);
            str.WriteInteger(Colours.Count);

            if (Colours.Count > 0)
            {
                foreach (var pair in Colours)
                {
                    str.WriteByte(pair.First);
                    str.WriteColour(pair.Second);
                }
            }
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                Period = str.ReadInteger();
                IsLoop = str.ReadBool();
                int numberOfPairs = str.ReadInteger();

                if (numberOfPairs > 0)
                {
                    for (int i = 0; i < numberOfPairs; i++)
                        Colours.Add(
                            new Pair<byte, Color>((byte)str.ReadByte(), str.ReadColour()));
                }
            }
        }

        #endregion
    }

    public class PaletteReferencePaletteEntry : PaletteEntryBase, ICacheable
    {
        public int Reference { get; private set; }

        public PaletteReferencePaletteEntry()
            : this(0)
        {
        }

        public PaletteReferencePaletteEntry(int rf)
            : base(PaletteEntryType.PaletteReference)
        {
            Reference = rf;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.PaletteReferencePaletteEntry; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteInteger(Reference);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
                Reference = str.ReadInteger();
        }

        #endregion
    }
}
