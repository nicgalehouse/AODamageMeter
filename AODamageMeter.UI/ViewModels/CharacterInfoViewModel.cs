using AODamageMeter.UI.Utilities;
using System.Windows.Input;

namespace AODamageMeter.UI.ViewModels
{
    public class CharacterInfoViewModel : ViewModelBase
    {
        public CharacterInfoViewModel(string characterName = null, string logFilePath = null)
        {
            CharacterName = characterName;
            LogFilePath = logFilePath;
            AutoConfigureCommand = new RelayCommand(CanExecuteAutoConfigureCommand, ExecuteAutoConfigureCommand);
        }

        private string _characterName;
        public string CharacterName
        {
            get => _characterName;
            set
            {
                // When this changes null out the configure result to avoid confusion over stale data.
                if (Set(ref _characterName, value))
                {
                    AutoConfigureResult = null;
                }
            }
        }

        private string _logFilePath;
        public string LogFilePath
        {
            get => _logFilePath;
            set => Set(ref _logFilePath, value);
        }

        public ICommand AutoConfigureCommand { get; }
        private bool _isExecutingAutoConfigureCommand;
        private string _previousCharacterName;
        private bool CanExecuteAutoConfigureCommand()
            => !_isExecutingAutoConfigureCommand
            && CharacterName != _previousCharacterName
            && Character.FitsPlayerNamingRequirements(CharacterName);
        private async void ExecuteAutoConfigureCommand()
        {
            _isExecutingAutoConfigureCommand = true;
            var characterAndBioRetriever = Character.GetOrCreateCharacterAndBioRetriever(CharacterName);
            await characterAndBioRetriever.bioRetriever;
            AutoConfigureResult = characterAndBioRetriever.character.ID == null ? "Failure" : "Success";
            _previousCharacterName = CharacterName;
            _isExecutingAutoConfigureCommand = false;
        }

        private string _autoConfigureResult;
        public string AutoConfigureResult
        {
            get => _autoConfigureResult;
            set => Set(ref _autoConfigureResult, value);
        }
    }
}
