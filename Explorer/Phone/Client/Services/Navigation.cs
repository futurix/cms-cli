using System;
using System.Collections.Generic;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.UI;

namespace Wave.Services
{
    public class NavigationAgent : IMessageEndpoint
    {
        /// <summary>
        /// Standard prefix for any node URI, the absence of which shows a URI has been shortened.
        /// </summary>
        public const string StandardNodePrefix = "wave:/";
        
        /// <summary>
        /// String which can be found at the end of the node prefix, for use when the server send shortened URIs.
        /// </summary>
        public const string ShortMessageSearchKey = "/Node/Default/";

        // frame ID constants
        public const string TopFrameID = "_top";
        public const string SelfFrameID = "_self";

        public RequestManager Requests { get; private set; }

        /// <summary>
        /// Application-wide action sets.
        /// </summary>
        public ActionSetCollection ApplicationEvents { get; private set; } //TODO: clean this on new application!

        /// <summary>
        /// The prefix to add to any shortened node URIs for the current application.
        /// </summary>
        public string ApplicationMessagePrefix { get; private set; }

        //TODO: replace with per-node variable
        private int currentApplicationID = -1;

        public NavigationAgent()
        {
            Requests = new RequestManager();
            ApplicationEvents = null;
        }

        public void OnMessageReceived(WaveServerComponent dest, Enum msgID, WaveMessage data)
        {
            if (msgID is NaviAgentMessageID)
            {
                if (data != null)
                {
                    switch ((NaviAgentMessageID)msgID)
                    {
                        case NaviAgentMessageID.ApplicationResponse:
                            {
                                UnpackApplicationID(data.RootList);

                                // if we've got a cache item ID but no node URI in the message then the server is 
                                // telling us that the home node is already in our cache
                                if (data.RootList.GetItemCount(MessageOutFieldID.ItemURI) == 0)
                                {
                                    byte[] ciidData = data.RootList[MessageOutFieldID.CacheItemID].AsByteArray();

                                    if (ciidData != null)
                                    {
                                        CacheItemID ciid = new CacheItemID(ciidData);
                                        byte[] bin = Core.Cache.Server.FindKey(ciid);

                                        if (bin != null)
                                        {
                                            string uri = StringHelper.GetString(bin);

                                            if (!String.IsNullOrEmpty(uri))
                                                FindAndStoreApplicationMessagePrefix(uri);

                                            GoToNodeByID(ciid, String.Empty, NodeTransition.None, false, Core.UI.RootViewID);
                                            break;
                                        }
                                    }
                                }

                                // we also have the home node
                                goto case NaviAgentMessageID.NodeResponse;
                            }

                        case NaviAgentMessageID.NodeResponse:
                            {
                                UnpackReceivedNode(data.RootList);
                                break;
                            }

                        case NaviAgentMessageID.NodeIsInClientCache:
                            {
                                CacheItemID? ciid = null;

                                short requestID = data.RootList[MessageOutFieldID.RequestID].AsShort() ?? -1;
                                byte[] ciidData = data.RootList[MessageOutFieldID.CacheItemID].AsByteArray();
                                string uri = data.RootList[MessageOutFieldID.ItemURI].AsString();

                                if (ciidData != null)
                                    ciid = new CacheItemID(ciidData);

                                RequestData rd = Core.Navigation.Requests.Remove(requestID);

                                if (rd != null)
                                    GoToNodeByID(ciid, uri, rd.Transition, rd.IsPopup, rd.ViewID);
                                else
                                    GoToNodeByID(ciid, uri, NodeTransition.None, false, Core.UI.RootViewID);
                                break;
                            }

                        case NaviAgentMessageID.NodeResponseError:
                            {
                                View view = Core.UI[Core.Navigation.Requests.RemoveEx(data.RootList[MessageOutFieldID.RequestID].AsShort() ?? -1)];

                                if (view != null)
                                    view.SignalNavigationFailure();

                                break;
                            }

                        case NaviAgentMessageID.FormResponse:
                            {
                                if (data.RootList.GetItemCount(NaviAgentFieldID.SubmitFormResult) != 1)
                                    break; // illegal message, only one form result should exist

                                SubmitFormResult sfr = (SubmitFormResult)(data[NaviAgentFieldID.SubmitFormResult].AsShort() ?? (short)SubmitFormResult.Empty);

                                if (sfr == SubmitFormResult.NodeResponse)
                                    UnpackReceivedNode(data.RootList);

                                //TODO: add waiting for result for form submit/response cycle (when navigation is replaced)
                                break;
                            }
                        
                        default:
                            break;
                    }
                }
            }
        }

