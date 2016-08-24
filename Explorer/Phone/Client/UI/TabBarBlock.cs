using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Shell;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class TabBarBlock : ContainerBlock
    {
        public override bool IsHidden { get { return true; } }

        private bool isBarSetup = false;

        private List<TabBarButtonBlock> buttons = null;

        private List<ApplicationBarIconButton> preparedButtons = null;
        private List<ApplicationBarMenuItem> preparedMenuItems = null;

        private Color? background = null;
        private Color? foreground = null;

        public TabBarBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            buttons = new List<TabBarButtonBlock>();

            preparedButtons = new List<ApplicationBarIconButton>();
            preparedMenuItems = new List<ApplicationBarMenuItem>();

            UnpackContent();
        }

        private void UnpackContent()
        {
            // creating children
            List<FieldList> children = Content.GetItems<FieldList>(NaviAgentFieldID.BlockData);

            for (int i = 0; i < children.Count; i++)
            {
                int childDefinitionID = children[i][MessageOutFieldID.DefinitionID].AsInteger() ?? 0;
                DefinitionBase definition = Core.Definitions.Find(ParentNode.ApplicationID, childDefinitionID, true);

                if ((definition != null) && (definition is BlockDefinition))
                {
                    BlockBase newBlock = Core.UIFactory.CreateAndInitialiseBlock(Host, ParentNode, this, definition as BlockDefinition, children[i], false);

                    if ((newBlock != null) && (newBlock is TabBarButtonBlock))
                        buttons.Add((TabBarButtonBlock)newBlock);
                }
            }
        }

        public override void SignalShow()
        {
            base.SignalShow();

            // one time setup
            if (!isBarSetup)
            {
                isBarSetup = true;
                
                // add buttons
                if (buttons.Count > 0)
                {
                    for (int i = 0; (i < buttons.Count) && (i < 4); i++)
                    {
                        TabBarButtonBlock buttonRef = buttons[i];

                        ApplicationBarIconButton button = new ApplicationBarIconButton();
                        button.Text = !String.IsNullOrEmpty(buttonRef.Text) ? buttonRef.Text : WaveConstant.UnknownText;
                        button.IconUri = (buttonRef.Icon != null) ? buttonRef.Icon : new Uri(WaveConstant.InvalidPath, UriKind.Relative);

                        preparedButtons.Add(button);

                        button.Click += (s, e) => { buttonRef.Fire(); };
                    }

                    if (buttons.Count > 4)
                    {
                        for (int i = 4; i < buttons.Count; i++)
                        {
                            TabBarButtonBlock buttonRef = buttons[i];

                            ApplicationBarMenuItem menuItem = new ApplicationBarMenuItem();
                            menuItem.Text = !String.IsNullOrEmpty(buttonRef.Text) ? buttonRef.Text : WaveConstant.UnknownText;

                            preparedMenuItems.Add(menuItem);

                            menuItem.Click += (s, e) => { buttonRef.Fire(); };
                        }
                    }
                }
            }

            // set global appbar
            Core.UI.ApplicationBar.Set(preparedButtons, preparedMenuItems, null, background, foreground);
        }

        public override void SignalHide()
        {
            base.SignalHide();

            // reset global appbar
            Core.UI.ApplicationBar.Reset();
        }

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        #endregion
    }
}
