namespace Wave.Platform.Messaging
{
    public enum MessageOutFieldID : short
    {
        /// <summary>
        /// URL of a node that the client wishes to unsubscribe to.
        /// </summary>
        UnsubscribeNodeURI = 202, // string
        
        /// <summary>
        /// Clear all existing node subscriptions.
        /// </summary>
        ClearNodeSubscriptions = 203, // boolean
        
        RefreshingNode = 204, // boolean - true if the request is the result of a refresh action
        
        /// <summary>
        /// Subscription Application ID.
        /// </summary>
        ApplicationID = 207, // int32

        /// <summary>
        /// MD5 hash of data.
        /// </summary>
        MD5 = 210, // binary

        DataSourceID = 211, // int32

        ProfileID = 215, // int32

        UserName = 216, // string

        /// <remarks>
        /// Equates to the EPG owner identifier within the cluster service.
        /// </remarks>
        EPGID = 218, // binary

        EPGGenre = 219, // string

        SystemID = 220, // int32

        DefinitionID = 250, // int32

        BlockPrimitiveType = 251, // int16

        ContainerOrPluginType = 252, // int16

        SlotDisplayDataTypeID = 253, // byte; rename to DisplayData

        SlotDisplayData = 254, // any

        CacheItemID = 255, // binary

        CacheHint = 256, // byte

        ItemURI = 227, // string

        // media reference field list fields

        /// <summary>
        /// A unique binary "token" identifier for the media reference.
        /// </summary>
        ContentReferenceID = 231, // binary

        /// <summary>
        /// The media type: image, image-group, video, audio, etc.
        /// Default: image.
        /// </summary>
        MediaPrimitiveType = 232, // int16

        /// <summary>
        /// The media format: PNG, JPG, MPEG, etc.
        /// </summary>
        MediaTypeFormat = 233, // int16

        /// <summary>
        /// The delivery method: auto-download, request download, streamed, progressive download.
        /// Default: auto-download.
        /// </summary>
        DeliveryMethod = 234, // int16

        /// <summary>
        /// The version of the reference, if the customer changes the actual media that the reference 
        /// refers to the media binary size (not applicable to streams).
        /// </summary>
        Version = 235, // int32

        /// <summary>
        /// Filename for bulk downloads.
        /// </summary>
        Filename = 236, // string

        /// <summary>
        /// URL for HTTP downloads.
        /// </summary>
        HTTPDeliveryURL = 237, // string

        MediaMetadata = 238, // ?

        /// <summary>
        /// Total size of the download.
        /// </summary>
        DownloadSize = 5, // int32

        // media presentational attributes

        /// <summary>
        /// The width for 2D visual media.
        /// </summary>
        MediaActualWidth = 240, // int16

        /// <summary>
        /// The height for 2D visual media.
        /// </summary>
        MediaActualHeight = 241, // int16

        // image strips attributes

        /// <summary>
        /// The total number of sub-images in an image strip.
        /// </summary>
        ImageStripTotal = 243, // int16

        /// <summary>
        /// The index into the strip to draw.
        /// </summary>
        ImageStripIndex = 244, // int16

        // other media attributes

        /// <summary>
        /// A bitmask for access attributes.
        /// Default: zero (do not share).
        /// </summary>
        AccessFlag = 242, // int32

        /// <summary>
        /// The field list for a media reference and its meta data.
        /// </summary>
        ContentReference = 250, // field list

        /// <summary>
        /// Client-specified request id, echoed back by server in responses, valid per-session.
        /// </summary>
        RequestID = 260 // int16
    }
}
