using AODamageMeter.UI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow View;

        public MainWindow()
        {
            InitializeComponent();
            View = this;
            this.DataContext = new MainWindowViewModel();
        }

        private void Draggable(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                View.DragMove();
        }
    }
}
