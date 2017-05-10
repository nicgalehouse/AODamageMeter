using AODamageMeter.UI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class CharacterSelectionView : Window
    {
        public CharacterSelectionView()
        {
            InitializeComponent();
            DataContext = new CharacterSelectionViewModel();
        }

        private void AddButton_Click_ShowCharacterInfo(object sender, RoutedEventArgs e)
        {
            var xx = new CharacterInfoView();
            if (xx.ShowDialog() == true)
            {

            }
        }

        private void CloseButton_Click_CloseDialog(object sender, RoutedEventArgs e)
            => Close();

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
