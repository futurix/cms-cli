using System;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public struct PaintStyle : ICacheable
    {
        public PaletteEntryType StyleType { get; private set; }
        public int? Data { get; private set; }
        
        public PaintStyle(PaletteEntryType st, int? data)
            : this()
        {
            StyleType = st;
            Data = data;
        }

        public PaintStyle(FieldList source, Enum sourceID)
            : this()
        {
            if ((source != null) && (sourceID != null))
            {
                PaletteEntryType pst = (PaletteEntryType)(source[sourceID].AsByte() ?? (byte)PaletteEntryType.Inherited);

                switch (pst)
                {
                    case PaletteEntryType.Inherited:
                    default:
                        StyleType = PaletteEntryType.Inherited;
                        break;
                    
                    case PaletteEntryType.DoNotPaint:
                        StyleType = PaletteEntryType.DoNotPaint;
                        break;

                    case PaletteEntryType.Colour:
                        {
                            StyleType = PaletteEntryType.Colour;

                            Int32Field field = source.GetNextItemAfter<Int32Field>(sourceID, DefAgentFieldID.PaintStyleData);

                            if (field != null)
                                Data = field.Data;
                            
                            break;
                        }

                    case PaletteEntryType.PaletteReference:
                        {
                            StyleType = PaletteEntryType.PaletteReference;

                            Int32Field field = source.GetNextItemAfter<Int32Field>(sourceID, DefAgentFieldID.PaintStyleData);

                            if (field != null)
                                Data = field.Data;

                            break;
                        }
                }
            }
        }

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);
            str.WriteShort((short)StyleType);
            str.WriteBool(Data.HasValue);

            if (Data.HasValue)
                str.WriteInteger(Data.Value);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                StyleType = (PaletteEntryType)str.ReadShort();

                if (str.ReadBool())
                    Data = str.ReadInteger();
                else
                    Data = null;
            }
        }

        #endregion
    }
}
