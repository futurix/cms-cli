using System;
using System.Windows;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.UI
{
    public class NativeMessageBoxButtonBlock : BlockBase
    {
        public string Caption { get; private set; }
        
        public override bool IsHidden { get { return true; } }

        private AtomicBlockDefinition Data
        {
            get { return Definition as AtomicBlockDefinition; }
        }

        private DisplayDataCollection serverDisplayData = null;

        public NativeMessageBoxButtonBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            Caption = String.Empty;
            
            // parse server display data
            serverDisplayData = DisplayData.Parse(content);

            // extract button caption
            if ((serverDisplayData.Count > 0) && (serverDisplayData[0].DisplayType == DisplayType.String))
                Caption = (string)serverDisplayData[0].Data;
        }

        public bool Fire()
        {
            return FireAction(Anchor.Fire);
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
