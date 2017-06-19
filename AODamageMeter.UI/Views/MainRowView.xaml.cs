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
            LeftTextBlock.SizeChanged += (_, e) => 
            {
                if (!e.HeightChanged) return;

                Canvas.SetTop(LeftTextBlock, (24 - LeftTextBlock.ActualHeight) / 2);
                Canvas.SetTop(RightTextBlock, (24 - LeftTextBlock.ActualHeight) / 2);
            };
        }

        public static readonly RoutedEvent ProgressViewRequestedEvent = EventManager.RegisterRoutedEvent(
            "ProgressViewRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainRowView));

        public event RoutedEventHandler ProgressViewRequested
        {
            add { AddHandler(ProgressViewRequestedEvent, value); }
            remove { RemoveHandler(ProgressViewRequestedEvent, value); }
        }

        public MainRowBase MainRow => (MainRowBase)DataContext;

        private bool IsCtrlKeyDown
            => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

        private void Canvas_MouseLeftButtonDown_TryCopyAndScriptOrTryRaiseProgressViewRequested(object sender, MouseButtonEventArgs e)
        {
            if (IsCtrlKeyDown)
            {
                MainRow.TryCopyAndScriptProgressedRowsInfo();
            }
            else
            {
                RaiseEvent(new RoutedEventArgs(ProgressViewRequestedEvent));
            }
            e.Handled = true;
        }

        private void Icon_MouseLeftButtonDown_TryToggleShowDetails(object sender, MouseButtonEventArgs e)
        {
            MainRow.TryToggleShowDetails();
            e.Handled = true;
        }

        private void LeftTextBlock_MouseLeftButtonDown_CopyAndScript(object sender, MouseButtonEventArgs e)
        {
            if (!IsCtrlKeyDown) return;

            MainRow.CopyAndScriptLeftTextTooltip();
            e.Handled = true;
        }

        private void RightTextBlock_MouseLeftButtonDown_CopyAndScript(object sender, MouseButtonEventArgs e)
        {
            if (!IsCtrlKeyDown) return;

            MainRow.CopyAndScriptRightTextTooltip();
            e.Handled = true;
        }
    }
}
