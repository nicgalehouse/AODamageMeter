using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class MainRowView : UserControl
    {
        public MainRowView()
        {
            InitializeComponent();

            // Matters when font properties change. These two text blocks share the same height, and name's
            // text block has no width-only false positives, so pivot off of its event.
            NameTextBlock.SizeChanged += (_, e) => 
            {
                if (!e.HeightChanged) return;

                Canvas.SetTop(NameTextBlock, (24 - NameTextBlock.ActualHeight) / 2);
                Canvas.SetTop(RightTextBlock, (24 - NameTextBlock.ActualHeight) / 2);
            };
        }

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
