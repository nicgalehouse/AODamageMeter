using AODamageMeter.UI.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

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
    }
}
