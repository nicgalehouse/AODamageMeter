using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AODamageMeter.UI.Behaviors
{
    public static class GridViewColumnMinWidthBehavior
    {
        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.RegisterAttached(
                "MinWidth",
                typeof(double),
                typeof(GridViewColumnMinWidthBehavior),
                new PropertyMetadata(0.0, OnMinWidthChanged));

        public static double GetMinWidth(GridViewColumn column)
            => (double)column.GetValue(MinWidthProperty);

        public static void SetMinWidth(GridViewColumn column, double value)
            => column.SetValue(MinWidthProperty, value);

        private static void OnMinWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridViewColumn column)
            {
                var dpd = DependencyPropertyDescriptor.FromProperty(
                    GridViewColumn.WidthProperty, typeof(GridViewColumn));

                dpd.RemoveValueChanged(column, OnColumnWidthChanged);
                dpd.AddValueChanged(column, OnColumnWidthChanged);
            }
        }

        private static void OnColumnWidthChanged(object sender, System.EventArgs e)
        {
            if (sender is GridViewColumn column)
            {
                double minWidth = GetMinWidth(column);

                if (column.Width < minWidth)
                {
                    column.Width = minWidth;
                }
            }
        }
    }
}
