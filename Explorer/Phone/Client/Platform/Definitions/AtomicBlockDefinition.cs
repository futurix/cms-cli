using System;
using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class AtomicBlockDefinition : CommonAtomicBlockDefinition, ICacheable
    {
        public int SpecialTag { get; private set; }
        public bool DeviceTextScaling { get; private set; }

        public DisplayDataCollection StaticDisplayData = null;
        public string[] SlotHints = null;

        private Dictionary<BlockState, AtomicBlockStateData> stateData = new Dictionary<BlockState, AtomicBlockStateData>();

        public AtomicBlockDefinition()
            : base()
        {
        }

        public AtomicBlockStateData this[BlockState state]
        {
            get
            {
                AtomicBlockStateData res = null;
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
            DeviceTextScaling = false;
            
            // unpacking common attributes
            SpecialTag = source[DefAgentFieldID.DefinitionSpecialTag].AsInteger() ?? -1;
            BackgroundImageCrop = (CropStrategy)(source[DefAgentFieldID.BackgroundCropStrategy].AsNumber() ?? 0);
            SizeToBackgroundImage = source[DefAgentFieldID.BlockSizeToBackground].AsBoolean() ?? false;
            AcceptsFocus = source[DefAgentFieldID.AcceptsFocus].AsBoolean() ?? false;
            SortIndex = source[DefAgentFieldID.SortSlotIndex].AsShort() ?? -1;
            
            // static display data
            int numberOfDisplayData = source.GetItemCount(MessageOutFieldID.SlotDisplayDataTypeID);
            
            if (numberOfDisplayData > 0)
                StaticDisplayData = DisplayData.Parse(source);

            // loop through states
            PairedList<ByteField, FieldList> states =
                source.GetPairedItems<ByteField, FieldList>(DefAgentFieldID.ComponentState, DefAgentFieldID.DataPerComponentState);

            foreach (Pair<ByteField, FieldList> item in states)
            {
                BlockState state = (BlockState)item.First.Data;
                FieldList newData = item.Second;

                AtomicBlockStateData data = GetOrCreateDataForState(state);

                // mark as checkable if applicable
                if ((state == BlockState.CheckedNormal) || (state == BlockState.CheckedFocused))
                    IsCheckable = true;

                // set paint styles
                data.ComponentForeground = new PaintStyle(newData, DefAgentFieldID.ForegroundPaintStyle);
                data.ComponentBackground = new PaintStyle(newData, DefAgentFieldID.BackgroundPaintStyle);
                
                // font reference
                data.ComponentFont = newData[DefAgentFieldID.FontReference].AsShort() ?? 0;

                // margins and paddings
                data.MarginLeft = newData[DefAgentFieldID.LeftMargin2].AsShort() ?? 0;
                data.MarginTop = newData[DefAgentFieldID.TopMargin2].AsShort() ?? 0;
                data.MarginRight = newData[DefAgentFieldID.RightMargin].AsShort() ?? 0;
                data.MarginBottom = newData[DefAgentFieldID.BottomMargin].AsShort() ?? 0;
                data.PaddingLeft = newData[DefAgentFieldID.LeftPadding2].AsShort() ?? 0;
                data.PaddingTop = newData[DefAgentFieldID.TopPadding2].AsShort() ?? 0;
                data.PaddingRight = newData[DefAgentFieldID.RightPadding].AsShort() ?? 0;
                data.PaddingBottom = newData[DefAgentFieldID.BottomPadding].AsShort() ?? 0;

                // slot data
                List<FieldList> slotsData = newData.GetItems<FieldList>(DefAgentFieldID.SlotData);

                foreach (FieldList sd in slotsData)
                {
                    SlotData info = new SlotData();

                    info.Foreground = new PaintStyle(sd, DefAgentFieldID.ForegroundPaintStyle);
                    info.Background = new PaintStyle(sd, DefAgentFieldID.BackgroundPaintStyle);
                    info.Font = sd[DefAgentFieldID.FontReference].AsShort() ?? 0;
                    info.SlotIndex = sd[DefAgentFieldID.SlotIndex].AsShort() ?? 0;

                    // rendering hints
                    if ((SlotHints == null) || (SlotHints.Length <= info.SlotIndex) || (SlotHints[(int)info.SlotIndex] == null))
                    {
                        string newSlotHint = sd[DefAgentFieldID.SlotHint].AsString();

                        if (newSlotHint != null)
                        {
                            if (SlotHints == null)
                                SlotHints = new string[Math.Max(slotsData.Count, (int)(info.SlotIndex + 1))];
                            else if (SlotHints.Length <= info.SlotIndex)
                                Array.Resize<string>(ref SlotHints, Math.Max(slotsData.Count, (int)(info.SlotIndex + 1)));

                            SlotHints[(int)info.SlotIndex] = newSlotHint;
                        }
                    }

                    data.SlotInfo.Add(info);
                }

                // unpack layout
                LayoutType layoutType = (LayoutType)(newData[DefAgentFieldID.LayoutType].AsByte() ?? 0);

                if (layoutType == LayoutType.Table)
                {
                    // currently only table layout is supported for atomic blocks
                    data.LayoutTemplate = new TableLayoutTemplate(newData);
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

        private AtomicBlockStateData GetOrCreateDataForState(BlockState state)
        {
            AtomicBlockStateData res = null;

            stateData.TryGetValue(state, out res);

            if (res == null)
                res = new AtomicBlockStateData();

            return res;
        }

        #region ICacheable implementation

        public override CacheableType StoredType
        {
            get { return CacheableType.AtomicBlockDefinition; }
        }

        public override void Persist(Stream str)
        {
            base.Persist(str);

            str.WriteByte(0);
            str.WriteInteger(SpecialTag);
            str.WriteBool(AcceptsFocus);
            str.WriteInteger(SortIndex);
            str.WriteBool(IsCheckable);
            str.WriteBool(DeviceTextScaling);

            // display data
            if (StaticDisplayData != null)
            {
                str.WriteByte(1);
                StaticDisplayData.Persist(str);
            }
            else
                str.WriteByte(0);

            // slot hints
            if ((SlotHints != null) && (SlotHints.Length > 0))
            {
                str.WriteInteger(SlotHints.Length);

                foreach (string slotHint in SlotHints)
                    BinaryHelper.WriteString(str, slotHint);
            }
            else
                str.WriteInteger(0);

            // state data
            str.WriteShort((short)stateData.Count);

            if (stateData.Count > 0)
            {
                foreach (var state in stateData)
                {
                    str.WriteShort((short)state.Key);
                    state.Value.Persist(str);
                }
            }
        }

        public override void Restore(Stream str)
        {
            base.Restore(str);

            if (str.ReadByte() == 0)
            {
                SpecialTag = str.ReadInteger();
                AcceptsFocus = str.ReadBool();
                SortIndex = str.ReadInteger();
                IsCheckable = str.ReadBool();
                DeviceTextScaling = str.ReadBool();

                // static display data
                if (str.ReadByte() == 1)
                {
                    StaticDisplayData = new DisplayDataCollection();
                    StaticDisplayData.Restore(str);
                }
                else
                    StaticDisplayData = null;

                // slot hints
                int slotHintsCount = str.ReadInteger();

                if (slotHintsCount > 0)
                {
                    SlotHints = new string[slotHintsCount];

                    for (int i = 0; i < slotHintsCount; i++)
                        SlotHints[i] = BinaryHelper.ReadString(str);
                }
                else
                    SlotHints = null;

                // state data
                short statesCount = str.ReadShort();

                for (int i = 0; i < statesCount; i++)
                {
                    BlockState st = (BlockState)str.ReadShort();

                    AtomicBlockStateData stData = new AtomicBlockStateData();
                    stData.Restore(str);

                    stateData[st] = stData;
                }
            }
        }

        #endregion
    }

    public class AtomicBlockStateData : ICacheable
    {
        public short? ComponentFont { get; set; }
        public PaintStyle? ComponentForeground { get; set; }
        public PaintStyle? ComponentBackground { get; set; }

        public int MarginLeft { get; set; }
        public int MarginTop { get; set; }
        public int MarginRight { get; set; }
        public int MarginBottom { get; set; }

        public int PaddingLeft { get; set; }
        public int PaddingTop { get; set; }
        public int PaddingRight { get; set; }
        public int PaddingBottom { get; set; }

        public List<SlotData> SlotInfo = new List<SlotData>();

        public LayoutTemplateBase LayoutTemplate = null;

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);

            if (ComponentFont.HasValue)
            {
                str.WriteByte(1);
                str.WriteShort(ComponentFont.Value);
            }
            else
                str.WriteByte(0);

            if (ComponentForeground.HasValue)
            {
                str.WriteByte(1);
                ComponentForeground.Value.Persist(str);
            }
            else
                str.WriteByte(0);

            if (ComponentBackground.HasValue)
            {
                str.WriteByte(1);
                ComponentBackground.Value.Persist(str);
            }
            else
                str.WriteByte(0);

            str.WriteInteger(MarginLeft);
            str.WriteInteger(MarginTop);
            str.WriteInteger(MarginRight);
            str.WriteInteger(MarginBottom);

            str.WriteInteger(PaddingLeft);
            str.WriteInteger(PaddingTop);
            str.WriteInteger(PaddingRight);
            str.WriteInteger(PaddingBottom);

            str.WriteInteger(SlotInfo.Count);

            if (SlotInfo.Count > 0)
                foreach (SlotData slot in SlotInfo)
                    slot.Persist(str);

            CacheHelper.PackToStream(str, LayoutTemplate);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                if (str.ReadByte() == 1)
                    ComponentFont = str.ReadShort();
                else
                    ComponentFont = null;

                if (str.ReadByte() == 1)
                {
                    PaintStyle fg = new PaintStyle();
                    fg.Restore(str);

                    ComponentForeground = fg;
                }
                else
                    ComponentForeground = null;

                if (str.ReadByte() == 1)
                {
                    PaintStyle bg = new PaintStyle();
                    bg.Restore(str);

                    ComponentBackground = bg;
                }
                else
                    ComponentBackground = null;

                MarginLeft = str.ReadInteger();
                MarginTop = str.ReadInteger();
                MarginRight = str.ReadInteger();
                MarginBottom = str.ReadInteger();

                PaddingLeft = str.ReadInteger();
                PaddingTop = str.ReadInteger();
                PaddingRight = str.ReadInteger();
                PaddingBottom = str.ReadInteger();

                int slotInfoCount = str.ReadInteger();

                if (slotInfoCount > 0)
                {
                    for (int i = 0; i < slotInfoCount; i++)
                    {
                        SlotData slot = new SlotData();
                        slot.Restore(str);

                        SlotInfo.Add(slot);
                    }
                }

                LayoutTemplate = CacheHelper.UnpackFromStream(str) as LayoutTemplateBase;
            }
        }

        #endregion
    }

    public class SlotData : ICacheable
    {
        public short? Font { get; set; }
        public PaintStyle? Foreground { get; set; }
        public PaintStyle? Background { get; set; }

        public short? SlotIndex { get; set; }

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);

            if (Font.HasValue)
            {
                str.WriteByte(1);
                str.WriteShort(Font.Value);
            }
            else
                str.WriteByte(0);

            if (Foreground.HasValue)
            {
                str.WriteByte(1);
                Foreground.Value.Persist(str);
            }
            else
                str.WriteByte(0);

            if (Background.HasValue)
            {
                str.WriteByte(1);
                Background.Value.Persist(str);
            }
            else
                str.WriteByte(0);

            if (SlotIndex.HasValue)
            {
                str.WriteByte(1);
                str.WriteShort(SlotIndex.Value);
            }
            else
                str.WriteByte(0);
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                if (str.ReadByte() == 1)
                    Font = str.ReadShort();
                else
                    Font = null;

                if (str.ReadByte() == 1)
                {
                    PaintStyle fg = new PaintStyle();
                    fg.Restore(str);

                    Foreground = fg;
                }
                else
                    Foreground = null;

                if (str.ReadByte() == 1)
                {
                    PaintStyle fg = new PaintStyle();
                    fg.Restore(str);

                    Background = fg;
                }
                else
                    Background = null;

                if (str.ReadByte() == 1)
                    SlotIndex = str.ReadShort();
                else
                    SlotIndex = null;
            }
        }

        #endregion
    }

    public enum BlockState : short
    {
        // internal use only as value from server is byte (so cannot be less than zero)
        NotLaidOut = -1,

		Normal = 0,
		Focused = 1,
		CheckedNormal = 2,
		CheckedFocused = 3,
		Disabled = 4,
		Updated = 5,
		FocusedDisabled = 6,

		MaximumNumberOfStates = 7
    }
}
