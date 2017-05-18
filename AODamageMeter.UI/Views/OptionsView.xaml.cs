using AODamageMeter.UI.Properties;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class OptionsView : Window
    {
        private string _previousFontFamily = Settings.Default.FontFamily;
        private double _previousFontSize = Settings.Default.FontSize;
        private bool _previousShowPercentOfTotalDamageDone = Settings.Default.ShowPercentOfTotalDamageDone;

        public OptionsView()
        {
            InitializeComponent();
            ShowPercentOfTotalDamageDoneRadioButton.IsChecked = Settings.Default.ShowPercentOfTotalDamageDone;
            ShowPercentOfMaxDamageDoneRadioButton.IsChecked = !ShowPercentOfTotalDamageDoneRadioButton.IsChecked;
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
                Settings.Default.ShowPercentOfTotalDamageDone = _previousShowPercentOfTotalDamageDone;
            }
            else
            {
                Settings.Default.Save();
            }

            base.OnClosing(e);
        }

        private void ShowPercentOfTotalDamageDoneRadioButton_Checked_Persist(object sender, RoutedEventArgs e)
            => Settings.Default.ShowPercentOfTotalDamageDone = true;

        private void ShowPercentOfTotalDamageDoneRadioButton_Unchecked_Persist(object sender, RoutedEventArgs e)
            => Settings.Default.ShowPercentOfTotalDamageDone = false;
    }
}
