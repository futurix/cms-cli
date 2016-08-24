using System.Collections.Generic;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.Services
{
    public class NodeData
    {
        public string URI { get; set; }
        public CacheItemID? ID { get; set; }

        public NodeTransition Transition { get; set; }
        public bool IsPopup { get; set; }

        public bool WasCached { get; set; }
        public bool ShouldGoToBackStack { get; set; }

        public BlockDefinition Root { get; set; }
        public FieldList RootContent { get; set; }

        public int ApplicationID { get; set; }

        public List<string> SignpostNames { get; set; }

        public NodeData()
        {
            ApplicationID = -1;
            Transition = NodeTransition.None;
            IsPopup = false;
            SignpostNames = null;
        }

        public static NodeData Create(
            string uri, CacheItemID? ciid, NodeTransition transition, bool isPopup, 
            bool wasCached, bool addToBackstack, 
            BlockDefinition root, FieldList content, int appID,
            List<string> signpostNames = null)
        {
            NodeData data = new NodeData();

            data.URI = uri;
            data.ID = ciid;
            data.Transition = transition;
            data.IsPopup = isPopup;
            data.WasCached = wasCached;
            data.ShouldGoToBackStack = addToBackstack;
            data.Root = root;
            data.RootContent = content;
            data.ApplicationID = appID;
            data.SignpostNames = signpostNames;

            return data;
        }

        public static NodeData Create(
            string uri, CacheItemID? ciid, NodeTransition transition, bool isPopup, 
            bool wasCached, bool addToBackstack, 
            int definitionID, int appID,
            List<string> signpostNames = null)
        {
            BlockDefinition root = Core.Definitions.Find(appID, definitionID, true) as BlockDefinition;
            FieldList content = Core.Cache.Server[uri] as FieldList;

            return Create(uri, ciid, transition, isPopup, wasCached, addToBackstack, root, content, appID, signpostNames);
        }
    }
}
