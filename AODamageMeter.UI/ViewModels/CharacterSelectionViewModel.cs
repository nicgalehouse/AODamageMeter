using AODamageMeter.UI.Properties;
using AODamageMeter.UI.Utilities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AODamageMeter.UI.ViewModels
{
    public class CharacterSelectionViewModel : ViewModelBase
    {
        public CharacterSelectionViewModel()
        {
            var characterNames = Settings.Default.CharacterNames.Cast<string>().ToArray();
            var logFilePaths = Settings.Default.LogFilePaths.Cast<string>().ToArray();
            for (int i = 0; i < characterNames.Length; ++i)
            {
                CharacterInfoViewModels.Add(new CharacterInfoViewModel(characterNames[i], logFilePaths[i]));
            }

            DeleteCommand = new RelayCommand(CanExecuteDeleteCommand, ExecuteDeleteCommand);
        }

        public ObservableCollection<CharacterInfoViewModel> CharacterInfoViewModels { get; } = new ObservableCollection<CharacterInfoViewModel>();

        protected CharacterInfoViewModel _selectedCharacterInfoViewModel;
        public CharacterInfoViewModel SelectedCharacterInfoViewModel
        {
            get => _selectedCharacterInfoViewModel;
            set
            {
                if (Set(ref _selectedCharacterInfoViewModel, value))
                {
                    RaisePropertyChanged(nameof(IsACharacterInfoViewModelSelected));
                }
            }
        }

        public bool IsACharacterInfoViewModelSelected => SelectedCharacterInfoViewModel != null;

        public void Add(CharacterInfoViewModel characterInfoViewModel)
            => CharacterInfoViewModels.Add(characterInfoViewModel);

        public ICommand DeleteCommand { get; }
        public bool CanExecuteDeleteCommand()
            => SelectedCharacterInfoViewModel != null;
        public void ExecuteDeleteCommand()
            => CharacterInfoViewModels.Remove(SelectedCharacterInfoViewModel);
    }
}
