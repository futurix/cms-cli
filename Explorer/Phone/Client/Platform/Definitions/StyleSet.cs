using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class StyleSet : PaletteEntryBase, ICacheable
    {
        public bool IsUnpacked { get; private set; }

        public PaletteEntryType SecondaryEntryType { get; private set; }

        public Dictionary<BlockState, SlotStyleData> Data = new Dictionary<BlockState, SlotStyleData>();

        public StyleSet()
            : this(PaletteEntryType.StyleSet)
        {
        }

        public StyleSet(PaletteEntryType sd)
            : base(PaletteEntryType.StyleSet)
        {
            // if this is not a normal styleset, but instead a "system dialog" one
            // let's remember save that extra bit of data
            SecondaryEntryType = sd;
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            PairedList<ByteField, FieldList> searchResults =
                source.GetPairedItems<ByteField, FieldList>(DefAgentFieldID.ComponentState, DefAgentFieldID.DataPerComponentState);

            foreach (Pair<ByteField, FieldList> result in searchResults)
            {
                BlockState state = (BlockState)result.First.Data;
                
                if (!Data.ContainsKey(state))
                {
                    SlotStyleData newData = new SlotStyleData();
                    newData.Unpack(result.Second);

                    Data[state] = newData;
                }
            }

            // done
            IsUnpacked = true;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.StyleSet; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteBool(IsUnpacked);
            str.WriteShort((short)SecondaryEntryType);
            str.WriteInteger(Data.Count);

            if (Data.Count > 0)
            {
                foreach (var pair in Data)
                {
                    str.WriteShort((short)pair.Key);
                    pair.Value.Persist(str);
                }
            }
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                IsUnpacked = str.ReadBool();
                SecondaryEntryType = (PaletteEntryType)str.ReadShort();

                int numberOfItems = str.ReadInteger();

                if (numberOfItems > 0)
                {
                    for (int i = 0; i < numberOfItems; i++)
                    {
                        BlockState state = (BlockState)str.ReadShort();

                        SlotStyleData ssd = new SlotStyleData();
                        ssd.Restore(str);

                        Data.Add(state, ssd);
                    }
                }
            }
        }

        #endregion
    }
}
