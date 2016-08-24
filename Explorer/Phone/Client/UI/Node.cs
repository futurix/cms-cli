using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class Node : UserControl
    {
        #region View navigation / identification properties

        public long ViewID
        {
            get
            {
                return (parentView != null) ? parentView.ID : -1;
            }
        }

        public BlockBase RootBlock
        {
            get
            {
                return Content as BlockBase;
            }
        }

        #endregion

        #region Node application properties

        public int ApplicationID
        {
            get
            {
                return (data != null) ? data.ApplicationID : -1;
            }
        }

        public PaletteDefinition ApplicationPalette { get; set; }

        public string URI
        {
            get
            {
                return data != null ? data.URI : null;
            }
        }

        public CacheItemID? CacheID
        {
            get
            {
                return data != null ? data.ID : null;
            }
        }

        #endregion

        #region Node properties

        public bool IsPopup
        {
            get { return (data != null) ? data.IsPopup : false; }
        }

        public NodeTransition Transition
        {
            get { return (data != null) ? data.Transition : NodeTransition.None; }
        }

        #endregion

        public NativeMessageBoxBlock NativeMessageBox { get; set; }

        public SignpostedDataRegistrar SignpostedData { get; private set; }

        private View parentView = null;
        private NodeData data = null;

        private Dictionary<int, BlockBase> signpostedChildren = new Dictionary<int, BlockBase>();

        public Node(View view)
        {
            parentView = view;
            
            SignpostedData = new SignpostedDataRegistrar();
        }

        public void Initialise(NodeData source)
        {
            // save node data for the future use
            data = source;

            // try to create node contents
            if ((data != null) && (data.Root != null))
            {
                BlockBase block = Core.UIFactory.CreateAndInitialiseBlock(parentView, this, null, data.Root, data.RootContent, true);
                
                if (block != null)
                {
                    // always fill entire area of the node with root element
                    block.HorizontalAlignment = HorizontalAlignment.Stretch;
                    block.VerticalAlignment = VerticalAlignment.Stretch;

                    // show block
                    Content = block;
                }
            }

            // we are done
            if (RootBlock != null)
            {
                // check for native alerts to be triggered on node load and add trigger if it is not there
                if ((NativeMessageBox != null) && !NativeMessageBox.IntendedForCustomAction && !RootBlock.HasCustomAction(Anchor.OnNodeLoaded, CustomActions.NativeMessageBox))
                    RootBlock.AddAction(
                        Anchor.OnNodeLoaded,
                        new CustomAction(ActionSet.DefaultActionID, new string[] { CustomActions.NativeMessageBox }));
                
                // root block may need a special setup
                RootBlock.SignalIsRoot();

                // schedule launch for OnNodeLoaded actions
                ThreadHelper.Sync(() => RootBlock.FireAction(Anchor.OnNodeLoaded));
            }
        }

        public void Register(DisplayDataCollection data)
        {
            if (data != null)
            {
                foreach (DisplayData dd in data)
                    SignpostedData.Add(dd);
            }
        }

        #region Notifications

        public void OnLocationChanged()
        {
            if (RootBlock != null)
                RootBlock.FireAction(Anchor.OnLocationObtained);
        }

        public void OnLocationUnavailable()
        {
            if (RootBlock != null)
                RootBlock.FireAction(Anchor.OnLocationNotAvailable);
        }

        public void SignalShow()
        {
            if (RootBlock != null)
                RootBlock.SignalShow();

            IsHitTestVisible = true;
        }

        public void SignalHide()
        {
            IsHitTestVisible = false;
            
            if (RootBlock != null)
                RootBlock.SignalHide();
        }

        public void SignalDestroy()
        {
            if (RootBlock != null)
                RootBlock.SignalDestroy();
        }

        #endregion

        #region Signposts

        public void RegisterBlockSignpost(BlockBase block)
        {
            if (block.Signpost != FieldList.FieldNotFound)
                signpostedChildren[block.Signpost] = block;
        }

        public BlockBase FindBlockBySignpost(int signpost)
        {
            BlockBase res = null;

            signpostedChildren.TryGetValue(signpost, out res);

            return res;
        }

        #endregion

        #region Forms

        public void AttachFormData(short formID, FieldList storage)
        {
            if ((RootBlock != null) && (storage != null))
                RootBlock.AttachFormData(formID, storage);
        }

        #endregion

        #region Helper methods

        public short FindSignpostNumber(string signpostName)
        {
            if (data != null)
                return (short)data.SignpostNames.IndexOf(signpostName);
            else
                return FieldList.FieldNotFound;
        }

        public ViewBackstackEntry ToBackstack()
        {
            ViewBackstackEntry newEntry = null;

            if (data.ShouldGoToBackStack)
            {
                newEntry = new ViewBackstackEntry();
                
                newEntry.ViewID = ViewID;
                newEntry.ApplicationID = ApplicationID;
                newEntry.URI = URI;
                newEntry.CacheID = CacheID;
                newEntry.Transition = data.Transition;
                newEntry.IsPopup = data.IsPopup;
                newEntry.WasCached = data.WasCached;
                newEntry.ShouldGoToBackStack = data.ShouldGoToBackStack;
                newEntry.DefinitionID = (data.Root != null) ? data.Root.DefinitionID : -1;
                newEntry.SignpostNames = data.SignpostNames;

                newEntry.LiveEntity = this;
            }

            return newEntry;
        }

        #endregion
    }
}
