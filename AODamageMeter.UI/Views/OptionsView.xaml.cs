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
        private int _previousRefreshInterval = Settings.Default.RefreshInterval;
        private bool _previousShowPercentOfTotal = Settings.Default.ShowPercentOfTotal;
        private bool _previousShowRowNumbers = Settings.Default.ShowRowNumbers;
        private bool _previousIncludeTopLevelNPCRows = Settings.Default.IncludeTopLevelNPCRows;
        private bool _previousIncludeTopLevelZeroDamageRows = Settings.Default.IncludeTopLevelZeroDamageRows;

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

        private void ShowPercentOfTotalRadioButton_Checked_Persist(object sender, RoutedEventArgs e)
            => Settings.Default.ShowPercentOfTotal = true;

        private void ShowPercentOfTotalRadioButton_Unchecked_Persist(object sender, RoutedEventArgs e)
            => Settings.Default.ShowPercentOfTotal = false;

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult != true)
            {
                Settings.Default.FontFamily = _previousFontFamily;
                Settings.Default.FontSize = _previousFontSize;
                Settings.Default.RefreshInterval = _previousRefreshInterval;
                Settings.Default.ShowPercentOfTotal = _previousShowPercentOfTotal;
                Settings.Default.ShowRowNumbers = _previousShowRowNumbers;
                Settings.Default.IncludeTopLevelNPCRows = _previousIncludeTopLevelNPCRows;
                Settings.Default.IncludeTopLevelZeroDamageRows = _previousIncludeTopLevelZeroDamageRows;
            }
            else
            {
                Settings.Default.Save();
            }

            base.OnClosing(e);
        }
    }
}
