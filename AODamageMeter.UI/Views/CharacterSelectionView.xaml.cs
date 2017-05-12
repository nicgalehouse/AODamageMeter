using AODamageMeter.UI.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace AODamageMeter.UI.Views
{
    public partial class CharacterSelectionView : Window
    {
        public CharacterSelectionView()
        {
            InitializeComponent();
            DataContext = CharacterSelectionViewModel = new CharacterSelectionViewModel();
        }

        public CharacterSelectionViewModel CharacterSelectionViewModel { get; }

        private void AddButton_Click_ShowCharacterInfo(object sender, RoutedEventArgs e)
        {
            var characterInfoView = new CharacterInfoView();
            if (characterInfoView.ShowDialog() == true
                && !characterInfoView.CharacterInfoViewModel.IsEmpty)
            {
                CharacterSelectionViewModel.Add(characterInfoView.CharacterInfoViewModel);
            }
        }

        private void EditButton_Click_ShowCharacterInfo(object sender, RoutedEventArgs e)
        {
            var characterInfoView = new CharacterInfoView(CharacterSelectionViewModel.SelectedCharacterInfoViewModel);
            characterInfoView.ShowDialog();
        }

        private void OKButton_Click_CloseDialog(object sender, RoutedEventArgs e)
            => DialogResult = true;

        private void HeaderRow_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == true)
            {
                CharacterSelectionViewModel.Save();
            }
        }
    }
}
