using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Wave.Common;
using Wave.Platform;
using Wave.Platform.Messaging;
using Wave.Services;

namespace Wave.UI
{
    public abstract class BlockBase : WaveControl
    {
        public const string Hint = "hint:";
        
        // UI navigation
        public View Host { get; private set; }
        public Node ParentNode { get; private set; }
        public BlockBase ParentBlock { get; private set; }

        public bool IsRoot { get; private set; }

        // child controls
        public List<WaveControl> WaveChildren { get; private set; }
        public ImageBackground BlockBackground { get; protected set; }

        // data
        protected BlockDefinition Definition { get; private set; }
        protected FieldList Content { get; private set; }
        protected PaletteDefinition Palette { get; set; }
        protected ActionSetCollection ActionSets { get; set; }

        // visuals
        protected virtual PaintStyle? BackgroundStyle { get { return null; } }
        protected virtual PaintStyle? ForegroundStyle { get { return null; } }

        // state variables
        public bool IsChecked { get; protected set; }
        public bool IsFocused { get; protected set; }

        // state capability variables
        public virtual bool CanCheck { get { return false; } }
        public virtual bool CanFocus { get { return false; } }

        // form data
        public short FormID { get; protected set; }
        public string FormSubmissionKey { get; protected set; }
        public string FormSubmissionValue { get; set; }
        public string FormSubmissionHint { get; protected set; }

        // signpost
        public short Signpost { get; protected set; }

        // visibility
        public virtual bool IsHidden { get { return false; } }
        
        /// <summary>
        /// Value that needs to be sent to the server, can be null.
        /// </summary>
        public object FormSubmissionPayload { get; protected set; }

        // hierarchy interaction
        private bool subscribeAsBackKeyListener = false;