        #region Application utilities

        //TODO: perhaps it will be better if UnpackApplicationID() and FindAndStoreApplicationMessagePrefix() would use some
        // other ApplicationID values, perhaps saved from the last unpacked node
        private void UnpackApplicationID(FieldList source, NodeData data = null)
        {
            int newApplicationID = source[MessageOutFieldID.ApplicationID].AsInteger() ?? -1;

            if ((newApplicationID != -1) && (newApplicationID != currentApplicationID))
            {
                if (data != null)
                {
                    if (newApplicationID != data.ApplicationID)
                    {
                        // set application ID
                        data.ApplicationID = newApplicationID;
                        currentApplicationID = newApplicationID;

                        string itemURI = source[MessageOutFieldID.ItemURI].AsString();

                        if (!String.IsNullOrEmpty(itemURI))
                            FindAndStoreApplicationMessagePrefix(itemURI, newApplicationID);
                    }

                }
                else
                {
                    // set application ID
                    currentApplicationID = newApplicationID;

                    string itemURI = source[MessageOutFieldID.ItemURI].AsString();

                    if (!String.IsNullOrEmpty(itemURI))
                        FindAndStoreApplicationMessagePrefix(itemURI, newApplicationID);
                }
            }
        }

        private void FindAndStoreApplicationMessagePrefix(string uri, int appID = -1, string unqualifiedAppUri = null)
        {
            int slashCount = 0, slashIndex = 0, endIndexOfUnqualifiedUri = -1, endIndexOfMessagePrefix = -1;

            for (int i = 0; i < 7; i++)
            {
                slashIndex = uri.IndexOf('/', slashIndex + 1);

                if (slashIndex != -1)
                {
                    slashCount++;

                    switch (slashCount)
                    {
                        case 4:
                            endIndexOfUnqualifiedUri = slashIndex;
                            break;

                        case 7:
                            endIndexOfMessagePrefix = slashIndex;
                            break;
                    }
                }
                else
                    break;
            }

            if (String.IsNullOrEmpty(unqualifiedAppUri))
                unqualifiedAppUri = uri.Substring(0, endIndexOfUnqualifiedUri);

            // store the application ID in the cache agent
            if (appID != -1)
                Core.Cache.OnApplicationIDReceived(unqualifiedAppUri, appID);

            // store the application node prefix for shortened node URIs
            if (endIndexOfMessagePrefix != -1)
                ApplicationMessagePrefix = uri.Substring(0, endIndexOfMessagePrefix + 1);
            else
                ApplicationMessagePrefix = uri + ShortMessageSearchKey;
        }

        #endregion

        #region Node unpacking

        private void UnpackReceivedNode(FieldList source)
        {
            long targetID = -1;
            NodeTransition transition = NodeTransition.None;
            bool isPopup = false;

            // find target ID
            RequestData rq = Core.Navigation.Requests.Remove(source[MessageOutFieldID.RequestID].AsShort() ?? -1);

            if (rq != null)
            {
                targetID = rq.ViewID;
                transition = rq.Transition;
                isPopup = rq.IsPopup;
            }
            else
                targetID = Core.UI.RootViewID;

            if (targetID != -1)
                OnReceivedNode(source, transition, isPopup, targetID);

            //TODO: enable caching even if target ID is -1
        }

        private void UnpackCachedNode(FieldList source, NodeTransition transition, bool isPopup, long viewID)
        {
            if (viewID != -1)
                OnReceivedNode(source, transition, isPopup, viewID);

            //TODO: enable caching even if target ID is -1
        }

        private void OnReceivedNode(FieldList source, NodeTransition transition, bool isPopup, long targetID)
        {
            // get node URI
            string currentNodeURI = source[MessageOutFieldID.ItemURI].AsString();

            // unpack cache info and see if we have got some cache attributes
            CacheItemID? ciid = null;
            byte cacheHint = 0;

            byte[] ciidData = source[MessageOutFieldID.CacheItemID].AsByteArray();

            if (ciidData != null)
            {
                ciid = new CacheItemID(ciidData);
                cacheHint = source[MessageOutFieldID.CacheHint].AsByte() ?? 0;
            }

            if (String.IsNullOrEmpty(currentNodeURI) && !ciid.HasValue)
                return; // this field list does not contain any valid node data

            // cache node message
            if (Core.Cache.ShouldCache(cacheHint))
                Core.Cache.Server.Add(source, ciid, StringHelper.GetBytes(currentNodeURI), CacheEntityType.Node, cacheHint);

            UnpackNode(source, currentNodeURI, ciid, targetID, transition, isPopup);
        }

