using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Shell;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class ApplicationBarBlock : ContainerBlock
    {
        public override bool IsHidden { get { return true; } }

        private bool isBarSetup = false;

        private List<ApplicationBarButtonBlock> buttons = null;
        private List<ApplicationBarMenuItemBlock> menuItems = null;

        private List<ApplicationBarIconButton> preparedButtons = null;
        private List<ApplicationBarMenuItem> preparedMenuItems = null;

        private ApplicationBarMode? mode = null;
        private Color? background = null;
        private Color? foreground = null;
        private double? opacity = null;

        public ApplicationBarBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            buttons = new List<ApplicationBarButtonBlock>();
            menuItems = new List<ApplicationBarMenuItemBlock>();

            preparedButtons = new List<ApplicationBarIconButton>();
            preparedMenuItems = new List<ApplicationBarMenuItem>();

            UnpackDefinition();
            UnpackContent();
        }

        private void UnpackDefinition()
        {
            if (ContainerDefinition != null)
            {
                mode = ContainerDefinition.RenderingHints["mode"].Equals("mini") ? ApplicationBarMode.Minimized : ApplicationBarMode.Default;

                string opacityString = ContainerDefinition.RenderingHints["opacity"];
                double opacityTest = 1.0;

                if (Double.TryParse(opacityString, out opacityTest) && (opacityTest >= 0.0) && (opacityTest <= 1.0))
                    opacity = opacityTest;
            }
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

                    if (newBlock != null)
                    {
                        if (newBlock is ApplicationBarButtonBlock)
                        {
                            buttons.Add((ApplicationBarButtonBlock)newBlock);
                        }
                        else if (newBlock is ApplicationBarMenuItemBlock)
                        {
                            menuItems.Add((ApplicationBarMenuItemBlock)newBlock);
                        }
                        else if (newBlock is ApplicationBarOptionsBlock)
                        {
                            ApplicationBarOptionsBlock colours = newBlock as ApplicationBarOptionsBlock;

                            if (colours != null)
                            {
                                if (colours.ForegroundColour.HasValue)
                                    foreground = colours.ForegroundColour.Value;

                                if (colours.BackgroundColour.HasValue)
                                    background = colours.BackgroundColour.Value;
                            }
                        }
                    }
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
                    foreach (ApplicationBarButtonBlock buttonData in buttons)
                    {
                        if (!String.IsNullOrEmpty(buttonData.Text))
                        {
                            ApplicationBarIconButton button = new ApplicationBarIconButton();
                            button.Text = buttonData.Text;
                            button.IconUri = buttonData.Icon;

                            preparedButtons.Add(button);

                            button.Click += (s, e) => { buttonData.Fire(); };
                        }
                    }
                }

                // add menu items
                if (menuItems.Count > 0)
                {
                    foreach (ApplicationBarMenuItemBlock menuItemData in menuItems)
                    {
                        if (!String.IsNullOrEmpty(menuItemData.Text))
                        {
                            ApplicationBarMenuItem menuItem = new ApplicationBarMenuItem();
                            menuItem.Text = menuItemData.Text;

                            preparedMenuItems.Add(menuItem);

                            menuItem.Click += (s, e) => { menuItemData.Fire(); };
                        }
                    }
                }
            }

            // setup the global appbar
            Core.UI.ApplicationBar.Set(preparedButtons, preparedMenuItems, mode, background, foreground, opacity);
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
