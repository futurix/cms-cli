using System;
using System.Windows;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.UI
{
    public class ApplicationBarMenuItemBlock : BlockBase
    {
        public string Text { get; private set; }
        
        public override bool IsHidden { get { return true; } }
        
        private AtomicBlockDefinition Data
        {
            get { return Definition as AtomicBlockDefinition; }
        }

        private DisplayDataCollection serverDisplayData = null;

        public ApplicationBarMenuItemBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            Text = String.Empty;

            // parse server display data
            serverDisplayData = DisplayData.Parse(content);

            // extract text
            if ((serverDisplayData.Count > 0) && (Data.SlotHints.Length > 0) &&
                !String.IsNullOrEmpty(Data.SlotHints[0]) && (serverDisplayData[0] != null) &&
                (serverDisplayData[0].DisplayType == DisplayType.String))
            {
                HintsDictionary hints = new HintsDictionary();
                hints.Parse(Data.SlotHints[0]);

                if (hints.ValueEquals(HintsDictionary.OfType, RenderingHint.ApplicationBarMenuItemText))
                    Text = (string)serverDisplayData[0].Data;
            }
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
