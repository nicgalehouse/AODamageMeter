using AODamageMeter.UI.Properties;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class AutoConfigureView : Window
    {
        private readonly bool _previousIncludeSystemChannel = Settings.Default.IncludeSystemChannel;

        public AutoConfigureView()
            => InitializeComponent();

        private void GoButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = true;

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult != true)
            {
                Settings.Default.IncludeSystemChannel = _previousIncludeSystemChannel;
            }

            base.OnClosing(e);
        }
    }
}