        private void UnpackNode(FieldList source, string currentNodeURI, CacheItemID? ciid, long targetID, NodeTransition transition, bool isPopup)
        {
            NodeData data = new NodeData();

            data.ApplicationID = currentApplicationID;
            data.ShouldGoToBackStack = !(source[NaviAgentFieldID.DoNotAddNodeToBackStack].AsBoolean() ?? false); //IMPLEMENT: back stack skipping

            // set node uri
            if (String.IsNullOrEmpty(currentNodeURI))
                currentNodeURI = source[MessageOutFieldID.ItemURI].AsString();

            data.URI = currentNodeURI;

            // set current node ciid
            if (!ciid.HasValue)
            {
                data.WasCached = false;

                byte cacheHint = 0;
                byte[] ciidData = source[MessageOutFieldID.CacheItemID].AsByteArray();

                if (ciidData != null)
                {
                    ciid = new CacheItemID(ciidData);
                    cacheHint = source[MessageOutFieldID.CacheHint].AsByte() ?? 0;

                    data.WasCached = Core.Cache.ShouldCache(cacheHint);
                }
            }
            else
                data.WasCached = true;

            // unpack application id
            UnpackApplicationID(source, data);

            data.ID = ciid;
            data.Transition = transition;
            data.IsPopup = isPopup;

            // Update the system events
            ApplicationEvents = Core.Definitions.GetApplicationEvents(data.ApplicationID);

            data.Root = null;

            int definitionID = source[MessageOutFieldID.DefinitionID].AsInteger() ?? 0;
            data.Root = Core.Definitions.Find(data.ApplicationID, definitionID) as BlockDefinition;
            data.RootContent = source;

            // unpack signpost names (if any)
            List<StringField> signpostNames = source.GetItems<StringField>(NaviAgentFieldID.SignpostName);

            if (signpostNames.Count > 0)
            {
                data.SignpostNames = new List<string>();

                foreach (StringField field in signpostNames)
                    data.SignpostNames.Add(field.Data);
            }

            Core.UI.SignalViewNavigationSuccess(targetID, data);
        }

        #endregion

        #region Actions and events

        public void ExecuteAction(ActionSet actionSet, BlockBase sourceBlock, Node sourceNode, long sourceViewID)
        {
            foreach (ActionBase action in actionSet.Actions)
            {
                switch (action.ActionType)
                {
                    case ActionType.GotoNodeStable:
                        {
                            GotoNodeStableAction gns = action as GotoNodeStableAction;

                            if (gns != null)
                                GoToNodeByURI(gns.URI, gns.Transition, gns.IsPopup, sourceViewID);

                            break;
                        }

                    case ActionType.GotoNodeStableSSP:
                        {
                            GotoNodeStableSSPAction gns = action as GotoNodeStableSSPAction;

                            if ((gns != null) && (gns.Signpost >= 0))
                            {
                                DisplayData dd = Core.UIFactory.FindSignpostedData(gns.Signpost, sourceViewID);

                                if ((dd != null) && (dd.DisplayType == DisplayType.String))
                                {
                                    string uri = dd.Data as string;

                                    if (!String.IsNullOrEmpty(uri))
                                        GoToNodeByURI(uri, gns.Transition, gns.IsPopup, sourceViewID);
                                }
                            }

                            break;
                        }

                    case ActionType.GotoNodeInFrame:
                        {
                            GotoNodeInFrameAction gnif = action as GotoNodeInFrameAction;

                            if (gnif != null)
                            {
                                View target = ResolveFrameID(gnif.FrameID, sourceBlock);

                                if (target != null)
                                    GoToNodeByURI(gnif.URI, gnif.Transition, gnif.IsPopup, target.ID);
                            }

                            break;
                        }

                    case ActionType.GotoBack:
                        {
                            GotoBackAction gb = action as GotoBackAction;

                            if (gb != null)
                            {
                                View view = Core.UI[sourceViewID];

                                if (view != null)
                                    view.GoBack();
                            }
                            
                            break;
                        }

                    case ActionType.Refresh:
                        {
                            RefreshAction rfr = action as RefreshAction;

                            if (rfr != null)
                            {
                                View target = ResolveFrameID(rfr.FrameID, sourceBlock);

                                if (target != null)
                                    GoToNodeByURI(target.CurrentNode.URI, NodeTransition.None, false, target.ID, true);
                            }
                            
                            break;
                        }

                    case ActionType.ClearBackStack:
                        {
                            ClearBackstackAction cbs = action as ClearBackstackAction;

                            if (cbs != null)
                            {
                                View target = ResolveFrameID(cbs.FrameID, sourceBlock);

                                if (target != null)
                                    target.ClearBackstack();
                            }

                            break;
                        }

                    case ActionType.SubmitForm:
                        {
                            SubmitFormAction sf = action as SubmitFormAction;

                            if (sf != null)
                                SendForm(sf, actionSet, sourceNode, sourceBlock, sourceViewID);

                            break;
                        }

                    case ActionType.SubmitFormToFrame:
                        {
                            SubmitFormToFrameAction sftf = action as SubmitFormToFrameAction;

                            if (sftf != null)
                            {
                                View target = ResolveFrameID(sftf.FrameID, sourceBlock);

                                if (target != null)
                                    SendForm(sftf, actionSet, sourceNode, sourceBlock, sourceViewID, true);
                            }

                            break;
                        }

                    case ActionType.GotoApplicationHome:
                        {
                            GotoApplicationHomeAction gah = action as GotoApplicationHomeAction;

                            if (gah != null)
                                GoToApplication(gah.URI, null, gah.Transition, gah.IsPopup, sourceViewID);

                            break;
                        }

                    case ActionType.GotoApplicationNode:
                        {
                            GotoApplicationNodeAction gan = action as GotoApplicationNodeAction;

                            if (gan != null)
                                GoToApplication(gan.ApplicationURN, gan.ItemURI, gan.Transition, gan.IsPopup, sourceViewID);

                            break;
                        }
                    
                    case ActionType.GotoNodeVolatile:
                    case ActionType.SaveApplicationToFavourites:
                    case ActionType.EPGSubmitForm:
                    case ActionType.EPGViewportUp:
                    case ActionType.EPGViewportDown:
                    case ActionType.EPGViewportForward:
                    case ActionType.EPGViewportBackward:
                    case ActionType.EPGViewportForward24Hours:
                    case ActionType.EPGViewportBackward24Hours:
                    case ActionType.EPGViewportGotoChannel:
                    case ActionType.UploadContentAndSubmitForm:
                    case ActionType.SetStateSSP:
                    case ActionType.ToggleStateSSP:
                        //IMPLEMENT: other actions
                        break;

                    default:
                        Core.UIFactory.ExecuteAction(action, sourceBlock, sourceNode, sourceViewID);
                        break;
                }
            }
        }

