using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class PaletteDefinition : DefinitionBase, ICacheable
    {
        public int Count
        {
            get { return (entries != null) ? entries.Length : 0; }
        }

        private PaletteEntryBase[] entries = null;
        
        public PaletteDefinition()
            : base()
        {
        }

        public PaletteEntryBase this[int index]
        {
            get
            {
                if ((entries != null) && (index >= 0) && (index < entries.Length))
                    return entries[index];
                
                return null;
            }
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            UnpackDefinitionID(source);

            if (source.GetItemCount(DefAgentFieldID.PaletteEntryIndex) > 0)
            {
                int paletteSize = source[DefAgentFieldID.PaletteSize].AsShort() ?? 0;

                if (paletteSize > 0)
                {
                    // create internal array
                    entries = new PaletteEntryBase[paletteSize];
                    
                    // add entries
                    IEnumerator<IFieldBase> enumerator = source.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.FieldID == (short)DefAgentFieldID.PaletteEntryIndex)
                        {
                            int entryIndex = enumerator.Current.AsShort() ?? -1;

                            if (entryIndex != -1)
                            {
                                enumerator.MoveNext();

                                if (enumerator.Current.FieldID == (short)DefAgentFieldID.PaletteEntryType)
                                {
                                    PaletteEntryType entryType = (PaletteEntryType)(enumerator.Current.AsByte() ?? 0);
                                    PaletteEntryBase newEntry = null;

                                    switch (entryType)
                                    {
                                        case PaletteEntryType.Undefined:
                                        default:
                                            break;

                                        case PaletteEntryType.Inherited:
                                            newEntry = new InheritedPaletteEntry();
                                            break;

                                        case PaletteEntryType.DoNotPaint:
                                            newEntry = new DoNotPaintPaletteEntry();
                                            break;

                                        case PaletteEntryType.FontReference:
                                            {
                                                enumerator.MoveNext();

                                                if (enumerator.Current.FieldID == (short)DefAgentFieldID.PaletteEntryData)
                                                {
                                                    int fontDefID = enumerator.Current.AsInteger() ?? -1;

                                                    if (fontDefID != -1)
                                                        newEntry = new FontReferencePaletteEntry(fontDefID);
                                                }

                                                break;
                                            }

                                        case PaletteEntryType.Image:
                                            {
                                                break;
                                            }

                                        case PaletteEntryType.Colour:
                                            {
                                                enumerator.MoveNext();

                                                if (enumerator.Current.FieldID == (short)DefAgentFieldID.PaletteEntryData)
                                                {
                                                    int? argb = enumerator.Current.AsInteger();

                                                    if (argb.HasValue)
                                                        newEntry = new ColourPaletteEntry(argb.Value);
                                                }

                                                break;
                                            }

                                        case PaletteEntryType.LinearGradient:
                                            {
                                                enumerator.MoveNext();

                                                if (enumerator.Current.FieldID == (short)DefAgentFieldID.PaletteEntryData)
                                                {
                                                    FieldList gradientData = enumerator.Current as FieldList;

                                                    if (gradientData != null)
                                                        newEntry = new LinearGradientPaletteEntry(gradientData);
                                                }

                                                break;
                                            }

                                        case PaletteEntryType.ColourSequence:
                                            {
                                                enumerator.MoveNext();

                                                if (enumerator.Current.FieldID == (short)DefAgentFieldID.PaletteEntryData)
                                                {
                                                    FieldList clsData = enumerator.Current as FieldList;

                                                    if (clsData != null)
                                                        newEntry = new ColourSequencePaletteEntry(clsData);
                                                }

                                                break;
                                            }

                                        case PaletteEntryType.PaletteReference:
                                            {
                                                enumerator.MoveNext();

                                                if (enumerator.Current.FieldID == (short)DefAgentFieldID.PaletteEntryData)
                                                {
                                                    int rf = enumerator.Current.AsInteger() ?? -1;

                                                    if (rf != -1)
                                                        newEntry = new PaletteReferencePaletteEntry(rf);
                                                }

                                                break;
                                            }

                                        case PaletteEntryType.StyleSet:
                                        case PaletteEntryType.SystemDialogRoot:
                                        case PaletteEntryType.SystemDialogContainer:
                                        case PaletteEntryType.SystemDialogTitle:
                                        case PaletteEntryType.SystemDialogStaticText:
                                        case PaletteEntryType.SystemDialogListEntry:
                                        case PaletteEntryType.SystemDialogMenuBar:
                                            {
                                                enumerator.MoveNext();

                                                if (enumerator.Current.FieldID == (short)DefAgentFieldID.PaletteEntryData)
                                                {
                                                    FieldList styleSetData = enumerator.Current as FieldList;

                                                    if (styleSetData != null)
                                                    {
                                                        StyleSet sst = new StyleSet(entryType);
                                                        sst.Unpack(styleSetData);

                                                        newEntry = sst;
                                                    }
                                                }

                                                break;
                                            }
                                    }

                                    if (newEntry != null)
                                        entries[entryIndex] = newEntry;
                                }
                            }
                        }
                    }
                }
            }

            // done
            IsUnpacked = true;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.PaletteDefinition; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);

            if ((entries != null) && (entries.Length > 0))
            {
                str.WriteInteger(entries.Length);

                foreach (PaletteEntryBase entry in entries)
                {
                    if (entry != null)
                    {
                        str.WriteByte(1);
                        CacheHelper.PackToStream(str, entry);
                    }
                    else
                        str.WriteByte(0);
                }
            }
            else
                str.WriteInteger(0);
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                int numberOfEntries = str.ReadInteger();

                if (numberOfEntries > 0)
                {
                    entries = new PaletteEntryBase[numberOfEntries];

                    for (int i = 0; i < numberOfEntries; i++)
                    {
                        if (str.ReadByte() == 1)
                            entries[i] = CacheHelper.UnpackFromStream(str) as PaletteEntryBase;
                        else
                            entries[i] = null;
                    }
                }
                else
                    entries = null;
            }
        }

        #endregion
    }

    public enum PaletteEntryType : short
    {
		Inherited = 0,
		DoNotPaint = 1,
		Colour = 2,
        PaletteReference = 3, // a reference to entry in another palette
		LinearGradient = 4,
		Image = 5,
		FontReference = 6,
		StyleSet = 7,
		Undefined = 8, // used for packing out sparse palettes
		ColourSequence = 9,
		
		// special system styleset types (basically they allow Explorer to refer to the app-wide system block rendering styles)
		SystemDialogRoot = 100,
		SystemDialogContainer = 101,
		SystemDialogTitle = 102,
		SystemDialogStaticText = 103,
		SystemDialogListEntry = 104,
		SystemDialogMenuBar = 105 // for use with softkeys, etc.
    }
}
