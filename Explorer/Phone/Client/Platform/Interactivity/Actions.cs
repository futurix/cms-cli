using System;
using System.Collections.Generic;
using Wave.Platform.Messaging;

namespace Wave.Platform
{
    public abstract class ActionBase
    {
        public byte ID { get; private set; }
        public ActionType ActionType { get; private set; }
        
        public ActionBase(byte id, ActionType at)
        {
            ID = id;
            ActionType = at;
        }
    }

    public class CommonAction : ActionBase
    {
        public object Data { get; private set; }

        public CommonAction(byte id, ActionType at, object data)
            : base(id, at)
        {
            Data = data;
        }
    }

    #region Node navigation

    public class GotoNodeStableAction : ActionBase
    {
        public string URI { get; private set; }
        public NodeTransition Transition { get; private set; }
        public bool IsPopup { get; private set; }

        public GotoNodeStableAction(byte id, string uri, NodeTransition transition, bool isPopup)
            : base(id, ActionType.GotoNodeStable)
        {
            URI = uri;
            Transition = transition;
            IsPopup = isPopup;
        }
    }

    public class GotoNodeStableSSPAction : ActionBase
    {
        public short Signpost { get; private set; }
        public NodeTransition Transition { get; private set; }
        public bool IsPopup { get; private set; }

        public GotoNodeStableSSPAction(byte id, short signpost, NodeTransition transition, bool isPopup)
            : base(id, ActionType.GotoNodeStableSSP)
        {
            Signpost = signpost;
            Transition = transition;
            IsPopup = isPopup;
        }
    }

    public class GotoBackAction : ActionBase
    {
        public NodeTransition Transition { get; private set; }

        public GotoBackAction(byte id, NodeTransition transition)
            : base(id, ActionType.GotoBack)
        {
            Transition = transition;
        }
    }

    #endregion

    #region Frames

    public abstract class FrameAction : ActionBase
    {
        public string FrameID { get; private set; }

        public FrameAction(byte id, ActionType at, string frameID)
            : base(id, at)
        {
            FrameID = frameID;
        }
    }

    public class GotoNodeInFrameAction : FrameAction
    {
        public string URI { get; private set; }

        public NodeTransition Transition { get; private set; }
        public bool IsPopup { get; private set; }

        public GotoNodeInFrameAction(byte id, string uri, string frameID, NodeTransition transition, bool isPopup)
            : base(id, ActionType.GotoNodeInFrame, frameID)
        {
            URI = uri;
            Transition = transition;
            IsPopup = isPopup;
        }
    }

    public class RefreshAction : FrameAction
    {
        public RefreshAction(byte id, string frameID)
            : base(id, ActionType.Refresh, frameID)
        {
        }
    }

    public class ClearBackstackAction : FrameAction
    {
        public ClearBackstackAction(byte id, string frameID)
            : base(id, ActionType.ClearBackStack, frameID)
        {
        }
    }

    #endregion

    #region Application navigation

    public class GotoApplicationHomeAction : ActionBase
    {
        public string URI { get; private set; }
        public NodeTransition Transition { get; private set; }
        public bool IsPopup { get; private set; }

        public GotoApplicationHomeAction(byte id, string uri, NodeTransition transition, bool isPopup)
            : base(id, ActionType.GotoApplicationHome)
        {
            URI = uri;
            Transition = transition;
            IsPopup = isPopup;
        }
    }

    public class GotoApplicationNodeAction : ActionBase
    {
        public string ApplicationURN { get; private set; }
        public string ItemURI { get; private set; }
        public NodeTransition Transition { get; private set; }
        public bool IsPopup { get; private set; }

        public GotoApplicationNodeAction(byte id, string urn, string uri, NodeTransition transition, bool isPopup)
            : base(id, ActionType.GotoApplicationNode)
        {
            ApplicationURN = urn;
            ItemURI = uri;
            Transition = transition;
            IsPopup = isPopup;
        }
    }

    #endregion

    #region UI visibility

    public class SetVisibilityAction : ActionBase
    {
        public Signpost BlockSignpost { get; private set; }
        public bool IsVisible { get; private set; }

        public SetVisibilityAction(byte id, Signpost blockSignpost, bool isVisible)
            : base(id, ActionType.SetVisibility)
        {
            BlockSignpost = blockSignpost;
            IsVisible = isVisible;
        }
    }

