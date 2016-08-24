using System;
using System.Windows;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class FrameBlock : ContainerBlock
    {
        public string AwaitedURI { get; private set; }
        public bool LocalBackstack { get; private set; }
        public bool BackAnchorDelegate { get; private set; }

        public bool HasInitiatedNavigation { get; private set; }

        private View childView = null;
        
        private FrameDefinition Data
        {
            get { return ContainerDefinition as FrameDefinition; }
        }

        public FrameBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot, bool startNavigation = true)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            HasInitiatedNavigation = false;
            
            childView = new View(Host);
            Children.Add(childView);

            if (hostView != null)
                hostView.RegisterChildView(childView);

            if (Content != null)
            {
                AwaitedURI = Content[NaviAgentFieldID.FrameNodeURI].AsString();
                LocalBackstack = Content[NaviAgentFieldID.LocalBackstack].AsBoolean() ?? false;
                BackAnchorDelegate = Content[NaviAgentFieldID.BackAnchorDelegate].AsBoolean() ?? false;

                // setup backstack
                childView.HasBackstack = LocalBackstack;

                // start navigation
                if (startNavigation && !String.IsNullOrWhiteSpace(AwaitedURI))
                    StartNavigation();
            }
        }

        public override void AttachFormData(short formID, FieldList storage)
        {
            base.AttachFormData(formID, storage);

            if ((childView != null) && (childView.CurrentNode != null) && (childView.CurrentNode.RootBlock != null))
                childView.CurrentNode.RootBlock.AttachFormData(formID, storage);
        }

        public override void SignalShow()
        {
            base.SignalShow();

            if (childView != null)
                childView.SignalShow();
        }

        public override void SignalHide()
        {
            base.SignalHide();

            if (childView != null)
                childView.SignalHide();
        }

        public override void SignalDestroy()
        {
            base.SignalDestroy();

            if (Host != null)
                Host.UnregisterChildView(childView);

            if (childView != null)
                childView.SignalDestroy();
        }

        public void StartNavigation()
        {
            if (!HasInitiatedNavigation)
            {
                HasInitiatedNavigation = true;

                Core.Navigation.GoToNodeByURI(Core.Navigation.CheckURI(AwaitedURI), NodeTransition.None, false, childView.ID);
            }
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Double.IsInfinity(availableSize.Width) || Double.IsInfinity(availableSize.Height) || (availableSize.Width == 0) || (availableSize.Height == 0))
            {
                return new Size(0, 0);
            }
            else
            {
                foreach (UIElement child in Children)
                    child.Measure(availableSize);

                return availableSize;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in Children)
                child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            
            return finalSize;
        }

        #endregion
    }
}
