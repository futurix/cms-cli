namespace Wave.Platform.Messaging
{
    public enum DefAgentMessageID : short
    {
        DefinitionResponse = 2,
        BulkDefinitionResponse = 4
    }

    public enum DefAgentFieldID : short
    {
        ComponentState = 1, // byte
        DataPerComponentState = 2, // field list

        DefinitionType = 3, // byte
        DefinitionSpecialTag = 4, // int32
        DefinitionMessage = 5, // binary

        SlotData = 10, // field list

        ForegroundPaintStyle = 11, // byte
        BackgroundPaintStyle = 12, // byte
        PaintStyleData = 13, // int32 (incorrectly documented as int16 for palette ref)
        FontReference = 14, // int16
        SlotIndex = 15, // int16 - index into the combined display data array

        // palette attributes useful to have pallete entiry to another palette entry
        PaletteEntryType = 20, // byte - full enum of all the Palette entry type including fonts and paint styles
        PaletteEntryData = 21, // any
        PaletteEntryIndex = 22, // int16
        GradientKeyPosition = 23, // byte
        GradientKeyColour = 24, // int32
        PaletteSize = 25, // int16
        TimePeriod = 26, // int16
        Loop = 27, // boolean

        FontData = 30, // binary

        ApplicationEventActionSet = 35, // field list

        LayoutType = 40, // byte
        PluginType = 41, // byte

        LayoutItem = 50, // field list
        StretchWeight = 51, // int16 - sent once per a row
        RelativeSize = 52, // int16 - sent once per a column
        Spacing = 53, // int16
        FlowDirection = 54, // byte
        NumberOfColumns = 55, // byte
        NumberOfRows = 56, // byte

        XPosition = 57, // int16
        YPosition = 58, // int16
        Width = 59, // int16
        Height = 60, // int16

        LeftMargin2 = 61, // int16 - for use in all layouts apart from Flow, Grid and EPG (was XMargin)
        TopMargin2 = 62, // int16 - for use in all layouts apart from Flow, Grid and EPG (was YMargin)
        LeftPadding2 = 63, // int16 - for use in all layouts apart from Flow, Grid and EPG (was XPadding)
        TopPadding2 = 64, // int16 - for use in all layouts apart from Flow, Grid and EPG (was YPadding)

        CropStrategy = 65, // int32 (CSL4+), int16 (CSL3)

        LeftPadding = 66, // int16
        RightPadding = 67, // int16
        TopPadding = 68, // int16
        BottomPadding = 69, // int16

        VisibleChildren = 70, // int16 - the number of child items of a container to make visible
        AllowAutoscroll = 71, // boolean - only sent if true
        EnforceRadio = 72, // boolean - only sent if true
        FocusBehaviour = 73, // byte - optional, default to normal
        FocusPosition = 74, // byte - optional, default to free

        TickerSpeed = 75, // byte - optional, ticker speed
        PauseBetweenItem = 76, // byte - optional pause
        TickerDirection = 77, // byte - optional ticker direction
        UsePagination = 78, // byte - optional, default to false

        FirstFocus = 80, // byte
        ScrollIndicationType = 81, // byte
        AcceptsFocus = 82, // boolean - default to false (do not send if false)
        HorizontalScrollIndicationType = 83, // byte
        VerticalScrollIndicationType = 84, // byte
        ClickToScroll = 85, // boolean
        ForceContentToWidth = 86, // boolean
        SortSlotIndex = 87, // int16 - optional integer value used to sort atomics in a flow container
        FloatBehaviour = 88, // boolean - optional for lists, make the contents float above the parent's other children
        SpecialBehaviour = 89, // boolean - optional for lists, turn on Cover Flow-style functionality

        MinimumColumnWidth = 90, // byte
        MaximumColumnWidth = 91, // byte
        ColumnsHeadersFocusable = 92, // boolean - default to false (do not send if false)
        RowHeadersFocusable = 93, // boolean - default to false (do not send if false)
        KeepColumnsHeadersVisible = 94, // boolean - default to false (do not send if false)
        KeepRowHeadersVisible = 95, // boolean - default to false (do not send if false)
        SelectionMode = 96, // byte
        DefaultSlotDefaultPaletteEntry = 97, // int16
        AuxiliarySlotDefaultPaletteEntry = 98, // int16

        LeftMargin = 100, // int16
        RightMargin = 101, // int16
        TopMargin = 102, // int16
        BottomMargin = 103, // int16

        // text restrictions
        MaximumChars = 120, // int16 - the maximum number of characters that are allowed
        MaximumLines = 121, // int16 - the maximum number of lines that are allowed
        MinimumLines = 122, // int16 - the minimum number of lines that are allowed

        // EPG specific
        ChannelsCanFocus = 110, // boolean - default to false (do not send if false)
        ShowDateTitle = 111, // boolean - default to false (do not show the date title)
        ClockDisplay = 112, // byte - enum of the clock display
        ChannelSlotStyles = 150, // field list
        ProgrammeSlotStyles = 151, // field list

        // block proxy
        MaximumChildren = 130, // int16 - the maximum number of blocks a block proxy can display
        FromBottom = 131, // boolean - indicates if block is added from the top or bottom

        // hints
        Hint = 140, // string
        SlotHint = 141, // string

        StripeStyles = 152, // field list - contains the palette references for a stripe set

        // map Fields
        MapLatitude = 140, // int32 - latitude of the center of the map in microdegrees
        MapLongitude = 141, // int32 - longitude of the center of the map in microdegrees
        MapSpanLatitudeUDegrees = 142, // int32 - desired span of latitude in microdegrees
        MapSpanLongitudeUDegrees = 143, // int32 - desired span of longitude in microdegrees
        Pin = 144, // field list - a fieldlist defining a map pin
        PinLatitude = 145, // int32 - latitude of a map pin in microdegrees
        PinLongitude = 146, // int32 - longitude of a map pin in microdegrees
        PinTitle = 147, // string - title of a map pin
        PinSubtitle = 148, // string - subtitle of a map pin
        PinState = 149, // int16 - state of a map pin (enumeration)
        PinID = 150, // string - ID of the pin

        // map plugin fields
        MapZoomEnabled = 160, // boolean - determines whether the plugin should allow zooming
        MapScrollEnabled = 161, // boolean - determines whether the plugin should allow scrolling
        MapAnimationsEnabled = 162, // boolean - determines whether the plugin should enable animations
        MapShowUserLocEnabled = 163, // boolean - determines whether the plugin should show the user's location
        MapMode = 164, // int16 - determines the display mode for the map (enumeration)

        // background image support
        BackgroundCropStrategy = 170, // int16 - background crop strategy
        BackgroundScrollable = 171, // boolean - default to false (do not scroll background)
        BlockSizeToBackground = 172 // boolean - default to false (do not size block to background)
    }

    public enum LayoutType : short
    {
        Table = 0,
        Box = 1,
        Flow = 2,
        HotKey = 3,
        ScrollingText = 4,
        Video = 5,
        Dropdown = 6,
        Grid = 7,
        SingleSlot = 8,
        ScrollingImage = 9
    }

    public enum PluginType : short
    {
        EPG = 0,
        MediaList = 1,
        Map = 2
    }

    public enum UIFlowDirection : short
    {
        Up = 0,
        Down = 1,
        Right = 2,
        Left = 3,

        MinorAxisPositive = 4,
        MinorAxisNegative = 5,
        MajorAxisPositive = 6,
        MajorAxisNegative = 7
    }

    public enum ScrollIndication : short
    {
        None = 0,
        Bar = 1,
        Arrows = 2,
        Dots = 3
    }

    public enum DefinitionType : short
    {
        AtomicBlock = 0,
        Container = 1,
        Plugin = 3,
        Palette = 4,
        Font = 5,
        ApplicationEvents = 6,
        Frame = 7
    }

    public enum GridSelectionMode : byte
    {
        Cell = 0,
        Row = 1,
        Column = 2,
        RowAndColumn = 3
    }

    public enum FocusBehaviour : byte
    {
        Normal = 0,
        Wrap = 1,
        Loop = 2
    }

    public enum FocusPosition : byte
    {
        Free = 0,
        Center = 1
    }

    public enum TickerFlowDirection : byte
    {
        Backwards = 1,
        Forwards = 2
    }

    public enum TickerSpeed : byte
    {
        Slow = 1,
        Medium = 2,
        Fast = 3
    }

    public enum TickerItemPause
    {
        None = 0,
        Short = 1,
        Medium = 2,
        Long = 3
    }

    public enum StateFlag
    {
        Focused = 0,
        Checked = 1
    }
}
