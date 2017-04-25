using AODamageMeter.UI.ViewModels;
using Microsoft.Win32;
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

        private async void FileButton_Click_SetLogFile(object sender, RoutedEventArgs e)
        {
            FileButton.IsEnabled = false;

            var dialog = new OpenFileDialog()
            {
                FileName = "Log.txt",
                DefaultExt = ".txt",
                Filter = "Log File(*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                await _damageMeterViewModel.SetLogFile(dialog.FileName);
            }

            FileButton.IsEnabled = true;
        }

        private async void CloseButton_Click_CloseApplication(object sender, RoutedEventArgs e)
        {
            CloseButton.IsEnabled = false;

            await _damageMeterViewModel.DisposeDamageMeter();
            Close();
        }

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
