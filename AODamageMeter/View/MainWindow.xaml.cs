using System;
using System.Windows;
using Microsoft.Win32;
using AODamageMeter.ViewModel;
using System.Windows.Input;

namespace AODamageMeter.View
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
