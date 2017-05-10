using AODamageMeter.UI.Properties;
using System.Collections.ObjectModel;
using System.Linq;

namespace AODamageMeter.UI.ViewModels
{
    public class CharacterSelectionViewModel : ViewModelBase
    {
        public CharacterSelectionViewModel()
        {
            var characterNames = Settings.Default.CharacterNames.Cast<string>().ToArray();
            var characterLogFilePaths = Settings.Default.CharacterLogFilePaths.Cast<string>().ToArray();
            for (int i = 0; i < characterNames.Length; ++i)
            {
                CharacterInfos.Add(new CharacterInfoViewModel(characterNames[i], characterLogFilePaths[i]));
            }
            CharacterInfos.Add(new CharacterInfoViewModel("Reimagine", "long file path super long long file path long"));
            CharacterInfos.Add(new CharacterInfoViewModel("Reimagine", "long file path super long long file path long"));
        }

        public ObservableCollection<CharacterInfoViewModel> CharacterInfos { get; } = new ObservableCollection<CharacterInfoViewModel>();
    }
}
