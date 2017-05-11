using AODamageMeter.UI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class CharacterSelectionView : Window
    {
        private readonly CharacterSelectionViewModel _characterSelectionViewModel;

        public CharacterSelectionView()
        {
            InitializeComponent();
            DataContext = _characterSelectionViewModel = new CharacterSelectionViewModel();
        }

        private void AddButton_Click_ShowCharacterInfo(object sender, RoutedEventArgs e)
        {
            var characterInfoView = new CharacterInfoView();
            if (characterInfoView.ShowDialog() == true)
            {
                _characterSelectionViewModel.Add(characterInfoView.CharacterInfoViewModel);
            }
        }

        private void EditButton_Click_ShowCharacterInfo(object sender, RoutedEventArgs e)
        {
            var characterInfoView = new CharacterInfoView(_characterSelectionViewModel.SelectedCharacterInfoViewModel);
            characterInfoView.ShowDialog();
        }

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
