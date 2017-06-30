using AODamageMeter.UI.ViewModels;
using AODamageMeter.UI.ViewModels.Rows;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AODamageMeter.UI.Views
{
    public partial class DetailRowView : UserControl
    {
        public DetailRowView()
        {
            InitializeComponent();

            // Matters when font properties change. These two text blocks share the same height, and name's
            // text block has no width-only false positives, so pivot off of its event.
            LeftTextBlock.SizeChanged += (_, e) =>
            {
                if (!e.HeightChanged) return;

                Canvas.SetTop(LeftTextBlock, (18 - LeftTextBlock.ActualHeight) / 2);
                Canvas.SetTop(RightTextBlock, (18 - LeftTextBlock.ActualHeight) / 2);
            };

            // Annoying flash of the right text over the left side of the row happens when quickly changing between
            // views w/ detail rows expanded. This slightly delays making the right text visible to prevent the flash.
            // I guess it leads to a much less noticeable flash itself, but there's already a little flash regardless.
            // Can we solve directly/differently? Maybe z-index but then would have to put something on the far left...
            // Code based on: https://stackoverflow.com/questions/19684191/rendering-not-finished-in-loaded-event.
            RightTextBlock.Loaded += (_, __) =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => RightTextBlock.Opacity = 1));
            };
        }

        public static readonly RoutedEvent DeregisterFightPetRequestedEvent = EventManager.RegisterRoutedEvent(
            "DeregisterFightPetRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DetailRowView));

        public event RoutedEventHandler DeregisterFightPetRequested
        {
            add { AddHandler(DeregisterFightPetRequestedEvent, value); }
            remove { RemoveHandler(DeregisterFightPetRequestedEvent, value); }
        }

        public DetailRowBase DetailRow => (DetailRowBase)DataContext;

        private bool IsCtrlKeyDown
            => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

        private void Canvas_MouseLeftButtonDown_TryCopyAndScript(object sender, MouseButtonEventArgs e)
        {
            if (!IsCtrlKeyDown) return;

            DetailRow.TryCopyAndScriptProgressedRowsInfo();
            e.Handled = true;
        }

        private void LeftTextBlock_MouseLeftButtonDown_CopyAndScript(object sender, MouseButtonEventArgs e)
        {
            if (!IsCtrlKeyDown) return;

            DetailRow.CopyAndScriptLeftTextTooltip();
            e.Handled = true;
        }

        private void RightTextBlock_MouseLeftButtonDown_CopyAndScript(object sender, MouseButtonEventArgs e)
        {
            if (!IsCtrlKeyDown) return;

            DetailRow.CopyAndScriptRightTextTooltip();
            e.Handled = true;
        }

        private void Canvas_RightMouseButtonDown_TryRaiseDeregisterFightPetRequested(object sender, MouseButtonEventArgs e)
        {
            if (!IsCtrlKeyDown || !(DetailRow is DamageDoneDetailRow)) return;

            RaiseEvent(new RoutedEventArgs(DeregisterFightPetRequestedEvent));
            e.Handled = true;
        }
    }
}
