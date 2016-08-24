using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Wave.Common;
using Wave.Explorer;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class View : Panel
    {
        public const int BackstackLiveEntitiesLimit = 5;
        
        public long ID { get; private set; }
        public View ParentView { get; private set; }

        public bool InProgress
        {
            get { return progress.IsEnabled; }
            set { progress.IsEnabled = value; }
        }

        public bool HasBackstack
        {
            get { return isBackstackEnabled; }
            set
            {
                isBackstackEnabled = value;

                if (!value && (backstack.Count > 0))
                    ClearBackstack();
            }
        }

        public bool IsProgressScreenShaded
        {
            get { return progress.IsShaded; }
            set { progress.IsShaded = value; }
        }

        public event EventHandler InitialNodeLoad;

        #region Shortcut properties

        public Node CurrentNode
        {
            get
            {
                if (Children.Count > 0)
                    return Children[Children.Count - 1] as Node;
                else
                    return null;
            }
        }

        public FrameBlock ParentFrame
        {
            get { return Parent as FrameBlock; }
        }

        public short Signpost
        {
            get
            {
                if (ParentFrame != null)
                    return ParentFrame.Signpost;
                else
                    return -1;
            }
        }

        public bool IsBackKeyDelegate
        {
            get
            {
                if (ParentFrame != null)
                    return ParentFrame.BackAnchorDelegate;
                else
                    return false;
            }
        }

        public ActionSheetBlock ActionSheet
        {
            get { return actionSheet; }
        }

        #endregion

        #region Private fields

        private bool isBackstackEnabled = true;
        private Stack<ViewBackstackEntry> backstack = new Stack<ViewBackstackEntry>();

        private List<View> childViews = new List<View>();
        private ProgressControl progress = null;

        private BlockBase backKeyListener = null;
        private ActionSheetBlock actionSheet = null;

        private bool initialNodeLoaded = false;

        #endregion

        public View(View parentView, MainPage registrationOverride = null)
        {
            // setup internals
            ID = Core.UIFactory.NextViewID;
            ParentView = parentView;

            // create progress control
            progress = new ProgressControl();
            Canvas.SetZIndex(progress, 1000);
            Children.Add(progress);

            // no initial progress
            InProgress = false;

            // connect with the main page
            if (registrationOverride != null)
                registrationOverride.RegisterView(this);
            else
                Core.UI.RegisterView(this);
        }

        public void SignalNavigationStart()
        {
            InProgress = true;
        }

        public void SignalNavigationSuccess(NodeData data)
        {
            if (data != null)
            {
                Node newNode = CreateNewNode(data);

                if (newNode != null)
                {
                    if (!initialNodeLoaded)
                    {
                        initialNodeLoaded = true;

                        SignalInitialNodeLoad();
                    }
                    
                    ShowNewNode(newNode);
                }
            }

            InProgress = false;
        }

        public void SignalNavigationFailure()
        {
            if (!Core.Navigation.ExecuteApplicationEvent(Anchor.OnPageNotFound, null, CurrentNode, ID))
                UIHelper.Message("Connection to server failed!");

            InProgress = false;
        }

        public void SignalDestroy()
        {
            Node currentNode = CurrentNode;

            if (currentNode != null)
                currentNode.SignalDestroy();
        }

        public void SignalShow()
        {
            Node currentNode = CurrentNode;

            if (currentNode != null)
                currentNode.SignalShow();
        }

        public void SignalHide()
        {
            Node currentNode = CurrentNode;

            if (currentNode != null)
                currentNode.SignalHide();
        }

        #region Navigation

        public bool GoBack()
        {
            return ShowPreviousNode();
        }

        public void ClearBackstack()
        {
            foreach (ViewBackstackEntry entry in backstack)
                if (Children.Contains(entry.LiveEntity))
                    Children.Remove(entry.LiveEntity);
            
            backstack.Clear();
        }

        #endregion

        #region Node management

        private Node CreateNewNode(NodeData data)
        {
            Node newNode = new Node(this);

            newNode.HorizontalAlignment = HorizontalAlignment.Stretch;
            newNode.VerticalAlignment = VerticalAlignment.Stretch;
            newNode.Initialise(data);

            return newNode;
        }

        private void ShowNewNode(Node newNode)
        {
            if (newNode != null)
            {
                Node currentNode = CurrentNode;
                bool hideOldNode = true, destroyOldNode = false;

                // handle current node
                if (currentNode != null)
                {
                    ViewBackstackEntry newEntry = null;

                    if (isBackstackEnabled)
                        newEntry = currentNode.ToBackstack();

                    // method ToBackstack() returns null if the current node should not be added to backstack
                    if (newEntry != null)
                    {
                        // save current node to backstack
                        backstack.Push(newEntry);

                        // clean backstack (if needed)
                        if (backstack.Count > BackstackLiveEntitiesLimit)
                        {
                            var backstackEntries = backstack.ToArray();

                            for (int i = BackstackLiveEntitiesLimit; i < backstackEntries.Length; i++)
                            {
                                if (backstackEntries[i].LiveEntity != null)
                                {
                                    backstackEntries[i].LiveEntity.Visibility = Visibility.Collapsed;

                                    Children.Remove(backstackEntries[i].LiveEntity);
                                    backstackEntries[i].LiveEntity = null;
                                }
                            }
                        }

                        // set actions
                        hideOldNode = !newNode.IsPopup;
                        destroyOldNode = false;
                    }
                    else
                    {
                        // must hide and destroy old node
                        hideOldNode = true;
                        destroyOldNode = true;
                    }
                }

                // handle new node
                Children.Add(newNode);

                // launch transition
                TransitionHelper.Show(
                    newNode,
                    newNode.Transition,
                    () =>
                    {
                        if (currentNode != null)
                        {
                            // deactivate
                            currentNode.SignalHide();

                            // hide
                            if (hideOldNode)
                                currentNode.Visibility = Visibility.Collapsed;

                            // if needed - destroy
                            if (destroyOldNode)
                            {
                                Children.Remove(currentNode);

                                currentNode.SignalDestroy();
                                currentNode = null;
                            }
                        }

                        newNode.Visibility = Visibility.Visible;
                        newNode.SignalShow();
                    });
            }
        }

        private bool ShowPreviousNode()
        {
            // save current node
            Node currentNode = CurrentNode;

            // search for backstack entry
            ViewBackstackEntry entry = null;

            if (backstack.Count > 0)
                entry = backstack.Pop();

            if (entry != null)
            {
                // have old node - restore it
                Node prevNode = entry.LiveEntity;

                if (prevNode == null)
                {
                    prevNode = CreateNewNode(entry.ToNodeData());

                    if (prevNode != null)
                        Children.Add(prevNode);
                }

                if (prevNode != null)
                {
                    // deactivate current node (if any)
                    if (currentNode != null)
                        currentNode.SignalHide();

                    // show restored node and activate it
                    prevNode.Visibility = Visibility.Visible;
                    prevNode.SignalShow();
                    
                    // launch transition
                    TransitionHelper.Hide(
                        currentNode,
                        currentNode.Transition,
                        () =>
                        {
                            if (currentNode != null)
                            {
                                // hide
                                currentNode.Visibility = Visibility.Collapsed;

                                // destroy
                                Children.Remove(currentNode);

                                currentNode.SignalDestroy();
                                currentNode = null;
                            }
                        });

                    return true;
                }
            }
            else
            {
                // backstack is empty - just deactivate current node
                if (currentNode != null)
                {
                    currentNode.SignalHide();
                    currentNode.SignalDestroy();
                    currentNode = null;
                }
            }

            return false;
        }

        #endregion

        #region Child views

        public void RegisterChildView(View view)
        {
            if (!childViews.Contains(view))
                childViews.Add(view);
        }

        public void UnregisterChildView(View view)
        {
            if (childViews.Contains(view))
                childViews.Remove(view);
        }

        public View FindChildView(string signpost)
        {
            if (childViews.Count > 0)
            {
                if (CurrentNode != null)
                {
                    short signpostNumber = CurrentNode.FindSignpostNumber(signpost);

                    if (signpostNumber >= 0)
                    {
                        foreach (View childView in childViews)
                        {
                            if (childView.Signpost == signpostNumber)
                                return childView;
                        }
                    }
                }

                foreach (View childView in childViews)
                {
                    View temp = childView.FindChildView(signpost);

                    if (temp != null)
                        return temp;
                }
            }

            return null;
        }

        #endregion

        #region Back key listeners

        public void RegisterBackKeyListener(BlockBase block)
        {
            if ((block != null) && (backKeyListener == null))
                backKeyListener = block;
        }

        public void UnregisterBackKeyListener(BlockBase block)
        {
            if (backKeyListener == block)
                backKeyListener = null;
        }

        #endregion

        #region Action sheets

        public void RegisterActionSheet(ActionSheetBlock block)
        {
            if ((block != null) && (actionSheet == null))
                actionSheet = block;
        }

        public void UnregisterActionSheet(ActionSheetBlock block)
        {
            if (actionSheet == block)
                actionSheet = null;
        }
        
        #endregion

        #region Back key handling

        public View FindBackKeyDelegate()
        {
            View res = this;
            
            if (childViews.Count > 0)
            {
                foreach (View child in childViews)
                {
                    if (child.IsBackKeyDelegate)
                    {
                        res = child.FindBackKeyDelegate();
                        break;
                    }
                }
            }

            return res;
        }

        public bool HandleBackKey()
        {
            bool res = false;
            BlockBase notify = backKeyListener;

            // try the back key listener first
            if (notify != null)
            {
                res = notify.HasAction(Anchor.Back, ActionType.GotoBack);

                notify.FireAction(Anchor.Back);
            }

            // if not, try going back
            if (!res)
            {
                res = GoBack();

                if (!res && (ParentView != null))
                    return ParentView.HandleBackKey();
            }

            return res;
        }

        #endregion

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if ((Children.Count > 0) && !Double.IsInfinity(availableSize.Width) && !Double.IsInfinity(availableSize.Height))
            {
                foreach (UIElement child in Children)
                    child.Measure(availableSize);

                return availableSize;
            }
            else
                return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in Children)
            {
                if (child != progress)
                    child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }

            progress.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            
            return finalSize;
        }

        #endregion

        #region Events

        private void SignalInitialNodeLoad()
        {
            if (InitialNodeLoad != null)
                InitialNodeLoad(this, EventArgs.Empty);
        }

        #endregion
    }

    public class ViewBackstackEntry
    {
        public long ViewID { get; set; }

        public int ApplicationID { get; set; }
        public string URI { get; set; }
        public CacheItemID? CacheID { get; set; }
        public NodeTransition Transition { get; set; }
        public bool IsPopup { get; set; }
        public bool WasCached { get; set; } //?
        public bool ShouldGoToBackStack { get; set; } //?
        public int DefinitionID { get; set; }
        public List<string> SignpostNames { get; set; }

        public Node LiveEntity { get; set; }

        public NodeData ToNodeData()
        {
            return NodeData.Create(URI, CacheID, Transition, IsPopup, WasCached, ShouldGoToBackStack, DefinitionID, ApplicationID, SignpostNames);
        }
    }
}
