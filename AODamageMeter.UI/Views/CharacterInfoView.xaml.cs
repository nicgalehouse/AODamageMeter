using AODamageMeter.UI.ViewModels;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class CharacterInfoView : Window
    {
        private readonly CharacterInfoViewModel _characterInfoViewModel;

        public CharacterInfoView(CharacterInfoViewModel characterInfoViewModel = null)
        {
            InitializeComponent();
            Title = characterInfoViewModel == null ? "Add Character" : "Edit Character";
            DataContext = _characterInfoViewModel = characterInfoViewModel ?? new CharacterInfoViewModel();
        }

        private void ChooseButton_Click_ShowFileDialog(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                FileName = "Log.txt",
                DefaultExt = ".txt",
                Filter = "Log File(*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                _characterInfoViewModel.LogFilePath = dialog.FileName;
            }
        }

        private void CloseButton_Click_CloseDialog(object sender, RoutedEventArgs e)
            => Close();

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