        public bool ExecuteApplicationEvent(Anchor anchor, BlockBase sourceBlock, Node sourceNode, long sourceViewID)
        {
            if (ApplicationEvents != null)
            {
                ActionSet actions = ApplicationEvents[anchor];

                if (actions != null)
                {
                    ExecuteAction(actions, sourceBlock, sourceNode, sourceViewID);
                    return true;
                }
            }

            return false;
        }

        private void GoToApplication(string applicationHome, string applicationHomeNode, NodeTransition transition, bool isPopup, long sourceViewID)
        {
            //TODO: check in cache first, before trying to send a message

            WaveMessage msg = new WaveMessage();

            Core.Navigation.Requests.Add(msg, sourceViewID, transition, isPopup);
            msg.AddString(NaviAgentFieldID.ApplicationURN, applicationHome);

            if (applicationHomeNode != null)
                msg.AddString(MessageOutFieldID.ItemURI, applicationHomeNode);

            Core.System.Location.AddLocationData(msg);

            msg.Send(WaveServerComponent.NavigationAgent, NaviAgentMessageID.GotoAnotherApplicationNode);

            Core.UI.SignalViewNavigationStart(sourceViewID);
        }

        private void GoToNodeByID(CacheItemID? id, string nodeURI, NodeTransition transition, bool isPopup, long targetViewID)
        {
            // check if node is in the cache
            object data = null;

            if (id.HasValue)
                data = Core.Cache.Server[id.Value];

            if (data == null)
            {
                // request node from server
                if (!String.IsNullOrEmpty(nodeURI))
                {
                    WaveMessage msg = new WaveMessage();

                    Core.Navigation.Requests.Add(msg, targetViewID, transition, isPopup);
                    msg.AddString(MessageOutFieldID.ItemURI, nodeURI);
                    Core.System.Location.AddLocationData(msg);

                    msg.Send(WaveServerComponent.NavigationAgent, NaviAgentMessageID.RequestNodeOnCacheError);

                    Core.UI.SignalViewNavigationStart(targetViewID);
                }
            }
            else
            {
                // load node from cache
                UnpackCachedNode((FieldList)data, transition, isPopup, targetViewID);

                // inform server
                if (id.HasValue || !String.IsNullOrEmpty(nodeURI))
                {
                    WaveMessage msg = new WaveMessage();

                    if (id.HasValue)
                        msg.AddBinary(MessageOutFieldID.CacheItemID, id.Value.ToByteArray());
                    else
                        msg.AddString(MessageOutFieldID.ItemURI, nodeURI);

                    msg.Send(WaveServerComponent.NavigationAgent, NaviAgentMessageID.SubscribeToNodeWithoutRequest);
                }
            }
        }

