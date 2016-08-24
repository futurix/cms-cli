using System;
using System.Collections.Generic;
using Wave.Common;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.Platform
{
    public class ActionSet : DefinitionBase
    {
        public const int MaximumNumberOfAnchors = 39;
        public const byte DefaultActionID = 0;
        
        public List<Anchor> Anchors = new List<Anchor>();
        public List<ActionBase> Actions = new List<ActionBase>();

        public string Label { get; private set; }

        public ActionSet()
            : base()
        {
        }

        public bool HasAction(ActionType at)
        {
            foreach (ActionBase action in Actions)
                if (action.ActionType == at)
                    return true;
            
            return false;
        }

        public bool HasCustomAction(string customAction)
        {
            foreach (ActionBase action in Actions)
                if ((action is CustomAction) && ((CustomAction)action).IsAction(customAction))
                    return true;

            return false;
        }

        public void Unpack(FieldList source)
        {
			// set id (saving this as a definition ID for now)
            DefinitionID = source[NaviAgentFieldID.ActionSetID].AsInteger() ?? -1;

            // saving anchors
            List<ByteField> anchorData = source.GetItems<ByteField>(NaviAgentFieldID.AnchorID);

            if (anchorData.Count > 0)
            {
                // add available anchors
                foreach (ByteField field in anchorData)
                    Anchors.Add((Anchor)field.Data);
            }
            else
            {
                // add default anchor ("fire")
                Anchors.Add(Anchor.Fire);
            }

            // save action label
            Label = source[NaviAgentFieldID.ActionSetLabel].AsString();

            // adding actions
            FieldListNavigator nav = new FieldListNavigator(source);

            while (1 == 1)
            {
                ActionBase action = null;
                byte actionID = 0;

                if (nav.FindNext(NaviAgentFieldID.ActionRef))
                    actionID = nav.Current.AsByte() ?? 0;

                if (!nav.FindNext(NaviAgentFieldID.ActionTypeID))
                    break;

                ActionType actionType = (ActionType)(nav.Current.AsInteger() ?? (int)ActionType.Invalid);

                switch (actionType)
                {
                    #region Navigation

                    case ActionType.GotoNodeVolatile:
                        break;

                    case ActionType.GotoNodeStable:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string uri = Core.Navigation.CheckURI(nav.Current.AsString());
                            NodeTransition transition = NodeTransition.None;
                            bool isPopup = false;

                            if (nav.FindNext(NaviAgentFieldID.ActionPayload))
                            {
                                transition = (NodeTransition)(nav.Current.AsInteger() ?? (int)NodeTransition.None);

                                if (nav.FindNext(NaviAgentFieldID.ActionPopUp))
                                    isPopup = nav.Current.AsBoolean() ?? false;
                            }

                            action = new GotoNodeStableAction(actionID, uri, transition, isPopup);
                            break;
                        }

                    case ActionType.GotoNodeStableSSP:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            short signpost = nav.Current.AsShort() ?? -1;

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            NodeTransition transition = (NodeTransition)(nav.Current.AsInteger() ?? (int)NodeTransition.None);

                            nav.FindNext(NaviAgentFieldID.ActionPopUp);
                            bool isPopup = nav.Current.AsBoolean() ?? false;

                            action = new GotoNodeStableSSPAction(actionID, signpost, transition, isPopup);
                            break;
                        }

                    case ActionType.GotoNodeInFrame:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string uri = Core.Navigation.CheckURI(nav.Current.AsString());

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            NodeTransition transition = (NodeTransition)(nav.Current.AsInteger() ?? (int)NodeTransition.None);

                            nav.FindNext(NaviAgentFieldID.ActionPopUp);
                            bool isPopup = nav.Current.AsBoolean() ?? false;

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string frameID = nav.Current.AsString();

                            action = new GotoNodeInFrameAction(actionID, uri, frameID, transition, isPopup);
                            break;
                        }

                    case ActionType.GotoBack:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            NodeTransition transition = (NodeTransition)(nav.Current.AsInteger() ?? (int)NodeTransition.None);

                            action = new GotoBackAction(actionID, transition);
                            break;
                        }

                    case ActionType.GotoApplicationHome:
                        {
                            nav.FindNext(NaviAgentFieldID.ApplicationURN);
                            string uri = nav.Current.AsText();

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            NodeTransition transition = (NodeTransition)(nav.Current.AsInteger() ?? (int)NodeTransition.None);

                            nav.FindNext(NaviAgentFieldID.ActionPopUp);
                            bool isPopup = nav.Current.AsBoolean() ?? false;

                            action = new GotoApplicationHomeAction(actionID, uri, transition, isPopup);
                            break;
                        }

                    case ActionType.GotoApplicationNode:
                        {
                            string urn = null, uri = null;
                            
                            nav.FindNext(NaviAgentFieldID.ActionPayload);

                            FieldList fl = nav.Current as FieldList;

                            if (fl != null)
                            {
                                urn = fl[NaviAgentFieldID.ApplicationURN].AsText();
                                uri = fl[MessageOutFieldID.ItemURI].AsText();
                            }

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            NodeTransition transition = (NodeTransition)(nav.Current.AsInteger() ?? (int)NodeTransition.None);

                            nav.FindNext(NaviAgentFieldID.ActionPopUp);
                            bool isPopup = nav.Current.AsBoolean() ?? false;

                            action = new GotoApplicationNodeAction(actionID, urn, uri, transition, isPopup);
                            break;
                        }

                    case ActionType.Refresh:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string frameID = nav.Current.AsString();

                            if (!String.IsNullOrEmpty(frameID))
                                action = new RefreshAction(actionID, frameID);
                            break;
                        }

                    case ActionType.ClearBackStack:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string frameID = nav.Current.AsString();

                            if (!String.IsNullOrEmpty(frameID))
                                action = new ClearBackstackAction(actionID, frameID);
                            break;
                        }

                    #endregion

                    #region Media playback and control

                    case ActionType.PlayMedia:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);

                            action = new PlayMediaAction(actionID, nav.Current.AsString());
                            break;
                        }

                    case ActionType.PlayMediaSSP:
                        goto default;

                    case ActionType.PlayMediaWithAd:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string mediaCRef = nav.Current.AsString();

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string adCRef = nav.Current.AsString();

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            int delay = nav.Current.AsInteger() ?? 0;

                            action = new PlayMediaWithAdAction(actionID, mediaCRef, adCRef, delay);
                            break;
                        }

                    case ActionType.StopMedia:
                    case ActionType.FullScreenVideo:
                    case ActionType.MediaListPlayMedia:
                        goto default;

                    #endregion

                    #region Slot content

                    case ActionType.SetSlotContent:
                        {
                            Signpost sp = new Signpost(nav);
                            
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            FieldList fl = nav.Current.AsFieldList();

                            if (fl != null)
                            {
                                DisplayDataCollection dc = DisplayData.Parse(fl);

                                if (dc.Count > 0)
                                    action = new SetSlotContentAction(actionID, sp, dc[0]);
                            }
                            
                            break;
                        }

                    case ActionType.SetSlotContentSSP:
                    case ActionType.PrefetchContent:
                        goto default;

                    #endregion

                    #region Forms

                    case ActionType.SubmitForm:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);

                            FieldList fl = nav.Current as FieldList;

                            if (fl != null)
                            {
                                int formID = fl[NaviAgentFieldID.FormID].AsInteger() ?? -1;
                                string formURL = fl[NaviAgentFieldID.FormRequestURL].AsString();
                                NodeTransition transition = (NodeTransition)(fl[NaviAgentFieldID.FormTransition].AsInteger() ?? (int)NodeTransition.None);
                                bool waitForNode = fl[NaviAgentFieldID.FormRequiresNoWait].AsBoolean() ?? true;
                                bool popUp = fl[NaviAgentFieldID.ActionPopUp].AsBoolean() ?? false;

                                action = new SubmitFormAction(actionID, formID, formURL, transition, waitForNode, popUp);
                            }
                            
                            break;
                        }

                    case ActionType.SubmitFormToFrame:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);

                            FieldList fl = nav.Current as FieldList;

                            if (fl != null)
                            {
                                string frameID = fl[NaviAgentFieldID.FrameID].AsString();
                                int formID = fl[NaviAgentFieldID.FormID].AsInteger() ?? -1;
                                string formURL = fl[NaviAgentFieldID.FormRequestURL].AsString();
                                NodeTransition transition = (NodeTransition)(fl[NaviAgentFieldID.FormTransition].AsInteger() ?? (int)NodeTransition.None);
                                bool popUp = fl[NaviAgentFieldID.ActionPopUp].AsBoolean() ?? false;

                                action = new SubmitFormToFrameAction(actionID, frameID, formID, formURL, transition, popUp);
                            }
                            
                            break;
                        }

                    case ActionType.UploadContentAndSubmitForm:
                        goto default;

                    #endregion

                    #region Visibility

                    case ActionType.SetVisibility:
                        {
                            Signpost sn = new Signpost(nav);

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            bool isVisible = nav.Current.AsBoolean() ?? true;

                            action = new SetVisibilityAction(actionID, sn, isVisible);
                            break;
                        }

                    case ActionType.ToggleVisibility:
                        {
                            action = new ToggleVisibilityAction(actionID, new Signpost(nav));
                            break;
                        }

                    #endregion

                    #region Data channels

                    case ActionType.SubscribeToDataChannel:
                    case ActionType.SubscribeToDataChannelSSP:
                    case ActionType.UnsubscribeFromChannel:
                    case ActionType.ApplyFilterToDataChannel:
                        goto default;

                    #endregion

                    #region Downloads

                    case ActionType.StartBackgroundDownload:
                    case ActionType.StopBackgroundDownload:
                    case ActionType.BackgroundDownloadWithEndNode:
                    case ActionType.BackgroundHTTPRequest:
                        goto default;

                    #endregion

                    #region Telephony

                    case ActionType.TelephonySendSMSWithMessage:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string phoneNumber = nav.Current.AsText();

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string message = nav.Current.AsText();

                            if (!String.IsNullOrEmpty(phoneNumber) && !String.IsNullOrEmpty(message))
                                action = new TelephonySendSMSWithMessageAction(actionID, phoneNumber, message);

                            break;
                        }

                    case ActionType.TelephonyMakeCall:
                        goto default;

                    case ActionType.TelephonySendSMSSSP:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);

                            FieldList fld = nav.Current as FieldList;

                            if ((fld != null) && (fld.Count >= 2))
                            {
                                short? signpost1 = fld[0].AsShort();
                                short? signpost2 = fld[1].AsShort();

                                if (signpost1.HasValue && signpost2.HasValue)
                                    action = new TelephonySendSMSSSPAction(actionID, signpost1.Value, signpost2.Value);
                            }

                            break;
                        }

                    #endregion

                    #region E-mail

                    case ActionType.SendEmail:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string to = nav.Current.AsText();

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string subject = nav.Current.AsText();

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string body = nav.Current.AsText();

                            if (!String.IsNullOrEmpty(to) && !String.IsNullOrEmpty(subject) && (body != null))
                                action = new SendMailAction(actionID, to, subject, body);

                            break;
                        }

                    case ActionType.SendEmailSSP:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            int toSignpost = nav.Current.AsNumber() ?? Signpost.Invalid;

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            int subjectSignpost = nav.Current.AsNumber() ?? Signpost.Invalid;

                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            int bodySignpost = nav.Current.AsNumber() ?? Signpost.Invalid;

                            if ((toSignpost != Signpost.Invalid) && (subjectSignpost != Signpost.Invalid) && (bodySignpost != Signpost.Invalid))
                                action = new SendMailSSPAction(actionID, toSignpost, subjectSignpost, bodySignpost);

                            break;
                        }

                    #endregion

                    #region Browser

                    case ActionType.ConnectOpenBrowser:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string url = nav.Current.AsText();

                            if (!String.IsNullOrEmpty(url))
                                action = new OpenBrowserAction(actionID, url);
                            
                            break;
                        }

                    case ActionType.OpenEmbeddedBrowser:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string url = nav.Current.AsText();

                            if (!String.IsNullOrEmpty(url))
                                action = new OpenBrowserEmbeddedAction(actionID, url);

                            break;
                        }

                    #endregion

                    #region Contacts

                    case ActionType.GetContact:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            short signpost = (short)(nav.Current.AsNumber() ?? -1);

                            if (signpost >= 0)
                                action = new GetContactAction(actionID, signpost);

                            break;
                        }

                    case ActionType.GetContactNoThumbnail:
                        break;

                    case ActionType.SetContact:
                    case ActionType.GetAllContacts:
                        goto default;

                    #endregion

                    #region Mapping and location sharing

                    case ActionType.ToggleMapPin:
                    case ActionType.SetMapPin:
                    case ActionType.ShowMap:
                        goto default;

                    case ActionType.SetLocationSharing:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);

                            action = new SetLocationSharingAction(actionID, nav.Current.AsBoolean() ?? false);
                            break;
                        }

                    #endregion

                    #region Social

                    case ActionType.PublishOnOnlineCommunity:
                        {
                            // social network ID
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            SocialNetwork target = (SocialNetwork)(nav.Current.AsNumber() ?? (int)SocialNetwork.Unknown);

                            // message
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            string messageData = nav.Current.AsString();

                            if (messageData != null)
                                action = new PublishOnSocialNetworkAction(actionID, target, messageData);

                            break;
                        }

                    case ActionType.PublishOnOnlineCommunitySSP:
                        {
                            // social network ID signpost
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            SocialNetwork target = (SocialNetwork)(nav.Current.AsNumber() ?? (int)SocialNetwork.Unknown);

                            // message signpost
                            nav.FindNext(NaviAgentFieldID.ActionPayload);
                            int messageSignpost = nav.Current.AsNumber() ?? Signpost.Invalid;

                            if (messageSignpost != Signpost.Invalid)
                                action = new PublishOnSocialNetworkSSPAction(actionID, target, messageSignpost);
                            
                            break;
                        }

                    #endregion

                    #region Get media

                    case ActionType.GetPhotoFromCamera:
                    case ActionType.GetPhotoFromFile:
                    case ActionType.GetVideoFromCamera:
                    case ActionType.GetVideoFromFile:
                    case ActionType.GetAudioFromRecorder:
                    case ActionType.GetAudioFromFile:
                        goto default;

                    #endregion

                    #region Vectors

                    case ActionType.VectorSetAllObjectState:
                    case ActionType.VectorSetObjectState:
                    case ActionType.VectorSetAllObjectStateSSP:
                    case ActionType.VectorSetObjectStateSSP:
                        goto default;

                    #endregion

                    #region EPG

                    case ActionType.EPGViewportUp:
                    case ActionType.EPGViewportDown:
                    case ActionType.EPGViewportForward:
                    case ActionType.EPGViewportBackward:
                    case ActionType.EPGViewportForward24Hours:
                    case ActionType.EPGViewportBackward24Hours:
                    case ActionType.EPGViewportGotoChannel:
                    case ActionType.EPGSubmitForm:
                    case ActionType.EPGSetSlotContent:
                    case ActionType.EPGPlayMedia:
                    case ActionType.EPGSendSMS:
                        DebugHelper.Out("EPG functionality is not supported.");
                        break;

                    #endregion

                    #region UI

                    case ActionType.IncreaseFontSize:
                    case ActionType.DecreaseFontSize:
                        goto default;

                    #endregion

                    #region State management

                    case ActionType.SetStateSSP:
                    case ActionType.ToggleStateSSP:
                        goto default;

                    #endregion

                    #region Application

                    case ActionType.Quit:
                        action = new QuitAction(actionID);
                        break;

                    case ActionType.SaveApplicationToFavourites:
                    case ActionType.WaveSendAuditEvent:
                    case ActionType.SendCredentials:
                    case ActionType.ToggleBacklight:
                    case ActionType.ToggleInactivityTimer:
                        goto default;

                    #endregion

                    #region Custom

                    case ActionType.Custom:
                        {
                            nav.FindNext(NaviAgentFieldID.ActionPayload);

                            FieldList fl = nav.Current as FieldList;

                            if (fl != null)
                            {
                                List<string> strings = new List<string>();

                                foreach (IFieldBase field in fl)
                                {
                                    if (field is StringField)
                                        strings.Add(field.AsString());
                                }

                                action = new CustomAction(actionID, strings);
                            }

                            break;
                        }

                    #endregion

                    default:
                        DebugHelper.Out("Unsupported action type: {0}", actionType);
                        break;
                }

                if (action != null)
                    Actions.Add(action);
            }

            // done!
            IsUnpacked = true;
        }
    }
}
