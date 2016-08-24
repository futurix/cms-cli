using System.Collections.Generic;
using Wave.Platform;

namespace Wave.UI
{
    public class SignpostedDataRegistrar
    {
        private Dictionary<short, DisplayData> data = new Dictionary<short, DisplayData>();

        public DisplayData this[short signpost]
        {
            get
            {
                DisplayData res = null;

                data.TryGetValue(signpost, out res);
                
                return res;
            }
        }

        public void Add(DisplayDataCollection slots)
        {
            if ((slots == null) || (slots.Count == 0))
                return;

            foreach (DisplayData dd in slots)
                Add(dd);
        }

        public void Add(DisplayData dd)
        {
            if (dd == null)
                return;

            if (dd.Signpost.HasValue && (dd.Signpost.Value >= 0))
                data[dd.Signpost.Value] = dd;
        }

        public void Remove(DisplayDataCollection slots)
        {
            if ((slots == null) || (slots.Count == 0))
                return;

            foreach (DisplayData dd in slots)
            {
                if (dd.Signpost.HasValue && (dd.Signpost.Value >= 0) && data.ContainsKey(dd.Signpost.Value))
                    data.Remove(dd.Signpost.Value);
            }
        }

        public void Remove(DisplayData dd)
        {
            if (dd == null)
                return;

            if (dd.Signpost.HasValue && (dd.Signpost.Value >= 0) && data.ContainsKey(dd.Signpost.Value))
                data.Remove(dd.Signpost.Value);
        }

        public void Reset()
        {
            data.Clear();
        }
    }
}
