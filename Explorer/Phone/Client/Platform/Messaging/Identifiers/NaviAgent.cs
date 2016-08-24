namespace Wave.Platform.Messaging
{
    public enum NaviAgentMessageID : short
    {
        RequestApplication = 1,
        ApplicationResponse = 2,
        Action = 3,
        RequestNode = 4,
        GotoAnotherApplicationNode = 13,
        NodeResponse = 5,
        SubscribeToNodeWithoutRequest = 7,
        NodeIsInClientCache = 8,
        RequestNodeOnCacheError = 9,
        NodeResponseError = 10,
        FormResponse = 11,
        PushAction = 12,
        FetchNode = 14,
        FetchNodeResponse = 15,
        FetchNodeResponseError = 16,
        FetchNodeIsInClientCache = 17,
        ModifyNodeSubscription = 18
    }
    
    public enum NaviAgentFieldID : short
    {
        ApplicationURN = 2, // string
        ApplicationVersion = 3, // string
        ApplicationBuild = 4, // int16
        WaveVersion = 5, // string
        PaletteID = 6, // int32
        RequestedItemURI = 7, // string
        KeepBackLight = 8, // bool
        IsTopEvent = 9, // bool

        // application attributes parameters
        UseBackStack = 10, // bool
        IncludeHomeNode = 11, // bool
        
        // navigation attributes parameters
        DoNotAddNodeToBackStack = 15, // bool - field sent only if set to true
        DoNotPersistFocusState = 16, // bool - field sent only if set to true

        // block and presentation data
        BlockData = 21, // field list

        /// <summary>
        /// List containing names of signposts on the page (string).
        /// </summary>
        SignpostName = 24,
        Signpost = 25, // int16
        PaletteEntry = 26, // int16
        BackgroundImage = 27, // field list
        IsFocused = 22, // bool

        // actionsets
        
        ActionSet = 30, // field list
        ActionSetID = 31, // int32
		AnchorID = 32, // byte
        ActionTypeID = 33, // int32 (incorrectly documented as int16 in server headers)
        
        /// <summary>
        /// The action type defines what is in the action payload including potentially a field-list of data (any).
        /// </summary>
		ActionPayload = 34,
        
        /// <summary>
        /// Simply an enumeration that's only sent for multiple action action-sets so that the appropriate 
        /// payload can be referred to (byte).
        /// </summary>
		ActionRef = 35,
        
        ActionSetLabel = 36, // string
        
        /// <summary>
        /// The payload within the action is a signpost (int16).
        /// </summary>
		ActionPayloadFromSlot = 37,
        
        /// <summary>
        /// Pop up the page (bool).
        /// </summary>
		ActionPopUp = 38,
        
        ActionText = 39, // string
		
		// node level attributes

        CacheStrategy = 40, // int32

        /// <summary>
        /// Identifies another node which should be fetched in the background (field list).
        /// </summary>
		FetchItem = 41,

        /// <summary>
        /// Specifies whether the given node should be fetched even if the user navigates from the current node (bool).
        /// </summary>
		MandatoryFetch = 42,
		
		// other
		RequestError = 50, // byte
		
		// forms and transactions

        /// <summary>
        /// Enumerated from zero for every page (int32, incorrectly documented as int16).
        /// </summary>
		FormID = 60,
		
        /// <summary>
        /// Got via content and echoed back, only applicable to blocks that have the Form ID applied (string).
        /// </summary>
        SubmissionKey = 61,
		
        /// <summary>
        /// This will be an empty string where not applicable but must alway be sent with the Form ID (string).
        /// </summary>
        SubmissionValue = 62,
		
        SubmitFormResult = 63, // int16
		
        /// <summary>
        /// This will be the fully qualified HTTP URL passed to the CAS for it to submit the form to (string).
        /// </summary>
        FormRequestURL = 64,
		
        /// <summary>
        /// This block should start in its checked state - not sent if false (bool).
        /// </summary>
        IsChecked = 65,

        DeviceProfileGroup = 66, // int16
		
        /// <summary>
        /// Defaults to false if not sent, added as a payload to the EPGSubmitForm action (bool).
        /// </summary>
        FormRequiresNoWait = 67,
		
        /// <summary>
        /// The form transition (int16).
        /// </summary>
        FormTransition = 68,
		
        /// <summary>
        /// The location of the upload server (string).
        /// </summary>
        UploadServerURL = 69,

        // currently for grid only
        NumberOfRows = 70, // int16
        NumberOfColumns = 71, // int16
        NumberOfColumnHeaderRows = 72, // int16
        NumberOfRowHeaderColumns = 73, // int16
		
		ApplicationEntryName = 80,
		ApplicationServerAddress = 81,
		ApplicationPort = 82,

        DeviceScreenResolutionWidth = 83, // int16
        DeviceScreenResolutionHeight = 84, // int16
		DeviceSupportsTouch = 85, // bool
		
        /// <summary>
        /// App request mode, one of the enum values from AppRequestAction (int16).
        /// </summary>
		ApplicationRequestAction = 86,
		
        /// <summary>
        /// Used in combination with RequestActionGetPage to specify the desired page (string).
        /// </summary>
		ApplicationRequestPayload = 83,
		
        // EPG plugin definitions

        /// <summary>
        /// ActionSet pinned to the EPG Programme anchor-site (field list).
        /// </summary>
		EPGProgrammeActionSet = 90,
		
        /// <summary>
        /// ActionSet pinned to the EPG Channel anchor-site (field list).
        /// </summary>
        EPGChannelActionSet = 91,
		
        /// <summary>
        /// Slot field list used in action (field list).
        /// </summary>
		ActionSlot = 100,
		
        /// <summary>
        /// Action Page Transition (int16).
        /// </summary>
        ActionPageTransition = 101,
		
        /// <summary>
        /// Unique ID used in spot updates (string).
        /// </summary>
		ProxyValue = 110,
		
        ProxyPersistence = 111, // bool
		ProxyHistory = 112, // int16
		
		// PIM lookup

        /// <summary>
        /// A field list of Contact Data key/value pairs: { (ContactKey, ContactValue)* } (field list).
        /// </summary>
		ContactData = 120,
		
        /// <summary>
        /// A key to an item of Contact Data, see PIMItem enumeration (byte).
        /// </summary>
        ContactKey = 121,
		
        /// <summary>
        /// An item of Contact Data (string).
        /// </summary>
        ContactValue = 122,
		
        /// <summary>
        /// The requested URL (string).
        /// </summary>
		RequestURL = 130,
		
        /// <summary>
        /// Add the used ID to the HTTP request (bool).
        /// </summary>
        AddUserID = 131,
		
        /// <summary>
        /// The number of retries that a HTTP request should make (int16).
        /// </summary>
        NumRetries = 132,
		
        /// <summary>
        /// The time to wait in seconds between HTTP retries (int16).
        /// </summary>
        RetryInterval = 133,
		
        /// <summary>
        /// Valid only for CharacterCounter slots.
        /// Specifies whether the counter should count down (true) or count up (false).
        /// (bool)
        /// </summary>
		CountDown = 125,

        // frame content fields
        
        /// <summary>
        /// Frame acts as a delegate for the global back button (bool).
        /// </summary>
        BackAnchorDelegate = 198,
        
        /// <summary>
        /// Frame has a local backstack (bool).
        /// </summary>
        LocalBackstack = 199,
        
        /// <summary>
        /// URI of the content in the frame (string).
        /// </summary>
        FrameNodeURI = 200,
        
        /// <summary>
        /// Are other items on the screen available while the frame is loading? (bool)
        /// </summary>
        FrameBlocking = 201,

        /// <summary>
        /// Frame ID as identified as a signpost, or _top or _self (string).
        /// </summary>
        FrameID = 261,
        
        /// <summary>
        /// Signpost field list for actions (field list).
        /// </summary>
        SignpostSpec = 262
    }
    
    /// <summary>
    /// Actions for NaviAgentFieldID.ApplicationRequestAction
    /// </summary>
    public enum AppRequestAction : short
    {
        /// <summary>
        /// Login to the app only.
        /// </summary>
        LoginOnly = 1,
        
        /// <summary>
        /// Login and request the home node.
        /// </summary>
        GetAppEntryPage = 2,
        
        /// <summary>
        /// Login and request a specific page.
        /// </summary>
        GetPage = 3
    }

    /// <summary>
    /// Contact data items for NaviAgentFieldID.ContactKey
    /// </summary>
    /// <remarks>All of data type "string" except Thumbnail which is "binary".</remarks>
    public enum PIMItem : byte
    {
		FirstName = 1,
		Surname = 2,
		Nickname = 3,
		MobileTelephone = 4, // the default telephone number
        HomeTelephone = 5,
        WorkTelephone = 6,
		Email = 7,
		DateOfBirth = 8,
		Title = 9,
		CompanyName = 10,
		JobTitle = 11,
		Thumbnail = 12,
		
        /// <summary>
        /// The client can enumerate up to this number of pim types.
        /// </summary>
		MaximumFields = 30
    }

    /// <summary>
    /// Display types
    /// </summary>
    public enum DisplayType : byte
    {
        Null = 0,
        String = 1,
        Integer = 2,
        Float = 3, // packed as a Double field which is serialised as a BCD
        Image = 4, // binary
        ContentReference = 5, // field list
        ShortCut = 6,
        LatencyIndicator = 7,
        EditableString = 8,
        DownloadIndicator = 9,
        StreamingIndicator = 10,
        FocusTraversalIndicator = 11,
        CurrentTimeIndicator = 12,
        SignalIndicator = 13,
        SortSlot = 14,
        BgDownloadProgressSlot = 15,
        BgDownloadStatusSlot = 16,
        LastBgDownloadSlot = 17,
        ActiveBackgroundDownload = 18,
        CharacterCounter = 19,
        MediaMetaData = 20,
        RichText = 21,

        WVG = 99,

        /// <summary>
        /// Special placeholder for Free Slots in Grid Block Types (data as empty binary)
        /// </summary>
        Block = 100
    }

    /// <summary>
    /// Submit form results
    /// </summary>
    public enum SubmitFormResult
    {
        Empty = 0,
        NodeResponse = 1
    }

    /// <summary>
    /// Node transition types
    /// </summary>
    public enum NodeTransition
    {
		None = 0,
		SlideFromRight = 1,
		SlideFromLeft = 2,
		SlideOverFromRight = 3,
		SlideOffToRight = 4,
		SpringOn = 5,
		SpringOff = 6,
		
        // from 3.3.3:
		SlideFromTop = 7,
		SlideFromBottom = 8,
		SlideOverFromLeft = 9,
		SlideOverFromTop = 10,
		SlideOverFromBottom = 11,
		SlideOffToLeft = 12,
		SlideOffToTop = 13,
		SlideOffToBottom = 14
    }

    public enum TraversalIndicator
    {
        Both = 1,
		Vertical = 2,
		Horizontal = 3
    }

    public enum TextEntryMode
    {
        Any = 1,
		NumericOnly = 2,
		Password = 3,
		PhoneNumber = 4,
		Price = 5,
		
		// deprecated
		AlphaNumerical = 6
    }

    public enum NodeSource
    {
    	/// <summary>
        /// Node was received directly from the server.
        /// </summary>
		Server = 0,

		/// <summary>
        /// Node was pulled out of cache.
        /// </summary>
		Cache = 1
    }
}
