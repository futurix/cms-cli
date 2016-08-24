using System;
using System.Windows;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class ApplicationBarButtonBlock : BlockBase
    {
        public string Text { get; private set; }
        public Uri Icon { get; private set; }
        
        public override bool IsHidden { get { return true; } }
        
        private AtomicBlockDefinition Data
        {
            get { return Definition as AtomicBlockDefinition; }
        }

        private DisplayDataCollection serverDisplayData = null;

        public ApplicationBarButtonBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            Text = String.Empty;
            Icon = null;

            // parse server display data
            serverDisplayData = DisplayData.Parse(content);

            // extract text and icon
            if ((serverDisplayData.Count > 0) && (Data.SlotHints.Length > 0))
                ProcessHintDataPair(Data.SlotHints[0], serverDisplayData[0]);

            if ((serverDisplayData.Count > 1) && (Data.SlotHints.Length > 1))
                ProcessHintDataPair(Data.SlotHints[1], serverDisplayData[1]);
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

        private void ProcessHintDataPair(string rh, DisplayData dd)
        {
            if (!String.IsNullOrEmpty(rh) && (dd != null))
            {
                HintsDictionary hints = new HintsDictionary();
                hints.Parse(rh);

                if (hints.ValueEquals(HintsDictionary.OfType, RenderingHint.ApplicationBarButtonIcon))
                {
                    if (dd.DisplayType == DisplayType.String)
                    {
                        string tempPath = dd.Data as string;

                        if (!String.IsNullOrWhiteSpace(tempPath))
                            Icon = new Uri(String.Concat(SettingsAgent.ResidentIconsPath, tempPath), UriKind.Relative);
                    }
                    else if (dd.DisplayType == DisplayType.ContentReference)
                    {
                        ContentReference cref = dd.Data as ContentReference;

                        if ((cref != null) && (cref.MediaType == MediaPrimitiveType.Image) && (cref.Delivery == MediaDelivery.ResidentOnHandset))
                        {
                            string tempPath = Core.Settings.ResolveResidentMedia(cref.FileName, cref.DeviceGroup);

                            if (!String.IsNullOrWhiteSpace(tempPath))
                                Icon = new Uri(tempPath, UriKind.Relative);
                        }
                    }
                }
                else if (hints.ValueEquals(HintsDictionary.OfType, RenderingHint.ApplicationBarButtonText))
                {
                    if (dd.DisplayType == DisplayType.String)
                        Text = (string)dd.Data;
                }
            }
        }
    }
}
