namespace Wave.Platform.Messaging
{
    public enum MediaAgentMessageID : short
    {
        RequestDownload = 1,
		OpenStream = 2,
		OpenStreamResponse = 3,
		FastStreamSwitchResponse = 4,
		DownloadResponse = 5,
		Frame = 6,
		FrameAcknowledgement = 7,
		MediaStreamError = 8,
		DownloadError = 9,
		DownloadStaledCancellation = 10,
		DownloadCancellationAcknowledgement = 11,
        RequestBulkDownload = 12,
        WVGFile = 20,
		DownloadBatchResponse = 21,
		RequestBackgroundDownload = 13,
        BackgroundDownloadResponse = 14,
        BackgroundFrame = 15
    }

    public enum MediaReferenceFieldID : short
    {
		StreamURL = 1, // string
        StreamSessionID = 2, // binary
        ExternalURL = 3, // string
        BinaryFrame = 4, // binary
		FrameNumber = 6, // int16
        FrameSize = 7, // int16
        ErrorMessage = 8, // string

        NonMSGStream = 9, // boolean

        DeviceProfileGroup = 100 // int16
    }

    public enum MediaPrimitiveType : short
    {
        Unspecified = 0,
		Image = 1,
		ImageStrip = 2,
		Video = 3,
		Audio = 4,
		String = 5,
		WVG = 6
    }

    public enum MediaFormat : short
    {
		Unspecified = 0,
		PNG = 1,
		JPEG = 2,
		GIF = 3,
		AnimatedGIF = 4
    }

    public enum MediaDelivery : short
    {
		Unspecified = 0,
		AutoDownload = 1,
		RequestDownload = 2,
		Stream = 3,
		BulkRequestDownload = 4,
		ResidentOnHandset = 5,
		BackgroundDownload = 6,
		ViaHTTP = 7,
        ViaHTTPBackground = 8
    }
}
