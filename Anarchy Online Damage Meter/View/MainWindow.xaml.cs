using System;
using System.Windows;
using Microsoft.Win32;
using Anarchy_Online_Damage_Meter.ViewModel;
using System.Windows.Input;

namespace Anarchy_Online_Damage_Meter.View
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
