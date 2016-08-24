using System;
using System.Diagnostics;
using System.Windows;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.UI;

namespace Wave.Services
{
    public class UIAgent
    {
        public long NextViewID
        {
            get { return ++nextViewID; }
        }

        private long nextViewID = 0;

        public UIAgent()
        {
        }
        
        #region UI elements creation

        public BlockBase CreateAndInitialiseBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList contentData, bool isRoot)
        {
            if (definition != null)
            {
                BlockBase block = null;

                if (definition is ContainerBlockDefinition)
                {
                    ContainerBlockDefinition cdef = (ContainerBlockDefinition)definition;

                    switch (cdef.HintedType)
                    {
                        case UIHintedType.ApplicationBar:
                            block = new ApplicationBarBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.NativeMessageBox:
                            block = new NativeMessageBoxBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.TabBar:
                            block = new TabBarBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.ActionSheet:
                            block = new ActionSheetBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.Pivot:
                            block = new PivotBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.Panorama:
                            block = new PanoramaBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.None:
                        default:
                            {
                                if (cdef is BoxLayoutBlockDefinition)
                                    block = new BoxLayoutBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                                else if (cdef is GridBlockDefinition)
                                    block = new GridBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                                else if (cdef is ListBlockDefinition)
                                    block = new ListBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                                else if (cdef is FrameDefinition)
                                    block = new FrameBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                                
                                break;
                            }
                    }
                }
                else if (definition is CommonAtomicBlockDefinition)
                {
                    CommonAtomicBlockDefinition cabdef = (CommonAtomicBlockDefinition)definition;

                    switch (cabdef.HintedType)
                    {
                        case UIHintedType.ApplicationBarOptions:
                            block = new ApplicationBarOptionsBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.ApplicationBarButton:
                            block = new ApplicationBarButtonBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.ApplicationBarMenuItem:
                            block = new ApplicationBarMenuItemBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.NativeMessageBoxBody:
                            block = new NativeMessageBoxBodyBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.NativeMessageBoxButton:
                            block = new NativeMessageBoxButtonBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.TabBarButton:
                            block = new TabBarButtonBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.SystemTray:
                            block = new SystemTrayBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.Placeholder:
                            block = new PlaceholderBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                            break;

                        case UIHintedType.None:
                        default:
                            {
                                if (cabdef is AtomicBlockDefinition)
                                    block = new AtomicBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                                else if (cabdef is SingleSlotBlockDefinition)
                                    block = new SingleSlotBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                                
                                break;
                            }
                    }
                }
                else if (definition is ScrollingTextBlockDefinition)
                    block = new ScrollingTextBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);
                else if (definition is MapPluginBlockDefinition)
                    block = new MapPluginBlock(hostView, parentNode, parentBlock, definition, contentData, isRoot);

                if (block != null)
                {
                    if (block.IsHidden)
                        block.Visibility = Visibility.Collapsed;

                    return block;
                }
            }

