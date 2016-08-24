using System;
using System.Windows;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public class TabBarButtonBlock : BlockBase
    {
        public string Text { get; private set; }
        public Uri Icon { get; private set; }

        public override bool IsHidden { get { return true; } }

        private DisplayDataCollection serverDisplayData = null;

        public TabBarButtonBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
            Text = String.Empty;
            Icon = null;

            // parse server display data
            serverDisplayData = DisplayData.Parse(content);

            // extract text and icon
            AtomicBlockDefinition data = Definition as AtomicBlockDefinition;

            if (data != null)
            {
                for (int i = 0; i < serverDisplayData.Count; i++)
                {
                    if (data.SlotHints.Length > i)
                        ProcessHintDataPair(data.SlotHints[i], serverDisplayData[i]);
                }
            }
        }

        public bool Fire()
        {
            return FireAction(Anchor.Fire);
        }

        private void ProcessHintDataPair(string rh, DisplayData dd)
        {
            if (!String.IsNullOrEmpty(rh) && (dd != null))
            {
                HintsDictionary hints = new HintsDictionary();
                hints.Parse(rh);

                if (hints.ValueEquals(HintsDictionary.OfType, RenderingHint.TabBarButtonImage))
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
                else if (hints.ValueEquals(HintsDictionary.OfType, RenderingHint.TabBarButtonText))
                {
                    if (dd.DisplayType == DisplayType.String)
                        Text = (string)dd.Data;
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
