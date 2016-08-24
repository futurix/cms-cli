using System.Windows;

namespace Wave.UI
{
    public static class TableLayout
    {
        public static readonly DependencyProperty SlotPositionProperty = 
            DependencyProperty.RegisterAttached("SlotPosition", typeof(TableLayoutPosition), typeof(TableLayout), new PropertyMetadata(TableLayoutPosition.Default));

        public static void SetSlotPosition(UIElement element, TableLayoutPosition value)
        {
            element.SetValue(SlotPositionProperty, value);
        }

        public static TableLayoutPosition GetSlotPosition(UIElement element)
        {
            return (TableLayoutPosition)element.GetValue(SlotPositionProperty);
        }
    }

    public struct TableLayoutPosition
    {
        public static TableLayoutPosition Default = new TableLayoutPosition() { Column = 0, Row = 0, ColumnSpan = 0, RowSpan = 0 };
        
        public int Column { get; set; }
        public int Row { get; set; }

        public int ColumnSpan { get; set; }
        public int RowSpan { get; set; }
    }
}
