using AODamageMeter.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AODamageMeter.UI.Views
{
    public partial class MainGridView : UserControl
    {
        public MainGridView()
            => InitializeComponent();

        private void MainRowView_DetailGridTogglingRequested_TryTogglingDetailGrid(object sender, RoutedEventArgs e)
        {
            var mainRow = (e.OriginalSource as MainRowView).DataContext as MainRowViewModelBase;
            if (mainRow.DetailRows.Count == 0) return;

            var dataGridRow = (DataGridRow)MainGrid.ItemContainerGenerator.ContainerFromItem(mainRow);
            dataGridRow.DetailsVisibility = dataGridRow.DetailsVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
