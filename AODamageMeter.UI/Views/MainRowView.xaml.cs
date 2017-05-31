using AODamageMeter.UI.ViewModels;
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

        private void Icon_MouseLeftButtonDown_TryTogglingShowDetails(object sender, MouseButtonEventArgs e)
            => (DataContext as MainRowViewModelBase).TryTogglingShowDetails();

        public static readonly RoutedEvent ViewProgressionRequestedEvent = EventManager.RegisterRoutedEvent(
            "ViewProgressionRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainRowView));

        public event RoutedEventHandler ViewProgressionRequested
        {
            add { AddHandler(ViewProgressionRequestedEvent, value); }
            remove { RemoveHandler(ViewProgressionRequestedEvent, value); }
        }

        private void Canvas_MouseLeftButtonDown_TryRaisingViewProgressionRequested(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image)
                return; // Icon was clicked, which is for toggling the detail grid.

            RaiseEvent(new RoutedEventArgs(ViewProgressionRequestedEvent));
        }
    }
}
