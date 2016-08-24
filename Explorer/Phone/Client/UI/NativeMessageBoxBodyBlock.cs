using System;
using System.Windows;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class NativeMessageBoxBodyBlock : BlockBase
    {
        public const string DefaultMessage = "Your submission has been received successfully.";
        
        public string Title { get; private set; }
        public string Message { get; private set; }
        
        public override bool IsHidden { get { return true; } }

        private AtomicBlockDefinition Data
        {
            get { return Definition as AtomicBlockDefinition; }
        }
        
        private DisplayDataCollection serverDisplayData = null;

        public NativeMessageBoxBodyBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            Title = String.Empty;
            Message = String.Empty;

            if ((Data != null) && (Data.SlotHints != null))
            {
                // parse server display data
                serverDisplayData = DisplayData.Parse(content);

                // extract title and message
                if ((serverDisplayData.Count > 0) && (Data.SlotHints.Length > 0))
                    ProcessHintDataPair(Data.SlotHints[0], serverDisplayData[0]);

                if ((serverDisplayData.Count > 1) && (Data.SlotHints.Length > 1))
                    ProcessHintDataPair(Data.SlotHints[1], serverDisplayData[1]);
            }

            if (Title == String.Empty)
                Title = Core.ApplicationName;

            if (Message == String.Empty)
                Message = DefaultMessage;

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

        private void ProcessHintDataPair(string rh, DisplayData dd)
        {
            if (!String.IsNullOrEmpty(rh) && (dd != null))
            {
                HintsDictionary hints = new HintsDictionary();
                hints.Parse(rh);

                if (hints.ValueEquals(HintsDictionary.OfType, RenderingHint.NativeMessageBoxTitle) && (dd.DisplayType == DisplayType.String))
                    Title = (string)dd.Data;
                else if (hints.ValueEquals(HintsDictionary.OfType, RenderingHint.NativeMessageBoxMessage) && (dd.DisplayType == DisplayType.String))
                    Message = (string)dd.Data;
            }
        }
    }
}
