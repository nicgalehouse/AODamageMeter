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
        private bool _previousShowPercentOfTotal = Settings.Default.ShowPercentOfTotal;

        public OptionsView()
        {
            InitializeComponent();
            ShowPercentOfTotalRadioButton.IsChecked = Settings.Default.ShowPercentOfTotal;
            ShowPercentOfMaxRadioButton.IsChecked = !ShowPercentOfTotalRadioButton.IsChecked;
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
                Settings.Default.ShowPercentOfTotal = _previousShowPercentOfTotal;
            }
            else
            {
                Settings.Default.Save();
            }

            base.OnClosing(e);
        }

        private void ShowPercentOfTotalRadioButton_Checked_Persist(object sender, RoutedEventArgs e)
            => Settings.Default.ShowPercentOfTotal = true;

        private void ShowPercentOfTotalRadioButton_Unchecked_Persist(object sender, RoutedEventArgs e)
            => Settings.Default.ShowPercentOfTotal = false;
    }
}
