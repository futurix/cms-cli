using System.IO;

namespace Wave.Services
{
    public interface ICacheable
    {
        CacheableType StoredType { get; }
        
        void Persist(Stream str);
        void Restore(Stream str);
    }

    public enum CacheableType : short
    {
        Unsupported = 0,

        // basic classes and non-classes
        Binary = 1,
        FieldList = 2,

        // block definitions
        AtomicBlockDefinition = 100,
        BoxLayoutBlockDefinition = 110,
        GridBlockDefinition = 120,
        ListBlockDefinition = 130,
        SingleSlotBlockDefinition = 140,
        FrameDefinition = 150,

        // definitions for rare blocks or plug-ins
        ScrollingTextBlockDefinition = 340,
        MapPluginBlockDefinition = 400,

        // other definitions
        PaletteDefinition = 500,
        FontDefinition = 550,
        ApplicationEvents = 600,

        // layouts
        TableLayoutTemplate = 700,

        // palette entries
        InheritedPaletteEntry = 800,
        DoNotPaintPaletteEntry = 801,
        FontReferencePaletteEntry = 802,
        ColourPaletteEntry = 803,
        LinearGradientPaletteEntry = 804,
        ColourSequencePaletteEntry = 805,
        PaletteReferencePaletteEntry = 806,
        StyleSet = 807
    }
}
