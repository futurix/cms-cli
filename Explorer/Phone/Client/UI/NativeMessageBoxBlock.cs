using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Xna.Framework.GamerServices;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class NativeMessageBoxBlock : ContainerBlock
    {
        public const string DefaultButtonCaption = "OK";

        public bool IntendedForCustomAction { get; private set; }

        public override bool IsHidden { get { return true; } }

        private NativeMessageBoxBodyBlock body = null;
        private List<NativeMessageBoxButtonBlock> buttons = new List<NativeMessageBoxButtonBlock>();
        private MessageBoxIcon icon = MessageBoxIcon.None;

        public NativeMessageBoxBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            IntendedForCustomAction = false;
            
            // check for custom notification parameters
            if (ContainerDefinition != null)
            {
                string notify = ContainerDefinition.RenderingHints["notification"];

                switch (notify)
                {
                    case "none":
                        icon = MessageBoxIcon.None;
                        break;

                    case "alert":
                        icon = MessageBoxIcon.Alert;
                        break;

                    case "error":
                        icon = MessageBoxIcon.Error;
                        break;

                    case "warning":
                        icon = MessageBoxIcon.Warning;
                        break;
                }

                IntendedForCustomAction = ContainerDefinition.RenderingHints["show"].Equals("onAction", StringComparison.InvariantCultureIgnoreCase);
            }
            
            // unpack children
            UnpackContent();

            // register with the parent
            ParentNode.NativeMessageBox = this;
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
                        if (newBlock is NativeMessageBoxButtonBlock)
                            buttons.Add((NativeMessageBoxButtonBlock)newBlock);
                        else if (newBlock is NativeMessageBoxBodyBlock)
                            body = (NativeMessageBoxBodyBlock)newBlock;
                    }
                }
            }
        }

        public void Execute()
        {
            if (body != null)
            {
                string[] buttonCaptions = null;

                if (buttons.Count > 1)
                {
                    buttonCaptions = new string[2];

                    buttonCaptions[0] = buttons[0].Caption;
                    buttonCaptions[1] = buttons[1].Caption;
                }
                else
                {
                    buttonCaptions = new string[1];

                    if (buttons.Count == 1)
                        buttonCaptions[0] = buttons[0].Caption;
                    else
                        buttonCaptions[0] = DefaultButtonCaption;
                }

                var res = Guide.BeginShowMessageBox(body.Title, body.Message, buttonCaptions, 0, icon, null, null);

                if (res != null)
                {
                    int? ret = Guide.EndShowMessageBox(res);

                    if (ret.HasValue && (ret.Value >= 0) && (ret.Value < buttons.Count))
                        buttons[ret.Value].Fire();
                }
            }
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
