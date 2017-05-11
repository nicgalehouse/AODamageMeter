using AODamageMeter.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AODamageMeter.UI.Views
{
    public partial class MainGridView : UserControl
    {
        public MainGridView()
            => InitializeComponent();

        private void MainRowView_DetailGridNeedsToggling_ToggleDetailGrid(object sender, RoutedEventArgs e)
        {
            var mainRowViewModel = (e.Source as MainRowView).DataContext as RowViewModelBase;
            if (mainRowViewModel.DetailRowViewModels.Count == 0) return;

            var dataGridRow = (DataGridRow)MainGrid.ItemContainerGenerator.ContainerFromItem(mainRowViewModel);
            dataGridRow.DetailsVisibility = dataGridRow.DetailsVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
