using AODamageMeter.UI.Properties;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace AODamageMeter.UI.ViewModels
{
    public sealed class CharacterSelectionViewModel : ViewModelBase
    {
        private readonly DamageMeterViewModel _damageMeterViewModel;

        public CharacterSelectionViewModel(DamageMeterViewModel damageMeterViewModel)
        {
            Load();
            _damageMeterViewModel = damageMeterViewModel;
            DeleteCommand = new RelayCommand(CanExecuteDeleteCommand, ExecuteDeleteCommand);
            ClearFileCommand = new RelayCommand(CanExecuteClearFileCommand, ExecuteClearFileCommand);
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

        public ICommand ClearFileCommand { get; }
        public bool CanExecuteClearFileCommand()
            => SelectedCharacterInfoViewModel?.LogFilePath != null && SelectedCharacterInfoViewModel?.LogFileSize != null;
        public void ExecuteClearFileCommand()
        {
            if (_damageMeterViewModel.DamageMeter != null)
            {
                lock (_damageMeterViewModel.DamageMeter)
                {
                    TryClearFile();
                }
            }
            else
            {
                TryClearFile();
            }
        }

        private void TryClearFile()
        {
            try
            {
                File.WriteAllText(SelectedCharacterInfoViewModel.LogFilePath, string.Empty);
                SelectedCharacterInfoViewModel.RefreshLogFileSize();
                _damageMeterViewModel.DamageMeter?.SkipToStartOfLog();
            }
            catch
            {
                if (!SelectedCharacterInfoViewModel.LogFileSize.EndsWith(" (can't clear logged in)"))
                {
                    SelectedCharacterInfoViewModel.LogFileSize += " (can't clear when logged in)";
                }
            }
        }

        private void Load()
        {
            var characterNames = Settings.Default.CharacterNames.Cast<string>().ToArray();
            var logFilePaths = Settings.Default.LogFilePaths.Cast<string>().ToArray();
            for (int i = 0; i < characterNames.Length; ++i)
            {
                CharacterInfoViewModels.Add(new CharacterInfoViewModel(this, characterNames[i], logFilePaths[i]));
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
