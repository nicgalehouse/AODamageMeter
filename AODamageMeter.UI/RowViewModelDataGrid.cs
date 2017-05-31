using AODamageMeter.UI.ViewModels;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace AODamageMeter.UI
{
    public class RowViewModelDataGrid : DataGrid
    {
        // Working around this issue: https://stackoverflow.com/questions/11177351/wpf-datagrid-ignores-sortdescription/.
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue is ListCollectionView newCollectionView && !newCollectionView.SortDescriptions.Any())
            {
                newCollectionView.SortDescriptions.Add(new SortDescription(nameof(RowViewModelBase.PercentWidth), ListSortDirection.Descending));
                newCollectionView.SortDescriptions.Add(new SortDescription(nameof(RowViewModelBase.FightCharacterName), ListSortDirection.Ascending));
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        // Working around this issue: https://stackoverflow.com/questions/14348517/child-elements-of-scrollviewer-preventing-scrolling-with-mouse-wheel.
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Name != "MainGrid")
            {
                e.Handled = true;
                RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = MouseWheelEvent
                });
            }
        }
    }
}