    public class ToggleVisibilityAction : ActionBase
    {
        public Signpost BlockSignpost { get; private set; }

        public ToggleVisibilityAction(byte id, Signpost blockSignpost)
            : base(id, ActionType.ToggleVisibility)
        {
            BlockSignpost = blockSignpost;
        }
    }

    #endregion

    #region Telephony and integration

    public class GetContactAction : ActionBase
    {
        public short TargetSignpost { get; private set; }

        public GetContactAction(byte id, short targetSignpost)
            : base(id, ActionType.GetContact)
        {
            TargetSignpost = targetSignpost;
        }
    }

    public class TelephonySendSMSWithMessageAction : ActionBase
    {
        public string PhoneNumber { get; private set; }
        public string Message { get; private set; }

        public TelephonySendSMSWithMessageAction(byte id, string phoneNumber, string message)
            : base(id, ActionType.TelephonySendSMSWithMessage)
        {
            PhoneNumber = phoneNumber;
            Message = message;
        }
    }

    public class TelephonySendSMSSSPAction : ActionBase
    {
        public short NumberSignpost { get; private set; }
        public short MessageSignpost { get; private set; }

        public TelephonySendSMSSSPAction(byte id, short numberSignpost, short messageSignpost)
            : base(id, ActionType.TelephonySendSMSSSP)
        {
            NumberSignpost = numberSignpost;
            MessageSignpost = messageSignpost;
        }
    }

    #endregion

    #region E-mail

    public class SendMailAction : ActionBase
    {
        public string To { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }

        public SendMailAction(byte id, string to, string subject, string body)
            : base(id, ActionType.SendEmail)
        {
            To = to;
            Subject = subject;
            Body = body;
        }
    }

    public class SendMailSSPAction : ActionBase
    {
        public int ToSignpost { get; private set; }
        public int SubjectSignpost { get; private set; }
        public int BodySignpost { get; private set; }

        public SendMailSSPAction(byte id, int to, int subject, int body)
            : base(id, ActionType.SendEmailSSP)
        {
            ToSignpost = to;
            SubjectSignpost = subject;
            BodySignpost = body;
        }
    }

    #endregion

    #region Multimedia

    public class PlayMediaAction : ActionBase
    {
        public string MediaContentReference { get; private set; }

        public PlayMediaAction(byte id, string cref)
            : this(id, ActionType.PlayMedia, cref)
        {
        }

        protected PlayMediaAction(byte id, ActionType at, string cref)
            : base(id, at)
        {
            MediaContentReference = cref;
        }
    }

    public class PlayMediaWithAdAction : PlayMediaAction
    {
        public string AdvertisingContentReference { get; private set; }
        public int TimeDelay { get; private set; }

        public PlayMediaWithAdAction(byte id, string media, string ad, int delay)
            : base(id, ActionType.PlayMediaWithAd, media)
        {
            AdvertisingContentReference = ad;
            TimeDelay = delay;
        }
    }

    #endregion

    #region Slot content

    public class SetSlotContentAction : ActionBase
    {
        public Signpost Target { get; private set; }
        public DisplayData Data { get; private set; }

        public SetSlotContentAction(byte id, Signpost target, DisplayData data)
            : base(id, ActionType.SetSlotContent)
        {
            Target = target;
            Data = data;
        }
    }

    #endregion

    #region Forms

    public abstract class FormAction : ActionBase
    {
        public int FormID { get; private set; }
        public string FormURL { get; private set; }
        public bool WaitForNode { get; private set; }
        public NodeTransition Transition { get; private set; }
        public bool IsPopUp { get; private set; }

        public FormAction(byte id, ActionType at, int formID, string formURL, bool waitForNode, NodeTransition transition, bool popUp)
            : base(id, at)
        {
            FormID = formID;
            FormURL = formURL;
            WaitForNode = waitForNode;
            Transition = transition;
            IsPopUp = popUp;
        }
    }

    public class SubmitFormAction : FormAction
    {
        public SubmitFormAction(byte id, int formID, string formURL, NodeTransition transition, bool waitForNode, bool popUp)
            : base(id, ActionType.SubmitForm, formID, formURL, waitForNode, transition, popUp)
        {
        }
    }

