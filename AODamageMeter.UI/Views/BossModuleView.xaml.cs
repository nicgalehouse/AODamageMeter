using AODamageMeter.UI.Properties;
using AODamageMeter.UI.ViewModels;
using AODamageMeter.UI.ViewModels.BossModules;
using AODamageMeter.UI.Views.BossModules;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class BossModuleView : Window
    {
        public BossModuleView(string bossModuleName)
        {
            InitializeComponent();

            Title = bossModuleName;

            BossModuleViewModelBase bossModuleViewModel;
            UserControl bossModuleView;

            switch (bossModuleName)
            {
                case "The Beast":
                    bossModuleViewModel = new TheBeastModuleViewModel();
                    bossModuleView = new TheBeastModuleView();
                    BindWindowPosition(nameof(Settings.Default.TheBeastViewHeight),
                        nameof(Settings.Default.TheBeastViewWidth),
                        nameof(Settings.Default.TheBeastViewTop),
                        nameof(Settings.Default.TheBeastViewLeft));
                    break;
                default: throw new ArgumentException($"Unknown boss module: {bossModuleName}");
            }

            DataContext = bossModuleView.DataContext = BossModuleViewModel = bossModuleViewModel;
            BossModuleViewContent.Content = bossModuleView;

            LocationChanged += RepositionStatusBarsPopup;
            SizeChanged += RepositionStatusBarsPopup;
            StatusBarsPopup.CustomPopupPlacementCallback = PlaceStatusBarsPopup;
        }

        public BossModuleViewModelBase BossModuleViewModel { get; private set; }
        public bool PreserveSelectedBossModuleOnClose { get; set; } = false;

        private void BindWindowPosition(string heightProperty, string widthProperty, string topProperty, string leftProperty)
        {
            SetBinding(HeightProperty, new Binding(heightProperty) { Source = Settings.Default, Mode = BindingMode.TwoWay });
            SetBinding(WidthProperty, new Binding(widthProperty) { Source = Settings.Default, Mode = BindingMode.TwoWay });
            SetBinding(TopProperty, new Binding(topProperty) { Source = Settings.Default, Mode = BindingMode.TwoWay });
            SetBinding(LeftProperty, new Binding(leftProperty) { Source = Settings.Default, Mode = BindingMode.TwoWay });
        }

        // Toggling HorizontalOffset by +1 then back forces WPF to re-evaluate the popup's
        // position relative to its placement target, which is how we keep it fixed to the
        // window across drags and resizes.
        private void RepositionStatusBarsPopup(object sender, EventArgs e)
        {
            if (StatusBarsPopup == null || !StatusBarsPopup.IsOpen) return;

            double originalOffset = StatusBarsPopup.HorizontalOffset;
            StatusBarsPopup.HorizontalOffset = originalOffset + 1;
            StatusBarsPopup.HorizontalOffset = originalOffset;
        }

        private static CustomPopupPlacement[] PlaceStatusBarsPopup(Size popupSize, Size targetSize, Point offset)
            => new[]
            {
                // Prefer bottom placement with a 4px gap between window bottom and popup area.
                new CustomPopupPlacement(
                    new Point(0, targetSize.Height + 4),
                    PopupPrimaryAxis.Horizontal),
                // If unavailable, use top placement. This offset works because each popup row
                // already has 4px bottom margin, so we just need the popup area as a whole to be
                // its own height above the window.
                new CustomPopupPlacement(
                    new Point(0, -popupSize.Height),
                    PopupPrimaryAxis.Horizontal),
            };

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void ResetButton_Click_Reset(object sender, RoutedEventArgs e)
            => BossModuleViewModel.Reset();

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
