using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class MainRowView : UserControl
    {
        public MainRowView()
            => InitializeComponent();

        public static readonly RoutedEvent DetailGridNeedsTogglingEvent = EventManager.RegisterRoutedEvent(
            "DetailGridNeedsToggling", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainRowView));

        public event RoutedEventHandler DetailGridNeedsToggling
        {
            add { AddHandler(DetailGridNeedsTogglingEvent, value); }
            remove { RemoveHandler(DetailGridNeedsTogglingEvent, value); }
        }

        private void Icon_MouseDown_RaiseDetailGridNeedsToggling(object sender, MouseButtonEventArgs e)
            => RaiseEvent(new RoutedEventArgs(DetailGridNeedsTogglingEvent));
    }
}