    public class SubmitFormToFrameAction : FormAction
    {
        public string FrameID { get; private set; }

        public SubmitFormToFrameAction(byte id, string frameID, int formID, string formURL, NodeTransition transition, bool popUp)
            : base(id, ActionType.SubmitFormToFrame, formID, formURL, true, transition, popUp)
        {
            FrameID = frameID;
        }
    }

    #endregion

    #region Geo

    public class SetLocationSharingAction : ActionBase
    {
        public bool Enable { get; private set; }

        public SetLocationSharingAction(byte id, bool enable)
            : base(id, ActionType.SetLocationSharing)
        {
            Enable = enable;
        }
    }

    #endregion

    #region Web navigation

    public class OpenBrowserAction : ActionBase
    {
        public string URL { get; private set; }

        public OpenBrowserAction(byte id, string url)
            : base(id, ActionType.ConnectOpenBrowser)
        {
            URL = url;
        }
    }

    public class OpenBrowserEmbeddedAction : ActionBase
    {
        public string URL { get; private set; }

        public OpenBrowserEmbeddedAction(byte id, string url)
            : base(id, ActionType.OpenEmbeddedBrowser)
        {
            URL = url;
        }
    }

    #endregion

    #region Social

    public class PublishOnSocialNetworkAction : ActionBase
    {
        public SocialNetwork Target { get; private set; }
        public string MessageData { get; private set; }

        public PublishOnSocialNetworkAction(byte id, SocialNetwork target, string messageData)
            : base(id, ActionType.PublishOnOnlineCommunity)
        {
            Target = target;
            MessageData = messageData;
        }
    }

    public class PublishOnSocialNetworkSSPAction : ActionBase
    {
        public SocialNetwork Target { get; private set; }
        public int MessageSignpost { get; private set; }

        public PublishOnSocialNetworkSSPAction(byte id, SocialNetwork target, int message)
            : base(id, ActionType.PublishOnOnlineCommunitySSP)
        {
            Target = target;
            MessageSignpost = message;
        }
    }

    #endregion

    #region Extensibility

    public class CustomAction : ActionBase
    {
        public List<string> Actions { get; private set; }

        public CustomAction(byte id, IEnumerable<string> actions)
            : base(id, ActionType.Custom)
        {
            Actions = new List<string>();

            if (actions != null)
                Actions.AddRange(actions);
        }

        public bool IsAction(string actionName)
        {
            if ((Actions.Count > 0) && Actions[0].Equals(actionName, StringComparison.InvariantCultureIgnoreCase))
                return true;
            else
                return false;
        }
    }

    #endregion

    #region Unsupported

    /// <summary>
    /// Quit (or ClientQuit) action is not supported as it is forbidden to quit application in code
    /// </summary>
    public class QuitAction : ActionBase
    {
        public QuitAction(byte id)
            : base(id, ActionType.Quit)
        {
        }
    }

    #endregion

    #region Constants

    /// <summary>
    /// Action type constants.
    /// </summary>
    /// <remarks>
    /// Server action means something that the server does when the client actions the action set, the client must send the action message for these.
    /// For an action set composed of only client actions the client will not send the action action-set message to the server.
    /// </remarks>
    public enum ActionType
    {
        Invalid = 0,

        GotoNodeVolatile = 1, // go to a node, no client payload sent (server)
        GotoNodeStable = 2, // go to node, requires client action payload of Node URN + transition type (client)
        GotoBack = 401, // go back ADK, requires transition type (client)

        TelephonySendSMSWithMessage = 102, // send SMS with message, action payload is the phone number, and message text as strings (client)
        TelephonyMakeCall = 104, // make call, action payload is the phone number (client)
        TelephonySendSMSSSP = 105, // payloads: a signpost to a slot containing the number, a signpost to a slot containing the message (client)
        SendEmail = 106, // send email, action payload is string for to, string for title, and string for text (client)
        SendEmailSSP = 107, // send email, action payload is short for to, short for title, short for text (client)

        ConnectOpenBrowser = 201, // open the browser, action payload is URL (client)

        WaveSendAuditEvent = 301, // send audit event, no payload either direction (client)
        SubmitForm = 302,
        // SubmitForm payloads:
        // S->C - { FormID, FormRequestURL }
        // C->S - { FormRequestURL, (SubmissionKey, SubmissionValue)* }
        // (client)

