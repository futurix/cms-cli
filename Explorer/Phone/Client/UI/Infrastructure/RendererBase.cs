using System;
using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.UI
{
    public abstract class RendererBase : WaveControl
    {
        public CropStrategy Crop { get; protected set; }
        
        protected AtomicBlockBase Atomic = null;

        public RendererBase(AtomicBlockBase atomic)
            : base()
        {
            Atomic = atomic;
        }

        public void SetDisplayData(DisplayData data)
        {
            // only signposted or content reference slots can be updated
            if ((data != null) && ((data.Signpost.HasValue && (data.Signpost >= 0)) || 
                (data.DisplayType == DisplayType.ContentReference) || (data.DisplayType == DisplayType.MediaMetaData)))
                data.Updated += new EventHandler(data_Updated);

            OnDataInitialised(data);
        }

        public virtual void ApplyFormattingAndLayout(SlotData slotData, SlotStyleData slotStyleData, TableLayoutItemInfo layout)
        {
        }

        public PaintStyleResult ResolvePaintStyle(PaintStyle ps, PaintStyleUse pst = PaintStyleUse.Background)
        {
            PaintStyle? psTemp = null;
            PaletteEntryBase pRef = null;
            
            // check for inherited data
            if (ps.StyleType == PaletteEntryType.Inherited)
                psTemp = Atomic.FindPaintStyleByUsage(pst);
            else
                psTemp = ps;

            if (psTemp.HasValue)
            {
                PaintStyle resolved = psTemp.Value;
                
                // resolve palette reference if needed
                if (resolved.StyleType == PaletteEntryType.PaletteReference)
                    pRef = Atomic.FindPaintStyleTarget(resolved);

                return BlockBase.PreparePaintStyleResults(ref resolved, pRef);
            }
            else
                return new PaintStyleResult(PaintStyleResult.Result.NotFound, null);
        }

        protected virtual void OnDataInitialised(DisplayData data)
        {
        }

        protected virtual void OnDataUpdated(DisplayData data)
        {
        }

        private void data_Updated(object sender, EventArgs e)
        {
            OnDataUpdated(sender as DisplayData);
        }
    }
}
