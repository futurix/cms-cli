using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class SingleSlotBlockDefinition : CommonAtomicBlockDefinition, ICacheable
    {
        public string SlotHint { get; private set; }

        private Dictionary<BlockState, SingleSlotBlockStateData> stateData = new Dictionary<BlockState, SingleSlotBlockStateData>();

        public SingleSlotBlockDefinition()
            : base()
        {
        }

        public SingleSlotBlockStateData this[BlockState state]
        {
            get
            {
                SingleSlotBlockStateData res = null;
                stateData.TryGetValue(state, out res);

                return res;
            }
        }

        public void Unpack(FieldList source)
        {
            if (source == null)
                return;

            UnpackDefinitionID(source);

            IsCheckable = false;

            // unpacking common attributes
            AcceptsFocus = source[DefAgentFieldID.AcceptsFocus].AsBoolean() ?? false;
            SortIndex = source[DefAgentFieldID.SortSlotIndex].AsShort() ?? -1;
            SizeToBackgroundImage = source[DefAgentFieldID.BlockSizeToBackground].AsBoolean() ?? false;

            // loop through states
            PairedList<ByteField, FieldList> states =
                source.GetPairedItems<ByteField, FieldList>(DefAgentFieldID.ComponentState, DefAgentFieldID.DataPerComponentState);

            foreach (Pair<ByteField, FieldList> item in states)
            {
                BlockState state = (BlockState)item.First.Data;
                FieldList newData = item.Second;

                SingleSlotBlockStateData data = GetOrCreateDataForState(state);

                // slot style data
                data.SlotStyle = new SlotStyleData();
                data.SlotStyle.Unpack(newData);

                // slot index
                data.SlotIndex = newData[DefAgentFieldID.SlotIndex].AsShort() ?? 0;

                // mark as checkable if applicable
                if ((state == BlockState.CheckedNormal) || (state == BlockState.CheckedFocused))
                    IsCheckable = true;

                // margins and paddings
                data.MarginLeft = newData[DefAgentFieldID.LeftMargin2].AsShort() ?? 0;
                data.MarginTop = newData[DefAgentFieldID.TopMargin2].AsShort() ?? 0;
                data.MarginRight = newData[DefAgentFieldID.RightMargin].AsShort() ?? 0;
                data.MarginBottom = newData[DefAgentFieldID.BottomMargin].AsShort() ?? 0;
                data.PaddingLeft = newData[DefAgentFieldID.LeftPadding2].AsShort() ?? 0;
                data.PaddingTop = newData[DefAgentFieldID.TopPadding2].AsShort() ?? 0;
                data.PaddingRight = newData[DefAgentFieldID.RightPadding].AsShort() ?? 0;
                data.PaddingBottom = newData[DefAgentFieldID.BottomPadding].AsShort() ?? 0;

                data.MaximumChars = newData[DefAgentFieldID.MaximumChars].AsShort() ?? 0;
                data.MaximumLines = newData[DefAgentFieldID.MaximumLines].AsShort() ?? 0;
                data.MinimumLines = newData[DefAgentFieldID.MinimumLines].AsShort() ?? 0;

                // rendering hints
                FieldList firstSlotData = (FieldList)newData[DefAgentFieldID.SlotData];

                if (firstSlotData != null)
                {
                    string newSlotHint = firstSlotData[DefAgentFieldID.SlotHint].AsString();

                    if (newSlotHint != null)
                        SlotHint = newSlotHint;
                }

                stateData.Add(state, data);
            }

            // rendering hints for the block
            UnpackBlockHints(source);

            // done
            IsUnpacked = true;
        }

        public bool HasState(BlockState state)
        {
            return stateData.ContainsKey(state);
        }

        private SingleSlotBlockStateData GetOrCreateDataForState(BlockState state)
        {
            SingleSlotBlockStateData res = null;

            stateData.TryGetValue(state, out res);

            if (res == null)
                res = new SingleSlotBlockStateData();

            return res;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.SingleSlotBlockDefinition; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteBool(AcceptsFocus);
            str.WriteInteger(SortIndex);
            str.WriteBool(IsCheckable);
            BinaryHelper.WriteString(str, SlotHint);
            str.WriteShort((short)stateData.Count);

            if (stateData.Count > 0)
            {
                foreach (var pair in stateData)
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
                AcceptsFocus = str.ReadBool();
                SortIndex = str.ReadInteger();
                IsCheckable = str.ReadBool();
                SlotHint = BinaryHelper.ReadString(str);

                short numberOfStates = str.ReadShort();

                if (numberOfStates > 0)
                {
                    for (int i = 0; i < numberOfStates; i++)
                    {
                        BlockState state = (BlockState)str.ReadShort();

                        SingleSlotBlockStateData ssbsd = new SingleSlotBlockStateData();
                        ssbsd.Restore(str);

                        stateData.Add(state, ssbsd);
                    }
                }
            }
        }

        #endregion
    }

    public class SingleSlotBlockStateData : ICacheable
    {
        public SlotStyleData SlotStyle { get; set; }

        public int SlotIndex { get; set; }
        
        public int MarginLeft { get; set; }
        public int MarginTop { get; set; }
        public int MarginRight { get; set; }
        public int MarginBottom { get; set; }

        public int PaddingLeft { get; set; }
        public int PaddingTop { get; set; }
        public int PaddingRight { get; set; }
        public int PaddingBottom { get; set; }

        public short MaximumChars { get; set; }
        public short MaximumLines { get; set; }
        public short MinimumLines { get; set; }

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);

            SlotStyle.Persist(str);

            str.WriteInteger(SlotIndex);

            str.WriteInteger(MarginLeft);
            str.WriteInteger(MarginTop);
            str.WriteInteger(MarginRight);
            str.WriteInteger(MarginBottom);

            str.WriteInteger(PaddingLeft);
            str.WriteInteger(PaddingTop);
            str.WriteInteger(PaddingRight);
            str.WriteInteger(PaddingBottom);

            str.WriteShort(MaximumChars);
            str.WriteShort(MaximumLines);
            str.WriteShort(MinimumLines);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                SlotStyle = new SlotStyleData();
                SlotStyle.Restore(str);

                SlotIndex = str.ReadInteger();

                MarginLeft = str.ReadInteger();
                MarginTop = str.ReadInteger();
                MarginRight = str.ReadInteger();
                MarginBottom = str.ReadInteger();

                PaddingLeft = str.ReadInteger();
                PaddingTop = str.ReadInteger();
                PaddingRight = str.ReadInteger();
                PaddingBottom = str.ReadInteger();

                MaximumChars = str.ReadShort();
                MaximumLines = str.ReadShort();
                MinimumLines = str.ReadShort();
            }
        }

        #endregion
    }
}
