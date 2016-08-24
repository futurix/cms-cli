using System;
using System.Collections.Generic;
using System.IO;
using Wave.Common;
using Wave.Services;

namespace Wave.Platform
{
    public class HintsDictionary : Dictionary<string, string>, ICacheable
    {
        public const string Target = "target";
        public const string Version = "version";
        public const string Version_1_0 = "1.0";
        public const string Version_1_1 = "1.1";
        public const string OfType = "type";
        
        public UIHintedType HintedType { get; private set; }

        public HintsDictionary()
            : base()
        {
            HintedType = UIHintedType.None;
        }

        #region Utility search methods

        public new string this[string key]
        {
            get
            {
                string res = null;

                if (TryGetValue(key, out res))
                    return res;
                else
                    return String.Empty;
            }
            set
            {
                base[key] = value;
            }
        }

        public bool HasValue(string key)
        {
            return ContainsKey(key);
        }
        
        public bool ValueEquals(string key, string newValue)
        {
            string temp = null;
            
            if (TryGetValue(key, out temp) && (temp == newValue))
                return true;

            return false;
        }

        #endregion

        #region Hinted types management

        private void ParseHintedType(string typeString)
        {
            switch (typeString)
            {
                case RenderingHint.ApplicationBar:
                    HintedType = UIHintedType.ApplicationBar;
                    break;

                case RenderingHint.ApplicationBarOptions:
                    HintedType = UIHintedType.ApplicationBarOptions;
                    break;

                case RenderingHint.ApplicationBarButton:
                    HintedType = UIHintedType.ApplicationBarButton;
                    break;

                case RenderingHint.ApplicationBarMenuItem:
                    HintedType = UIHintedType.ApplicationBarMenuItem;
                    break;

                case RenderingHint.NativeMessageBox:
                    HintedType = UIHintedType.NativeMessageBox;
                    break;

                case RenderingHint.NativeMessageBoxBody:
                    HintedType = UIHintedType.NativeMessageBoxBody;
                    break;

                case RenderingHint.NativeMessageBoxButton1:
                case RenderingHint.NativeMessageBoxButton2:
                case RenderingHint.NativeMessageBoxButton3:
                    HintedType = UIHintedType.NativeMessageBoxButton;    
                    break;

                case RenderingHint.TabBar:
                    HintedType = UIHintedType.TabBar;
                    break;

                case RenderingHint.TabBarButton:
                    HintedType = UIHintedType.TabBarButton;
                    break;

                case RenderingHint.ActionSheet:
                    HintedType = UIHintedType.ActionSheet;
                    break;

                case RenderingHint.ActionSheetTitle:
                    HintedType = UIHintedType.ActionSheetTitle;
                    break;

                case RenderingHint.ActionSheetToggle:
                    HintedType = UIHintedType.ActionSheetToggle;
                    break;

                case RenderingHint.Pivot:
                    HintedType = UIHintedType.Pivot;
                    break;

                case RenderingHint.PivotTitle:
                    HintedType = UIHintedType.PivotTitle;
                    break;

                case RenderingHint.Panorama:
                    HintedType = UIHintedType.Panorama;
                    break;

                case RenderingHint.PanoramaTitle:
                    HintedType = UIHintedType.PanoramaTitle;
                    break;

                case RenderingHint.SystemTray:
                    HintedType = UIHintedType.SystemTray;
                    break;

                case RenderingHint.Placeholder:
                    HintedType = UIHintedType.Placeholder;
                    break;
            }
        }

        #endregion

        #region Parser