        UploadContentAndSubmitForm = 303, // ->
        SubmitFormToFrame = 304, // payload: { FrameID, FormID, FormRequestURL, FormTransition, ActionPopUp }
        // S->C - { FormID, FormRequestURL, UploadServerURL }
        // C->S
        // As SubmitForm but the client also checks the form content buffers for any content fetched via the GetContent actions (GetPhotoFromCamera, etc)
        // that is marked as part of that form by the same Form ID. Where there are any applicable form content buffers these are uploaded.
        // The upload ID string (as sent back by the HTTP upload server) is then sent as the value for the key associated with the GetContent action.

        // content actions
        GetPhotoFromCamera = 310,
        GetPhotoFromFile = 311,
        GetVideoFromCamera = 312,
        GetVideoFromFile = 313,
        GetAudioFromRecorder = 314,
        GetAudioFromFile = 315,
        // content actions payload: { FormID, SubmissionKey, [Signpost] }
        // - an optional signpost to put the content once it's fetched

        Quit = 402, // action to quit (client/other)
        SetSlotContent = 403, // set the contents of a slot, payload is a list of { Signpost, SlotDisplayDataTypeID, SlotDisplayData } (client/other)
        PrefetchContent = 404, // set the contents of a slot (client/other)
        Custom = 405, // action for bespoke bonding payload is a field list (ActionPayload) of strings (ActionPayload) (client/other)
        FullScreenVideo = 406, // action that instructs the client to make a video block display in full screen (client/other)
        ToggleBacklight = 408, // action toggles given backlight on clients machine (client/other)
        ToggleInactivityTimer = 407, // action toggles inactivity timer on client (client/other)
        PlayMedia = 409, // action for use with audio media references; if it is used with an image or video media the client should just show the media on a unbranded clean screen, or we just don't support it (client/other)
        SubscribeToDataChannel = 410, // payload: channel long ID, subchannel ID (client/other)
        ApplyFilterToDataChannel = 411, // payload: channel long ID and filter ID (client/other)

        StartBackgroundDownload = 414, // payload: content ref for downloaded, a display reference string, an optional ontent-ref for display thumbnail
        StopBackgroundDownload = 415, // no payload: only 1 background download can be done at a time
        DeleteItem = 416,
        BackgroundDownloadWithEndNode = 417, // payload: ActionSlot, ActionText, ItemURI, ActionPageTransition, ActionPopUp

        // private APIs that exist only for NASCAR and must be replaced by a generic vector update via weMessage mechanism ASAP
        VectorSetAllObjectState = 500, // payload: vec object type ID (binary), new state (binary) (client)
        VectorSetObjectState = 501, // payload: vec object instance ID (binary), new state (binary) (client)

        // payloaded actions; the following actions need to be able to get 1 or more parameters from slots, so the payload will contain a signpost
        SetSlotContentSSP = 502, // payload: Signpost (to assign data to), Signpost (to get data from) (client/other)
        PlayMediaSSP = 503, // payload: Signpost (to get data from) (client/other)
        SubscribeToDataChannelSSP = 504, // payload: Signpost (to get channel), Signpost (subchannel) (client/other)
        SendCredentials = 505, // payload: phone number (client/other)
        VectorSetAllObjectStateSSP = 506, // payload: signpost ID (int16, for the slot that contains vec object type ID), new state (binary)
        VectorSetObjectStateSSP = 507, // payload: signpost ID (int16, for the slot that contains vec object instance ID), new state (binary)
        UnsubscribeFromChannel = 508, // payload: channel name (binary)
        GotoNodeStableSSP = 509, // payload: signpost ID (int16) with string content of URI to go to

        // EPG extended actions
        EPGSubmitForm = 510, // payload: form response URL (string), extended variable/action lookup key (byte)
        EPGSetSlotContent = 511, // payload: Signpost (to assign data to), extended variable/action lookup key (byte)
        EPGPlayMedia = 512, // payload: extended variable/action lookup key (byte)
        EPGSendSMS = 513, // payload: telephone number (string), extended variable/action lookup key (byte)

