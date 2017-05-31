using AODamageMeter.UI.Properties;
using AODamageMeter.UI.ViewModels;
using System.ComponentModel;
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
        }

        private void FileButton_Click_ShowCharacterSelection(object sender, RoutedEventArgs e)
        {
            string previousSelectedCharacterName = Settings.Default.SelectedCharacterName;
            string previousSelectedLogFilePath = Settings.Default.SelectedLogFilePath;

            var characterSelectionView = new CharacterSelectionView();
            if (characterSelectionView.ShowDialog() == true)
            {
                _damageMeterViewModel.TryInitializingDamageMeter(
                    Settings.Default.SelectedCharacterName, Settings.Default.SelectedLogFilePath);
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

        private void MainRowView_ViewProgressionRequested_TryProgressingView(object sender, RoutedEventArgs e)
        {
            var mainRow = (e.OriginalSource as MainRowView).DataContext as MainRowViewModelBase;
            _damageMeterViewModel.TryProgressingView(mainRow.FightCharacter);
        }

        private void MainGridView_MouseRightButtonDown_TryRegressingView(object sender, MouseButtonEventArgs e)
            => _damageMeterViewModel.TryRegressingView();

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