        public void Parse(string input)
        {
            if (!String.IsNullOrEmpty(input))
            {
                try
                {
                    List<string> tags = StringHelper.FindEnclosures(input, '[', ']');

                    foreach (string tag in tags)
                    {
                        string ver = StringHelper.Until(tag, ';');

                        if (VerifyVersion(ver))
                        {
                            List<string> containers = StringHelper.FindEnclosures(tag, '{', '}');

                            foreach (string container in containers)
                            {
                                List<Pair<string, string>> temp = StringHelper.FindPairs(container, ';', '=');
                                bool isForWin = false;

                                foreach (Pair<string, string> pair in temp)
                                {
                                    if ((pair.First == Target) && VerifyTarget(pair.Second))
                                    {
                                        isForWin = true;
                                        break;
                                    }
                                }

                                if (isForWin)
                                {
                                    foreach (Pair<string, string> pair in temp)
                                    {
                                        if (pair.First != Target)
                                        {
                                            this[pair.First] = pair.Second;

                                            if (pair.First == OfType)
                                                ParseHintedType(pair.Second);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Clear();

                    DebugHelper.Out("Failed to parse rendering hints:");
                    DebugHelper.Out(input);
                }
            }
        }

        private bool VerifyVersion(string version)
        {
            string[] parts = version.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 1)
            {
                string verLeft = parts[0].Trim();
                string verRight = parts[1].Trim();

                return ((verLeft == Version) && ((verRight == Version_1_0) || (verRight == Version_1_1)));
            }

            return false;
        }

        private bool VerifyTarget(string tgt)
        {
            string[] targets = tgt.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string target in targets)
            {
                if (target == RenderingHintTarget.EveryDevice)
                    return true;

                if (target == RenderingHintTarget.EveryDeviceFamily)
                    return true;

                if (target == RenderingHintTarget.Windows)
                    return true;

                if (target == RenderingHintTarget.WindowsPhone)
                    return true;
            }

            return false;
        }

        private static class RenderingHintTarget
        {
            public const string EveryDevice = "All";
            public const string EveryDeviceFamily = "DeviceFamily.All";

            public const string Windows = "DeviceFamily.Windows";
            public const string WindowsPhone = "DeviceFamily.Windows.Phone";
            public const string WindowsTablet = "DeviceFamily.Windows.Tablet";
        }

        #endregion

        #region ICacheable implementation

        public CacheableType StoredType
        {
            get { return CacheableType.Unsupported; }
        }

        public void Persist(Stream str)
        {
            str.WriteByte(0);
            str.WriteShort((short)Count);
            str.WriteInteger((int)HintedType);

            foreach (var pair in this)
            {
                BinaryHelper.WriteString(str, pair.Key);
                BinaryHelper.WriteString(str, pair.Value);
            }
        }

        public void Restore(Stream str)
        {
            if (str.ReadByte() == 0)
            {
                short numberOfItems = str.ReadShort();
                HintedType = (UIHintedType)str.ReadInteger();

                if (numberOfItems > 0)
                {
                    for (int i = 0; i < numberOfItems; i++)
                    {
                        string key = BinaryHelper.ReadString(str);
                        string value = BinaryHelper.ReadString(str);

                        if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(value))
                            Add(key, value);
                    }
                }
            }
        }

        #endregion
    }

    public enum UIHintedType : int
    {
        None = 0,

        ApplicationBar,
        ApplicationBarOptions,
        ApplicationBarButton,
        ApplicationBarMenuItem,

        NativeMessageBox,
        NativeMessageBoxBody,
        NativeMessageBoxButton,

        TabBar,
        TabBarButton,

        ActionSheet,
        ActionSheetTitle,
        ActionSheetToggle,

        Pivot,
        PivotTitle,

        Panorama,
        PanoramaTitle,

        SystemTray,

        Placeholder
    }

    public static class RenderingHint
    {
        public const string ApplicationBar = "ApplicationBar";
        public const string ApplicationBarOptions = "ApplicationBar.Options";
        public const string ApplicationBarButton = "ApplicationBar.Button";
        public const string ApplicationBarButtonText = "ApplicationBar.Button.Text";
        public const string ApplicationBarButtonIcon = "ApplicationBar.Button.Icon";
        public const string ApplicationBarMenuItem = "ApplicationBar.MenuItem";
        public const string ApplicationBarMenuItemText = "ApplicationBar.MenuItem.Text";

        public const string NativeMessageBox = "AlertMsgContainer";
        public const string NativeMessageBoxBody = "AlertMsgBlockBody";
        public const string NativeMessageBoxTitle = "AlertMsgBlockBody.Title";
        public const string NativeMessageBoxMessage = "AlertMsgBlockBody.Message";
        public const string NativeMessageBoxButton1 = "AlertMsgBlockButtonNegative";
        public const string NativeMessageBoxButton2 = "AlertMsgBlockButtonPositive";
        public const string NativeMessageBoxButton3 = "AlertMsgBlockButtonNeutral";

        public const string TabBar = "TabBar";
        public const string TabBarButton = "TabBar.Button";
        public const string TabBarButtonImage = "TabBar.Button.Image";
        public const string TabBarButtonText = "TabBar.Button.Text";

        public const string ActionSheet = "ActionSheetCtrl";
        public const string ActionSheetTitle = "ActionSheetCtrl.TitleBlock";
        public const string ActionSheetToggle = "ShowActionSheetButton";

        public const string Pivot = "Pivot";
        public const string PivotTitle = "Pivot.Title";

        public const string Panorama = "Panorama";
        public const string PanoramaTitle = "Panorama.Title";

        public const string SystemTray = "SystemTray";

        public const string Placeholder = "Placeholder";
    }
}
