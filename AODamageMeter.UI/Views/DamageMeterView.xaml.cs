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
                _damageMeterViewModel.TryInitializeDamageMeter(
                    Settings.Default.SelectedCharacterName, Settings.Default.SelectedLogFilePath);
            }
        }

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void CloseButton_Click_CloseApplication(object sender, RoutedEventArgs e)
            => Close();

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.Save();
            _damageMeterViewModel.DisposeDamageMeter();
        }
    }
}
