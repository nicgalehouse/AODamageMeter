using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using AODamageMeter.UI.ViewModels;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class DamageMeterView : Window
    {
        private readonly DamageMeterViewModel _damageMeterViewModel;

        public DamageMeterView()
        {
            InitializeComponent();
            DataContext = _damageMeterViewModel = new DamageMeterViewModel();

            if (_damageMeterViewModel.DamageMeter == null)
            {
                Loaded += (_, __) => ShowCharacterSelection();
            }
        }

        private void FileButton_Click_ShowCharacterSelection(object sender, RoutedEventArgs e)
            => ShowCharacterSelection();

        private void ShowCharacterSelection()
        {
            string previousSelectedCharacterName = Settings.Default.SelectedCharacterName;
            string previousSelectedLogFilePath = Settings.Default.SelectedLogFilePath;

            var characterSelectionView = new CharacterSelectionView(_damageMeterViewModel) { Owner = this };
            if (characterSelectionView.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(Settings.Default.SelectedCharacterName))
                    return; // In this case we said 'No character selected' above the OK button, so I guess they know.

                if (string.IsNullOrWhiteSpace(Settings.Default.SelectedLogFilePath))
                {
                    MessageBox.Show(
                        $"No log file for {Settings.Default.SelectedCharacterName} has been specified.",
                        "Log file not specified",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else if (!File.Exists(Settings.Default.SelectedLogFilePath))
                {
                    MessageBox.Show(
                        $"Log file for {Settings.Default.SelectedCharacterName} at {Settings.Default.SelectedLogFilePath} can't be found.",
                        "Log file not found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    _damageMeterViewModel.TryInitializeDamageMeter(
                        Settings.Default.SelectedCharacterName, Settings.Default.SelectedLogFilePath);
                }
            }
        }

        private void OptionsButton_Click_ShowOptions(object sender, RoutedEventArgs e)
            => new OptionsView { Owner = this }.ShowDialog();

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MainRowView_ProgressViewRequested_TryProgressView(object sender, RoutedEventArgs e)
            => _damageMeterViewModel.TryProgressView(((MainRowView)e.OriginalSource).MainRow);

        private void MainGridView_MouseRightButtonDown_TryRegressView(object sender, MouseButtonEventArgs e)
            => _damageMeterViewModel.TryRegressView();

        private void CloseButton_Click_CloseApplication(object sender, RoutedEventArgs e)
            => Close();

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.Save();
            _damageMeterViewModel.DisposeDamageMeter();

            base.OnClosing(e);
        }
    }
}