        public BlockBase(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base()
        {
            WaveChildren = new List<WaveControl>();
            ActionSets = new ActionSetCollection();

            IsChecked = false;
            IsFocused = false;

            FormID = -1;
            FormSubmissionKey = null;
            FormSubmissionValue = null;
            FormSubmissionHint = null;
            FormSubmissionPayload = null;

            Signpost = -1;

            Host = hostView;
            ParentNode = parentNode;
            ParentBlock = parentBlock;
            IsRoot = isRoot;

            Definition = definition;
            Content = content;

            UnpackContent();
        }

        private void UnpackContent()
        {
            if (Content != null)
            {
                /*
                m_acSpotID = (char[]) oContentFldListForBlock.getNextFldData(CFieldList.c_nSTR, CNaviAgent.c_fieldidProxyValue, null);
                m_fPersistentBlockProxy = oContentFldListForBlock.getNumFieldOfType(CFieldList.c_nINT, CNaviAgent.c_fieldidProxyPersistence) > 0?true:false;
                m_nMaxBlocksToStoreInCache = oContentFldListForBlock.getIntFldData(CNaviAgent.c_fieldidProxyHistory);
                */

                // find palette reference
                int paletteID = Content[NaviAgentFieldID.PaletteID].AsInteger() ?? 0;

                if (paletteID > 0)
                    Palette = Core.Definitions.Find(ParentNode.ApplicationID, paletteID) as PaletteDefinition;

                // unpack actionsets
                ActionSets.Load(Content, NaviAgentFieldID.ActionSet);

                // should we subscribe for back key notifications?
                subscribeAsBackKeyListener = (ActionSets[Anchor.Back] != null);

                /*
                bool fIsTopEvent = (oContentFldListForBlock.getIntFldData(CNaviAgent.c_fieldidIsTopEvent) == 1);
                if (fIsTopEvent)
                {
                    CNaviAgent.c_this.m_oBlockMgr.TopEventListener = this;
                }
                */

                // form details
                FormID = Content[NaviAgentFieldID.FormID].AsShort() ?? -1;
                FormSubmissionKey = Content[NaviAgentFieldID.SubmissionKey].AsString();
                FormSubmissionValue = Content[NaviAgentFieldID.SubmissionValue].AsString();

                if (FormSubmissionKey != null)
                {
                    /*
                    // allow block manager (bonded) to modify the value.
                    m_acSubmissionValue = CNaviAgent.c_this.m_oBlockMgr.checkModifySubmissionValue(m_acSubmissionKey, m_acSubmissionValue);
                    */
                }

                if ((FormSubmissionValue != null) && FormSubmissionValue.StartsWith(Hint))
                {
                    FormSubmissionHint = FormSubmissionValue.Substring(Hint.Length, FormSubmissionValue.Length - Hint.Length);
                    FormSubmissionValue = null;
                }

                // unpacking block signpost
                Signpost = Content[NaviAgentFieldID.Signpost].AsShort() ?? FieldList.FieldNotFound;

                if ((ParentNode != null) && (Signpost != FieldList.FieldNotFound))
                    ParentNode.RegisterBlockSignpost(this);

                // unpacking background image
                FieldList bgData = Content[NaviAgentFieldID.BackgroundImage] as FieldList;

                if (bgData != null)
                {
                    DisplayDataCollection ddc = DisplayData.Parse(bgData);

                    if ((ddc != null) && (ddc.Count > 0))
                    {
                        BlockBackground = new ImageBackground(ddc[0]);
                        Core.UI.ReferencedContent.Add(ddc[0]);
                        
                        Children.Add(BlockBackground);
                    }
                }
            }
        }

        #region Children

        public virtual void AddChildBlock(BlockBase childBlock)
        {
            WaveChildren.Add(childBlock);

            AddSynchronisedBlock(childBlock);
        }

        protected virtual void AddSynchronisedBlock(BlockBase childBlock)
        {
            Children.Add(childBlock);
        }

        #endregion

        #region Visuals management

        public PaintStyleResult ResolvePaintStyle(PaintStyle ps)
        {
            PaletteEntryBase pRef = null;

            // resolve palette reference if needed
            if (ps.StyleType == PaletteEntryType.PaletteReference)
                pRef = FindPaintStyleTarget(ps);

            return PreparePaintStyleResults(ref ps, pRef);
        }

        public static PaintStyleResult PreparePaintStyleResults(ref PaintStyle ps, PaletteEntryBase pRef)
        {
            PaintStyleResult.Result resType = PaintStyleResult.Result.NotFound;
            Brush br = null;

            switch (ps.StyleType)
            {
                case PaletteEntryType.DoNotPaint:
                    resType = PaintStyleResult.Result.Transparent;
                    br = new SolidColorBrush(Colors.Transparent);
                    break;

                case PaletteEntryType.Colour:
                    if (ps.Data is int)
                    {
                        resType = PaintStyleResult.Result.Colour;
                        br = new SolidColorBrush(DataHelper.IntToColour((int)ps.Data));
                        break;
                    }
                    else
                        goto case PaletteEntryType.DoNotPaint;

                case PaletteEntryType.PaletteReference:
                    if ((pRef != null) &&
                        ((pRef.EntryType != PaletteEntryType.DoNotPaint) && (pRef.EntryType != PaletteEntryType.Inherited)))
                    {
                        switch (pRef.EntryType)
                        {
                            case PaletteEntryType.Colour:
                                {
                                    ColourPaletteEntry cl = pRef as ColourPaletteEntry;

                                    if (cl != null)
                                    {
                                        resType = PaintStyleResult.Result.Colour;
                                        br = cl.ToBrush();
                                        break;
                                    }

                                    goto default;
                                }

                            case PaletteEntryType.LinearGradient:
                                {
                                    LinearGradientPaletteEntry lg = pRef as LinearGradientPaletteEntry;

                                    if (lg != null)
                                    {
                                        resType = PaintStyleResult.Result.LinearGradient;
                                        br = lg.ToBrush();
                                        break;
                                    }

                                    goto default;
                                }

                            //IMPLEMENT: other palette entry types

                            default:
                                resType = PaintStyleResult.Result.Transparent;
                                br = new SolidColorBrush(Colors.Transparent);
                                break;
                        }

                        break;
                    }
                    else
                        goto case PaletteEntryType.DoNotPaint;
            }

            return new PaintStyleResult(resType, br);
        }

        #endregion

        #region Palette and paint style search

        public PaletteEntryBase FindCascadedPaletteItem(int index)
        {
            PaletteEntryBase res = null;

            if (Palette != null)
                res = Palette[index];

            if ((res == null) && (ParentBlock != null))
                return ParentBlock.FindCascadedPaletteItem(index);

            if ((res == null) && (ParentBlock == null) && IsRoot && (Host.ParentFrame != null))
                return Host.ParentFrame.FindCascadedPaletteItem(index);

            if ((res == null) && (ParentBlock == null) && (ParentNode != null) && (ParentNode.ApplicationPalette != null))
                res = ParentNode.ApplicationPalette[index];

            return res;
        }

        public PaintStyle? FindPaintStyleByUsage(PaintStyleUse pst)
        {
            PaintStyle? temp = null;

            switch (pst)
            {
                case PaintStyleUse.Background:
                    temp = BackgroundStyle;
                    break;

                case PaintStyleUse.Foreground:
                    temp = ForegroundStyle;
                    break;
            }

            return temp;
        }

        public PaletteEntryBase FindPaintStyleTarget(PaintStyle ps)
        {
            PaletteEntryBase res = null;

            if ((ps.StyleType == PaletteEntryType.PaletteReference) && (ps.Data is int))
            {
                int entryIndex = (int)ps.Data;
                
                do
                    res = FindCascadedPaletteItem(entryIndex) as PaletteEntryBase;
                while
                    ((res != null) && (res.EntryType == PaletteEntryType.PaletteReference));
            }

            return res;
        }

        #endregion

        #region Focus and check management

        public virtual void SetState(bool isFocused, bool isChecked)
        {
        }

        public virtual void SignalTap(BlockBase block)
        {
        }

        public virtual void SignalTapAndHold(BlockBase block)
        {
        }

        public virtual void SignalFocus(BlockBase block)
        {
        }

        #endregion

        #region Background image support

        protected void MeasureBackground()
        {
            if (BlockBackground != null)
                BlockBackground.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
        }

        protected void ArrangeBackground(Size finalSize)
        {
            if (BlockBackground != null)
                BlockBackground.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        }

        #endregion

        #region Forms

        public virtual void AttachFormData(short formID, FieldList storage)
        {
            if ((storage != null) && (formID == FormID) && !String.IsNullOrEmpty(FormSubmissionKey))
            {
                storage.AddString(NaviAgentFieldID.SubmissionKey, FormSubmissionKey);

                if (FormSubmissionPayload != null)
                {
                    if (FormSubmissionPayload is FieldList)
                        storage.Add((FieldList)FormSubmissionPayload);
                }
                else
                {
                    string fsv = ProcessSubmissionValue();

                    if (fsv != null)
                        storage.AddString(NaviAgentFieldID.SubmissionValue, fsv);
                }
            }
        }

        public virtual string ProcessSubmissionValue()
        {
            if (FormSubmissionValue == null)
                return String.Empty;
            else
                return FormSubmissionValue;
        }

        #endregion

        #region Notifications

        public virtual void SignalShow()
        {
            if (subscribeAsBackKeyListener)
                Host.RegisterBackKeyListener(this);
        }

        public virtual void SignalHide()
        {
            if (subscribeAsBackKeyListener)
                Host.UnregisterBackKeyListener(this);
        }

        public virtual void SignalDestroy()
        {
            if (WaveChildren.Count > 0)
            {
                foreach (WaveControl child in WaveChildren)
                {
                    if (child is BlockBase)
                        ((BlockBase)child).SignalDestroy();
                }
            }
        }

        public virtual void SignalIsRoot()
        {
        }

        #endregion

        #region Actions support

        public bool HasAnchor(Anchor anchor)
        {
            return (ActionSets[anchor] != null);
        }

        public bool HasAction(Anchor anchor, ActionType actionType)
        {
            ActionSet test = ActionSets[anchor];

            if (test != null)
                return test.HasAction(actionType);
            
            return false;
        }

        public bool HasCustomAction(Anchor anchor, string customActionType)
        {
            ActionSet test = ActionSets[anchor];

            if (test != null)
                return test.HasCustomAction(customActionType);

            return false;
        }

        public bool FireAction(Anchor anchor)
        {
            ActionSet test = ActionSets[anchor];

            if (test != null)
            {
                Core.Navigation.ExecuteAction(test, this, ParentNode, ParentNode.ViewID);

                return true;
            }

            return false;
        }

        public void AddAction(Anchor anchor, ActionBase action)
        {
            // check for existing action set
            ActionSet loadedActions = ActionSets[anchor];

            // create action set if it is not there
            if (loadedActions == null)
            {
                loadedActions = new ActionSet();
                loadedActions.Anchors.Add(anchor);
                
                ActionSets.Add(loadedActions);
            }

            if (loadedActions != null)
                loadedActions.Actions.Add(action);
        }

        #endregion
    }

    #region Paint style search results

    public class PaintStyleResult
    {
        public Result Context { get; private set; }
        public Brush Brush { get; private set; }

        public PaintStyleResult(Result c, Brush br)
        {
            Context = c;
            Brush = br;
        }

        public enum Result
        {
            Transparent,
            Colour,
            ColourSequence,
            LinearGradient,
            NotFound
        }
    }

    public enum PaintStyleUse
    {
        Background,
        Foreground
    }

    #endregion
}
