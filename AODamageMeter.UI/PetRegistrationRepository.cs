using AODamageMeter.UI.Properties;
using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter.UI
{
    public static class PetRegistrationRepository
    {
        private const string Separator = "፦"; // Because player & pet names in AO can't contain this character.

        static PetRegistrationRepository()
        {
            // Just being paranoid here to avoid throwing exceptions if data gets messed up...
            // Could just do .ToDictionary(r => r[0], r => r[1]).
            var petRegistrations = Settings.Default.PetRegistrations
                .Cast<string>()
                .Select(r => r.Split(new[] { Separator }, 2, System.StringSplitOptions.None))
                .ToArray();
            foreach (var petRegistration in petRegistrations.Where(r => r.Length == 2))
            {
                _petRegistrations[petRegistration[0]] = petRegistration[1];
            }
        }

        private static Dictionary<string, string> _petRegistrations = new Dictionary<string, string>();
        public static IReadOnlyDictionary<string, string> PetRegistrations => _petRegistrations;

        public static void AddRegistration(FightCharacter fightPet, FightCharacter fightPetMaster)
        {
            if (_petRegistrations.TryGetValue(fightPet.Name, out string currentPetMasterName))
            {
                if (currentPetMasterName != fightPetMaster.Name)
                {
                    _petRegistrations[fightPet.Name] = fightPetMaster.Name;
                    Settings.Default.PetRegistrations.Remove($"{fightPet.Name}{Separator}{currentPetMasterName}");
                    Settings.Default.PetRegistrations.Add($"{fightPet.Name}{Separator}{fightPetMaster.Name}");
                }
            }
            else
            {
                _petRegistrations.Add(fightPet.Name, fightPetMaster.Name);
                Settings.Default.PetRegistrations.Add($"{fightPet.Name}{Separator}{fightPetMaster.Name}");
            }
        }

        public static void RemoveRegistration(FightCharacter fightPet, FightCharacter fightPetMaster)
        {
            if (_petRegistrations.TryGetValue(fightPet.Name, out string currentPetMasterName))
            {
                if (currentPetMasterName == fightPetMaster.Name)
                {
                    _petRegistrations.Remove(fightPet.Name);
                    Settings.Default.PetRegistrations.Remove($"{fightPet.Name}{Separator}{currentPetMasterName}");
                }
            }
        }
    }
}
