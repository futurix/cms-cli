using System.Windows;

namespace Wave.Common
{
    public struct Spacing
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }

        public Spacing(int left, int top, int right, int bottom)
            : this()
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public static implicit operator Spacing(Thickness arg)
        {
            return new Spacing() { Left = (int)arg.Left, Top = (int)arg.Top, Right = (int)arg.Right, Bottom = (int)arg.Bottom };
        }

        public static implicit operator Thickness(Spacing arg)
        {
            return new Thickness(arg.Left, arg.Top, arg.Right, arg.Bottom);
        }
    }
}
