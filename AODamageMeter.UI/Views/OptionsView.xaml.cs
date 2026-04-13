using AODamageMeter.UI.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class OptionsView : Window
    {
        private readonly string _previousBossModule = Settings.Default.BossModule;
        private readonly string _previousFontFamily = Settings.Default.FontFamily;
        private readonly double _previousFontSize = Settings.Default.FontSize;
        private readonly int _previousRefreshInterval = Settings.Default.RefreshInterval;
        private readonly int _previousMaxNumberOfDetailRows = Settings.Default.MaxNumberOfDetailRows;
        private readonly bool _previousShowPercentOfTotal = Settings.Default.ShowPercentOfTotal;
        private readonly bool _previousShowRowNumbers = Settings.Default.ShowRowNumbers;
        private readonly bool _previousIncludeTopLevelNPCRows = Settings.Default.IncludeTopLevelNPCRows;
        private readonly bool _previousIncludeTopLevelZeroDamageRows = Settings.Default.IncludeTopLevelZeroDamageRows;
        private readonly string _previousTheBeastDualLoggedCharacter = Settings.Default.TheBeastDualLoggedCharacter;

        public OptionsView()
        {
            InitializeComponent();
            ShowPercentOfTotalRadioButton.IsChecked = Settings.Default.ShowPercentOfTotal;
            ShowPercentOfMaxRadioButton.IsChecked = !ShowPercentOfTotalRadioButton.IsChecked;

            BossModuleComboBox.SelectionChanged += (_, __) => UpdateDualLoggedCharacterVisibility();
            DualLoggedCharacterComboBox.SelectionChanged += (_, __) => UpdateOKButton();
            UpdateDualLoggedCharacterVisibility();
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

        private void UpdateDualLoggedCharacterVisibility()
        {
            bool isTheBeastDualLoggedSelected = (string)BossModuleComboBox.SelectedItem == "The Beast (dual-logged)";
            var visibility = isTheBeastDualLoggedSelected ? Visibility.Visible : Visibility.Collapsed;
            DualLoggedCharacterLabel.Visibility = visibility;
            DualLoggedCharacterComboBox.Visibility = visibility;

            if (isTheBeastDualLoggedSelected)
            {
                PopulateDualLoggedCharacterComboBox();
            }

            UpdateOKButton();
        }

        private void UpdateOKButton()
        {
            bool isTheBeastDualLoggedSelected = (string)BossModuleComboBox.SelectedItem == "The Beast (dual-logged)";
            OKButton.IsEnabled = !isTheBeastDualLoggedSelected || DualLoggedCharacterComboBox.SelectedItem != null;
        }

        private void PopulateDualLoggedCharacterComboBox()
        {
            var characterNames = Settings.Default.CharacterNames.Cast<string>().ToArray();
            var dimensions = Settings.Default.Dimensions.Cast<string>().ToArray();
            var logFilePaths = Settings.Default.LogFilePaths.Cast<string>().ToArray();

            var dualLoggableCharacters = new List<string>();
            for (int i = 0; i < characterNames.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(logFilePaths[i]))
                    continue;

                if (characterNames[i] == Settings.Default.SelectedCharacterName)
                    continue;

                var dimension = DimensionHelpers.GetDimensionOrDefault(dimensions[i]);
                dualLoggableCharacters.Add($"{characterNames[i]} ({dimension.GetName()})");
            }

            DualLoggedCharacterComboBox.ItemsSource = dualLoggableCharacters;

            if (!dualLoggableCharacters.Contains(Settings.Default.TheBeastDualLoggedCharacter))
            {
                Settings.Default.TheBeastDualLoggedCharacter = "";
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult != true)
            {
                Settings.Default.BossModule = _previousBossModule;
                Settings.Default.TheBeastDualLoggedCharacter = _previousTheBeastDualLoggedCharacter;
                Settings.Default.FontFamily = _previousFontFamily;
                Settings.Default.FontSize = _previousFontSize;
                Settings.Default.RefreshInterval = _previousRefreshInterval;
                Settings.Default.MaxNumberOfDetailRows = _previousMaxNumberOfDetailRows;
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
