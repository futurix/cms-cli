using System;
using System.IO;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public static class CacheHelper
    {
        public static byte[] Pack(object data)
        {
            if (data is byte[])
            {
                using (MemoryStream mem = new MemoryStream())
                {
                    mem.WriteByte(0); // version
                    mem.WriteShort((short)CacheableType.Binary); // type
                    mem.WriteInteger(((byte[])data).Length); // data length
                    mem.WriteBytes((byte[])data); // data

                    return mem.ToArray();
                }
            }
            else if (data is ICacheable)
            {
                ICacheable source = data as ICacheable;

                if ((source != null) && (source.StoredType != CacheableType.Unsupported))
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        mem.WriteByte(0); // version
                        mem.WriteShort((short)source.StoredType); // type
                        source.Persist(mem); // data

                        return mem.ToArray();
                    }
                }
            }

            if (data != null)
                DebugHelper.Out("Attempt to serialize unsupported type of object: {0}", data.GetType().FullName);

            return null;
        }

        public static object Unpack(byte[] rawData)
        {
            if ((rawData == null) || (rawData.Length == 0))
                return null;

            using (MemoryStream mem = new MemoryStream(rawData))
            {
                if (mem.ReadByte() == 0)
                {
                    CacheableType ct = (CacheableType)mem.ReadShort();

                    if (ct != CacheableType.Unsupported)
                    {
                        if (ct == CacheableType.Binary)
                        {
                            int binLength = mem.ReadInteger();

                            if (binLength > 0)
                                return mem.ReadBytes(binLength);
                        }
                        else
                        {
                            Type type = ResolveStoredType(ct);

                            if (type != null)
                            {
                                object target = null;

                                try
                                {
                                    target = Activator.CreateInstance(type);
                                }
                                catch
                                {
                                    target = null;
                                }

                                if ((target != null) && (target is ICacheable))
                                {
                                    ((ICacheable)target).Restore(mem);

                                    return target;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static void PackToStream(Stream str, object data)
        {
            if (str != null)
            {
                byte[] packaged = Pack(data);

                if (packaged != null)
                {
                    str.WriteByte(1);
                    str.WriteInteger(packaged.Length);

                    if (packaged.Length > 0)
                        str.WriteBytes(packaged);
                }
                else
                    str.WriteByte(0);
            }
        }

        public static object UnpackFromStream(Stream str)
        {
            if ((str != null) && (str.ReadByte() == 1))
            {
                int dataSize = str.ReadInteger();
                byte[] dataBytes = null;

                if (dataSize > 0)
                    dataBytes = str.ReadBytes(dataSize);

                return Unpack(dataBytes);
            }

            return null;
        }

        private static Type ResolveStoredType(CacheableType input)
        {
            switch (input)
            {
                case CacheableType.FieldList:
                    return typeof(FieldList);

                case CacheableType.AtomicBlockDefinition:
                    return typeof(AtomicBlockDefinition);

                case CacheableType.BoxLayoutBlockDefinition:
                    return typeof(BoxLayoutBlockDefinition);

                case CacheableType.GridBlockDefinition:
                    return typeof(GridBlockDefinition);

                case CacheableType.ListBlockDefinition:
                    return typeof(ListBlockDefinition);

                case CacheableType.SingleSlotBlockDefinition:
                    return typeof(SingleSlotBlockDefinition);

                case CacheableType.FrameDefinition:
                    return typeof(FrameDefinition);

                case CacheableType.ScrollingTextBlockDefinition:
                    return typeof(ScrollingTextBlockDefinition);

                case CacheableType.MapPluginBlockDefinition:
                    return typeof(MapPluginBlockDefinition);

                case CacheableType.PaletteDefinition:
                    return typeof(PaletteDefinition);

                case CacheableType.FontDefinition:
                    return typeof(FontDefinition);

                case CacheableType.ApplicationEvents:
                    return typeof(ApplicationEventsDefinition);

                case CacheableType.TableLayoutTemplate:
                    return typeof(TableLayoutTemplate);

                case CacheableType.InheritedPaletteEntry:
                    return typeof(InheritedPaletteEntry);

                case CacheableType.DoNotPaintPaletteEntry:
                    return typeof(DoNotPaintPaletteEntry);

                case CacheableType.FontReferencePaletteEntry:
                    return typeof(FontReferencePaletteEntry);

                case CacheableType.ColourPaletteEntry:
                    return typeof(ColourPaletteEntry);

                case CacheableType.LinearGradientPaletteEntry:
                    return typeof(LinearGradientPaletteEntry);

                case CacheableType.ColourSequencePaletteEntry:
                    return typeof(ColourSequencePaletteEntry);

                case CacheableType.PaletteReferencePaletteEntry:
                    return typeof(PaletteReferencePaletteEntry);

                case CacheableType.StyleSet:
                    return typeof(StyleSet);
            }

            return null;
        }
    }
}
