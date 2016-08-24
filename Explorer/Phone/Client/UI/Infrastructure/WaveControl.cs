using System.Windows.Controls;
using Wave.Common;

namespace Wave.UI
{
    public abstract class WaveControl : Panel
    {
        public new Spacing Margin
        {
            get { return base.Margin; }
            set { base.Margin = value; }
        }

        public Spacing Padding { get; set; }

        public WaveControl()
            : base()
        {
        }
    }
}
