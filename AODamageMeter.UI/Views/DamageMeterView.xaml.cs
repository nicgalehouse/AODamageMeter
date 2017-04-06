using AODamageMeter.UI.ViewModels;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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

        private void FileButton_Click_SetLogFile(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                FileName = "Log.txt",
                DefaultExt = ".txt",
                Filter = "Log File(*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                _damageMeterViewModel.SetLogFile(dialog.FileName);
            }
        }

        private void Draggable(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
