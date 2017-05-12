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
            Load();
            DeleteCommand = new RelayCommand(CanExecuteDeleteCommand, ExecuteDeleteCommand);
        }

        public ObservableCollection<CharacterInfoViewModel> CharacterInfoViewModels { get; } = new ObservableCollection<CharacterInfoViewModel>();

        private CharacterInfoViewModel _selectedCharacterInfoViewModel;
        public CharacterInfoViewModel SelectedCharacterInfoViewModel
        {
            get => _selectedCharacterInfoViewModel;
            set => Set(ref _selectedCharacterInfoViewModel, value);
        }

        public void Add(CharacterInfoViewModel characterInfoViewModel)
            => CharacterInfoViewModels.Add(characterInfoViewModel);

        public ICommand DeleteCommand { get; }
        public bool CanExecuteDeleteCommand() => SelectedCharacterInfoViewModel != null;
        public void ExecuteDeleteCommand()
            => CharacterInfoViewModels.Remove(SelectedCharacterInfoViewModel);

        private void Load()
        {
            var characterNames = Settings.Default.CharacterNames.Cast<string>().ToArray();
            var logFilePaths = Settings.Default.LogFilePaths.Cast<string>().ToArray();
            for (int i = 0; i < characterNames.Length; ++i)
            {
                CharacterInfoViewModels.Add(new CharacterInfoViewModel(characterNames[i], logFilePaths[i]));
            }

            string selectedCharacterName = Settings.Default.SelectedCharacterName;
            string selectedLogFilePath = Settings.Default.SelectedLogFilePath;
            SelectedCharacterInfoViewModel = CharacterInfoViewModels
                .FirstOrDefault(c => c.CharacterName == selectedCharacterName && c.LogFilePath == selectedLogFilePath);
        }

        public void Save()
        {
            Settings.Default.CharacterNames.Clear();
            Settings.Default.CharacterNames.AddRange(CharacterInfoViewModels.Select(c => c.CharacterName).ToArray());
            Settings.Default.LogFilePaths.Clear();
            Settings.Default.LogFilePaths.AddRange(CharacterInfoViewModels.Select(c => c.LogFilePath).ToArray());
            Settings.Default.SelectedCharacterName = SelectedCharacterInfoViewModel?.CharacterName;
            Settings.Default.SelectedLogFilePath = SelectedCharacterInfoViewModel?.LogFilePath;
            Settings.Default.Save();
        }
    }
}
