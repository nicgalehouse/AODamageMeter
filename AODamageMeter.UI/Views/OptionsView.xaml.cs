using AODamageMeter.UI.Properties;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class OptionsView : Window
    {
        private string _previousFontFamily;
        private double _previousFontSize;

        public OptionsView()
        {
            InitializeComponent();
            _previousFontFamily = Settings.Default.FontFamily;
            _previousFontSize = Settings.Default.FontSize;
        }

        private void OKButton_Click_CloseDialog(object sender, RoutedEventArgs e)
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
                Settings.Default.FontFamily = _previousFontFamily;
                Settings.Default.FontSize = _previousFontSize;
            }

            base.OnClosing(e);
        }
    }
}