        public void GoToNodeByURI(string nodeURI, NodeTransition transition, bool isPopup, long sourceViewID, bool refresh = false)
        {
            // check if node is in the cache
            object data = Core.Cache.Server[nodeURI];

            if ((data == null) || refresh)
            {
                // not in cache, ask server
                Core.Network.EnsureConnection();

                WaveMessage msg = new WaveMessage();

                Core.Navigation.Requests.Add(msg, sourceViewID, transition, isPopup);

                if (refresh)
                    msg.AddBoolean(MessageOutFieldID.RefreshingNode, true);

                msg.AddString(MessageOutFieldID.ItemURI, nodeURI);
                Core.System.Location.AddLocationData(msg);

                msg.Send(WaveServerComponent.NavigationAgent, NaviAgentMessageID.RequestNode);

                Core.UI.SignalViewNavigationStart(sourceViewID);
            }
            else
            {
                if (data is FieldList)
                {
                    // inform server
                    WaveMessage msg = new WaveMessage();
                    msg.AddString(MessageOutFieldID.ItemURI, nodeURI);
                    msg.Send(WaveServerComponent.NavigationAgent, NaviAgentMessageID.SubscribeToNodeWithoutRequest);
                    
                    // unpack
                    UnpackCachedNode((FieldList)data, transition, isPopup, sourceViewID);
                }
                else
                    DebugHelper.Out("Unexpected data type: {0}", data.GetType().FullName);
            }
        }

        private void SendForm(FormAction trigger, ActionSet actionSet, Node sourceNode, BlockBase sourceBlock, long sourceViewID, bool expectResponse = false)
        {
            if (sourceNode != null)
            {
                WaveMessage msg = new WaveMessage();

                Core.Navigation.Requests.Add(msg, sourceViewID, trigger.Transition, trigger.IsPopUp);

                if (Core.CSLVersion == WaveCSLVersion.Version5)
                {
                    msg.AddInt16(NaviAgentFieldID.ActionTypeID, (short)trigger.ActionType);
                    msg.AddBoolean(NaviAgentFieldID.FormRequiresNoWait, trigger.WaitForNode);
                }
                else
                    msg.AddByte(NaviAgentFieldID.ActionRef, (byte)trigger.FormID);

                FieldList payload = FieldList.CreateField(NaviAgentFieldID.ActionPayload);

                payload.AddString(NaviAgentFieldID.FormRequestURL, trigger.FormURL);
                sourceNode.AttachFormData((short)trigger.FormID, payload);

                msg.AddFieldList(payload);

                Core.System.Location.AddLocationData(msg);

                if (sourceNode.CacheID != null)
                    msg.AddBinary(MessageOutFieldID.CacheItemID, sourceNode.CacheID.Value.ToByteArray());
                else
                    msg.AddString(MessageOutFieldID.ItemURI, sourceNode.URI);

                if (Core.CSLVersion != WaveCSLVersion.Version5)
                    msg.AddInt32(NaviAgentFieldID.ActionSetID, actionSet.DefinitionID);

                msg.Send(WaveServerComponent.NavigationAgent, NaviAgentMessageID.Action);

                if (expectResponse)
                    Core.UI.SignalViewNavigationStart(sourceViewID);
            }
        }

        #endregion

        #region Utilities

        public string CheckURI(string initialURI)
        {
            if (String.IsNullOrWhiteSpace(initialURI))
                return null;

            if (initialURI.StartsWith(StandardNodePrefix, StringComparison.InvariantCultureIgnoreCase))
                return initialURI;

            return String.Concat(ApplicationMessagePrefix, initialURI);
        }

        public View ResolveFrameID(string frameID, BlockBase source)
        {
            if (frameID.Equals(TopFrameID))
            {
                return Core.UI.RootView;
            }
            else if (source != null)
            {
                if (frameID.Equals(SelfFrameID))
                {
                    return source.Host;
                }
                else
                {
                    View temp = source.Host.FindChildView(frameID);

                    if (temp != null)
                        return temp;
                    else
                        return Core.UI.RootView.FindChildView(frameID);
                }
            }

            return null;
        }

        #endregion
    }
}
