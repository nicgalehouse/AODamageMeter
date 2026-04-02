using AODamageMeter.UI.Properties;
using AODamageMeter.UI.ViewModels;
using AODamageMeter.UI.ViewModels.BossModules;
using AODamageMeter.UI.Views.BossModules;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class BossModuleView : Window
    {
        public BossModuleView(string bossModuleName)
        {
            InitializeComponent();

            Title = bossModuleName;

            IBossModuleViewModel viewModel;
            UserControl view;

            switch (bossModuleName)
            {
                case "The Beast":
                    viewModel = new TheBeastModuleViewModel();
                    view = new TheBeastModuleView();
                    break;
                default: throw new System.ArgumentException($"Unknown boss module: {bossModuleName}");
            }

            view.DataContext = viewModel;
            BossModuleViewModel = viewModel;
            BossModuleViewContent.Content = view;
        }

        public IBossModuleViewModel BossModuleViewModel { get; private set; }

        public bool PreserveSelectedBossModuleOnClose { get; set; } = false;

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click_Minimize(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void CloseButton_Click_Close(object sender, RoutedEventArgs e)
            => Close();

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!PreserveSelectedBossModuleOnClose)
            {
                Settings.Default.BossModule = "";
                Settings.Default.Save();
            }

            base.OnClosing(e);
        }
    }
}