            return null;
        }

        public BlockBase CreateAndInitialiseBlock(View hostView, Node parentNode, BlockBase parentBlock, FieldList definitionData, FieldList contentData, bool isRoot)
        {
            if (definitionData != null)
            {
                BlockDefinition bd = Core.Definitions.UnpackDefinition(definitionData) as BlockDefinition;

                if (bd != null)
                    return CreateAndInitialiseBlock(hostView, parentNode, parentBlock, bd, contentData, isRoot);
            }

            return null;
        }

        public RendererBase CreateRenderer(AtomicBlockBase parent, DisplayData data, SlotData slotData, SlotStyleData slotStyleData, TableLayoutItemInfo layout)
        {
            RendererBase res = null;

            if (data != null)
            {
                switch (data.DisplayType)
                {
                    case DisplayType.String:
                        res = new TextRenderer(parent);
                        break;

                    case DisplayType.ContentReference:
                        {
                            ContentReference cref = data.Data as ContentReference;

                            if (cref != null)
                            {
                                switch (cref.MediaType)
                                {
                                    case MediaPrimitiveType.Image:
                                    case MediaPrimitiveType.ImageStrip:
                                        res = new ImageRenderer(parent);
                                        break;

                                    default:
                                        DebugHelper.Out("Unsupported content reference media type: {0}", cref.MediaType);
                                        break;
                                }
                            }

                            break;
                        }

                    case DisplayType.MediaMetaData:
                        {
                            MediaMetaData mmd = data.Data as MediaMetaData;

                            if (mmd != null)
                            {
                                ContentReference cref = mmd[Core.System.CurrentDeviceGroup];

                                if (cref != null)
                                {
                                    switch (cref.MediaType)
                                    {
                                        case MediaPrimitiveType.Image:
                                        case MediaPrimitiveType.ImageStrip:
                                            res = new ImageRenderer(parent);
                                            break;

                                        case MediaPrimitiveType.String:
                                            res = new TextRenderer(parent);
                                            break;

                                        default:
                                            DebugHelper.Out("Unsupported content reference media type: {0}", cref.MediaType);
                                            break;
                                    }
                                }
                            }

                            break;
                        }

                    case DisplayType.EditableString:
                        res = new TextEntryRenderer(parent);
                        break;

                    default: //IMPLEMENT: more display types
                        DebugHelper.Out("Unsupported renderer type: {0}", data.DisplayType);
                        break;
                }

                if (res != null)
                {
                    res.SetDisplayData(data);
                    res.ApplyFormattingAndLayout(slotData, slotStyleData, layout);
                }
            }

            return res;
        }

        #endregion

        #region Actions and supporting methods

        public void ExecuteAction(ActionBase action, BlockBase sourceBlock, Node sourceNode, long sourceViewID)
        {
            if (action != null)
            {
                switch (action.ActionType)
                {
                    #region UI element visibility

                    case ActionType.SetVisibility:
                        {
                            SetVisibilityAction sv = action as SetVisibilityAction;

                            if (sv != null)
                            {
                                BlockBase target = FindBlock(sv.BlockSignpost, sourceBlock);

                                if (target != null)
                                    target.Visibility = sv.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                            }

                            break;
                        }

                    case ActionType.ToggleVisibility:
                        {
                            ToggleVisibilityAction tv = action as ToggleVisibilityAction;

                            if (tv != null)
                            {
                                BlockBase target = FindBlock(tv.BlockSignpost, sourceBlock);

                                if (target != null)
                                {
                                    if (target.Visibility == Visibility.Visible)
                                        target.Visibility = Visibility.Collapsed;
                                    else
                                        target.Visibility = Visibility.Visible;
                                }
                            }

                            break;
                        }

                    #endregion

                    #region Slot management

                    case ActionType.SetSlotContent:
                        {
                            SetSlotContentAction ssca = action as SetSlotContentAction;

                            if (ssca != null)
                            {
                                DisplayData data = FindSignpostedData((short)ssca.Target.ID, sourceBlock.Host.ID);

                                if (data != null)
                                    data.Update(ssca.Data);
                            }
                            
                            break;
                        }

                    #endregion

                    #region Telephony and contacts

                    case ActionType.GetContact:
                        {
                            GetContactAction gca = action as GetContactAction;

                            if (gca != null)
                            {
                                DisplayData target = FindSignpostedData(gca.TargetSignpost, sourceViewID);

                                if (target != null)
                                {
                                    TelephonyHelper.ChoosePhoneNumber(
                                        (sender, e) =>
                                        {
                                            if ((e != null) && (target != null))
                                                target.Data = e.Data;

                                            //TODO: create payload message for sourceBlock
                                        });
                                }
                            }

                            break;
                        }

                    case ActionType.TelephonySendSMSWithMessage:
                        {
                            TelephonySendSMSWithMessageAction ssmswm = action as TelephonySendSMSWithMessageAction;

                            if (ssmswm != null)
                                TelephonyHelper.SendText(ssmswm.PhoneNumber, ssmswm.Message);

                            break;
                        }

                    case ActionType.TelephonySendSMSSSP:
                        {
                            TelephonySendSMSSSPAction ssmsssp = action as TelephonySendSMSSSPAction;

                            if (ssmsssp != null)
                            {
                                DisplayData numberData = FindSignpostedData(ssmsssp.NumberSignpost, sourceViewID);
                                DisplayData messageData = FindSignpostedData(ssmsssp.MessageSignpost, sourceViewID);
                                BlockBase numberBlock = FindBlock(ssmsssp.NumberSignpost, sourceBlock);

                                string phoneNumber = null;
                                string message = null;

                                if ((numberBlock != null) && !String.IsNullOrWhiteSpace(numberBlock.FormSubmissionValue))
                                    phoneNumber = numberBlock.FormSubmissionValue;
                                else if ((numberData != null) && (numberData.DisplayType == DisplayType.String))
                                    phoneNumber = numberData.Data as string;

                                if ((messageData != null) && (messageData.DisplayType == DisplayType.String))
                                    message = messageData.Data as string;

                                TelephonyHelper.SendText(phoneNumber, message);
                            }

                            break;
                        }

                    #endregion

                    #region E-mail

                    case ActionType.SendEmail:
                        {
                            SendMailAction sm = action as SendMailAction;

                            if (sm != null)
                                TelephonyHelper.SendEmail(sm.To, sm.Subject, sm.Body);
                            
                            break;
                        }

                    case ActionType.SendEmailSSP:
                        {
                            SendMailSSPAction smssp = action as SendMailSSPAction;

                            if (smssp != null)
                                TelephonyHelper.SendEmail(
                                    FindSignpostedData(smssp.ToSignpost, sourceBlock), 
                                    FindSignpostedData(smssp.SubjectSignpost, sourceBlock), 
                                    FindSignpostedData(smssp.BodySignpost, sourceBlock));
                            
                            break;
                        }

                    #endregion

                    #region Media

                    case ActionType.FullScreenVideo:
                        break;

                    case ActionType.PlayMedia:
                        {
                            PlayMediaAction pma = action as PlayMediaAction;

                            if (pma != null)
                                Core.Downloads.OpenMediaStream(pma.MediaContentReference);

                            break;
                        }

                    case ActionType.PlayMediaSSP:
                        break;

                    case ActionType.PlayMediaWithAd:
                        {
                            //TODO: add support for advertising
                            PlayMediaWithAdAction pma = action as PlayMediaWithAdAction;

                            if (pma != null)
                                Core.Downloads.OpenMediaStream(pma.MediaContentReference);

                            break;
                        }

                    #endregion

                    #region Geolocation

                    case ActionType.SetLocationSharing:
                        {
                            SetLocationSharingAction loc = action as SetLocationSharingAction;

                            if (loc != null)
                            {
                                if (loc.Enable)
                                    Core.System.Location.Start();
                                else
                                    Core.System.Location.Stop();
                            }

                            break;
                        }

                    #endregion

                    #region Web browsing

                    case ActionType.ConnectOpenBrowser:
                        {
                            OpenBrowserAction ob = action as OpenBrowserAction;

                            if (ob != null)
                                BrowserHelper.Launch(ob.URL);

                            break;
                        }

                    case ActionType.OpenEmbeddedBrowser:
                        {
                            OpenBrowserEmbeddedAction obe = action as OpenBrowserEmbeddedAction;

                            if (obe != null)
                                BrowserHelper.LaunchEmbedded(obe.URL);

                            break;
                        }

                    #endregion

                    #region Social

                    case ActionType.PublishOnOnlineCommunity:
                        {
                            PublishOnSocialNetworkAction posn = action as PublishOnSocialNetworkAction;

                            if (posn != null)
                                SocialHelper.PublishSocial(posn.Target, posn.MessageData);

                            break;
                        }

                    case ActionType.PublishOnOnlineCommunitySSP:
                        {
                            PublishOnSocialNetworkSSPAction posnssp = action as PublishOnSocialNetworkSSPAction;

                            if (posnssp != null)
                            {
                                string message = FindSignpostedData(posnssp.MessageSignpost, sourceBlock);

                                if (message != null)
                                    SocialHelper.PublishSocial(posnssp.Target, message);
                            }

                            break;
                        }

                    #endregion

                    #region Extensibility

                    case ActionType.Custom:
                        {
                            CustomAction cs = action as CustomAction;

                            if ((cs != null) && (cs.Actions.Count > 0))
                            {
                                switch (cs.Actions[0])
                                {
                                    case CustomActions.NativeMessageBox:
                                        {
                                            if (sourceNode.NativeMessageBox != null)
                                                sourceNode.NativeMessageBox.Execute();

                                            break;
                                        }

                                    case CustomActions.ClearBackstack:
                                        {
                                            View view = Core.UI[sourceViewID];

                                            if (view != null)
                                                view.ClearBackstack();

                                            break;
                                        }

                                    case CustomActions.ActionSheet:
                                        {
                                            if (cs.Actions.Count > 1)
                                            {
                                                // cross-frame
                                                if (Core.UI.HasRootView)
                                                {
                                                    View target = Core.UI.RootView.FindChildView(cs.Actions[1]);

                                                    if ((target != null) && (target.ActionSheet != null))
                                                        target.ActionSheet.Launch();
                                                }
                                            }
                                            else
                                            {
                                                // local frame only
                                                View view = Core.UI[sourceViewID];

                                                if ((view != null) && (view.ActionSheet != null))
                                                    view.ActionSheet.Launch();
                                            }

                                            break;
                                        }

                                    case CustomActions.EnableSimpleTileUpdates:
                                        {
                                            if (cs.Actions.Count > 1)
                                            {
                                                string url = cs.Actions[1], user = null, pass = null;

                                                if (Core.Application.HasLogin)
                                                {
                                                    user = Core.Application.Login;
                                                    pass = Core.Application.PasswordString;
                                                }

                                                BackgroundHelper.EnableBackgroundTask(url, user, pass);
                                                BackgroundHelper.AddOrRenewBackgroundTask();
                                            }

                                            break;
                                        }

                                    case CustomActions.DisableSimpleTileUpdates:
                                        {
                                            BackgroundHelper.DisableBackgroundTask();
                                            break;
                                        }
                                    
                                    case CustomActions.MessageBox:
                                        {
                                            if (cs.Actions.Count > 1)
                                                UIHelper.Message(cs.Actions[1]);
                                            
                                            break;
                                        }
                                }
                            }

                            break;
                        }

                    #endregion
                }
            }
        }

        public BlockBase FindBlock(Signpost signpost, BlockBase sourceBlock)
        {
            if (signpost.ID != FieldList.FieldNotFound)
            {
                return sourceBlock.ParentNode.FindBlockBySignpost(signpost.ID);
            }
            else if ((signpost.FrameID != null) && (signpost.Name != null))
            {
                View target = Core.Navigation.ResolveFrameID(signpost.FrameID, sourceBlock);

                if ((target != null) && (target.CurrentNode != null))
                {
                    short signpostID = target.CurrentNode.FindSignpostNumber(signpost.Name);

                    if (signpostID != FieldList.FieldNotFound)
                        return target.CurrentNode.FindBlockBySignpost(signpostID);
                }
            }

            return null;
        }

        public BlockBase FindBlock(short signpostID, BlockBase sourceBlock)
        {
            if (signpostID != FieldList.FieldNotFound)
                return sourceBlock.ParentNode.FindBlockBySignpost(signpostID);
            else
                return null;
        }

        public string FindSignpostedData(int signpost, BlockBase sourceBlock)
        {
            string message = null;

            BlockBase block = FindBlock((short)signpost, sourceBlock);

            if ((block != null) && !String.IsNullOrEmpty(block.FormSubmissionValue))
                message = block.FormSubmissionValue;

            if (message == null)
            {
                DisplayData messageData = 
                    FindSignpostedData(
                        (short)signpost,
                        (sourceBlock.Host != null) ? sourceBlock.Host.ID : Core.UI.RootViewID);

                if (messageData.DisplayType == DisplayType.String)
                    message = (string)messageData.Data;
            }

            return message;
        }

        public DisplayData FindSignpostedData(short signpost, long sourceViewID)
        {
            View sourceView = Core.UI[sourceViewID];

            if ((sourceView != null) && (sourceView.CurrentNode != null))
                return sourceView.CurrentNode.SignpostedData[signpost];
            else
                return null;
        }

        #endregion

        #region Download management

        public void SignalDownloadReady(byte[] contentID, byte[] buffer)
        {
            Core.UI.ReferencedContent.Notify(contentID, buffer);
        }

        #endregion
    }

    public static class CustomActions
    {
        public const string NativeMessageBox = "ShowNativeAlertBox";
        public const string ClearBackstack = "ClearBackStack";
        public const string ActionSheet = "ShowActionSheet";

        public const string EnableSimpleTileUpdates = "EnableSimpleTileUpdates";
        public const string DisableSimpleTileUpdates = "DisableSimpleTileUpdates";
        
        public const string MessageBox = "MessageBox";
    }
}