        GotoApplicationHome = 514, // payload: ApplicationUnqualifiedUri (string)
        GotoApplicationNode = 515, // payload: ApplicationUnqualifiedUri (string), NodeUriLocal (string) OR ApplicationQualifiedUri (string)
        SaveApplicationToFavourites = 516, // payload: EntryName (string), ApplicationUnqualifiedUri (string), ServerAddress (string), port (int16)
        MediaListPlayMedia = 520,

        EPGViewportUp = 530,
        EPGViewportDown = 531,
        EPGViewportForward = 532,
        EPGViewportBackward = 533,
        EPGViewportForward24Hours = 534,
        EPGViewportBackward24Hours = 535,
        EPGViewportGotoChannel = 536,

        // Explorer content fetching actions; PIM, content upload, etc
        GetContact = 600, // fetch a complete contact with thumbnail; payload: a signpost to put the contact name once it's fetched
        GetContactNoThumbnail = 601, // placeholder
        SetContact = 602, // set/merge a set of contact details; payload: same as ContactData (contact details fieldlist) + [Signpost] an optional signpost for a thumbnail
        GetAllContacts = 603,

        StopMedia = 704, // (client)
        SetStateSSP = 708,
        BackgroundHTTPRequest = 709, // (client)
        ToggleStateSSP = 710,
        OpenEmbeddedBrowser = 711, // (client)
        PlayMediaWithAd = 712, // (client)

        ToggleMapPin = 713,
        SetMapPin = 714,
        ShowMap = 715,
        IncreaseFontSize = 716, // payload: signpost of container block whose child blocks should be redrawn with an increased font size for text slots (client)
        DecreaseFontSize = 717, // payload: signpost of container block whose child blocks should be redrawn with a decreased font size for text slots (client)
        PublishOnOnlineCommunity = 718,
        PublishOnOnlineCommunitySSP = 719,
        SetLocationSharing = 720, // payload: boolean of whether to turn location on or off
        SetVisibility = 722, // payload: signpost of the container block and boolean, true if visibile
        ToggleVisibility = 723, // payload: signpost of the container block
        GotoNodeInFrame = 724,
        ClearBackStack = 725, // payload: string (frame ID or _top or _self) - clears target frames back stack
        Refresh = 726, // payload: frame ID (string)
        OpenFullscreenFeature = 727 // payloads (all strings except numParams): authority , name, numParams, (paramName, paramValue)*
    }

    public enum Anchor : byte
    {
        Fire = 0,
        HotKeyLeft = 1,
        HotKeyRight = 2,
        Back = 3,
        Left = 4,
        Right = 5,
        Up = 6,
        Down = 7,
        Key0 = 8,
        Key1 = 9,
        Key2 = 10,
        Key3 = 11,
        Key4 = 12,
        Key5 = 13,
        Key6 = 14,
        Key7 = 15,
        Key8 = 16,
        Key9 = 17,
        KeyStar = 18,
        KeyPound = 19,
        KeyDelete = 20,
        KeySearch = 21,

        // non-keys (100+)
        OnFocused = 100,
        OnNodeLoaded = 101,
        OnChecked = 102,
        OnUnchecked = 103,
        SwitchTo2ndScreen = 104,
        SwitchTo1stScreen = 105,
        OnUnfocused = 106,

        // extra node states
        OnBackgroundDownloadComplete = 107,
        OnBackgroundDownloadFailure = 108,
        OnBackgroundDownloadAlreadyGot = 109,
        OnStall = 110,
        OnRequestTimeout = 111,
        OnPageNotFound = 112,
        OnContentDelivered = 113,
        OnLocationObtained = 114,
        OnLocationNotAvailable = 115,
        OnSwitchToLandscape = 116,
        OnSwitchToPortrait = 117,
        OnSwipeLeft = 118,
        OnSwipeRight = 119,
        OnSwipeUp = 120,
        OnSwipeDown = 121,
        OnVideoPlaybackStart = 122,
        OnVideoPlaybackStop = 123,
        OnVideoPlaybackPause = 124,
        OnVideoPlaybackResume = 125,
        OnVideoPlaybackEnd = 126,
        OnVideoPlaybackFail = 127
    }

    public enum SocialNetwork
    {
        Unknown = 0,

        Facebook = 1,
        Twitter = 2
    }

    #endregion
}
