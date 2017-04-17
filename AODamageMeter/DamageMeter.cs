using AODamageMeter.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class DamageMeter : IDisposable
    {
        public static readonly DamageMeter Empty = new DamageMeter();

        private readonly string _logPath;
        private readonly FileStream _logFileStream;
        private readonly StreamReader _logStreamReader;

        private DamageMeter()
        { }

        private DamageMeter(string logPath)
        {
            _logPath = logPath;
            _logFileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _logStreamReader = new StreamReader(_logFileStream);
        }

        public static async Task<DamageMeter> Create(string logPath)
        {
            var damageMeter = new DamageMeter(logPath);
            await damageMeter._logStreamReader.ReadToEndAsync();
            await damageMeter.SetOwner();
            damageMeter.CurrentFight = new Fight(damageMeter);

            return damageMeter;
        }

        public Character Owner { get; private set; }
        public Fight CurrentFight { get; private set; }
        public bool IsPaused { get; set; }

        private async Task SetOwner()
        {
            // We can (probably?) find the owner's ID from the specified log path...
            string ownersID = _logPath.Split('\\', '/')
                .LastOrDefault(d => d.StartsWith("Char"))
                ?.Substring("Char".Length);

            // ...And the character names that ID might correspond to from the currently opened instances of AO.
            var potentialCharacterNames = Process.GetProcessesByName("AnarchyOnline")
                .Where(p => p.MainWindowTitle.StartsWith("Anarchy Online - "))
                .Select(p => p.MainWindowTitle.Substring("Anarchy Online - ".Length))
                .ToArray();

            // Make the calls to people.anarchy-online.com concurrently.
            var characters = await Character.GetOrCreateCharacters(potentialCharacterNames);
            characters.ForEach(c => c.CharacterType = CharacterType.PlayerCharacter);

            if (ownersID != null)
            {
                // If everything worked out, there'll be a character with a matching ID now.
                Owner = characters.SingleOrDefault(c => c.ID == ownersID);

                // Maybe they just created the character though, so people.anarchy-online.com hasn't indexed it yet
                // and the ID comes back as null. If there's only one w/ a null ID, deduce it's the owner.
                if (Owner == null && characters.Count(c => c.ID == null) == 1)
                {
                    Owner = characters.Single(c => c.ID == null);
                    Owner.ID = ownersID;
                }
            }
            // If for some reason the ownersID wasn't found but there's only one instance open, assume it's the owner.
            else if (characters.Length == 1)
            {
                Owner = characters[0];
            }

            // And if all that failed, use the special name "You".
            if (Owner == null)
            {
                Owner = await Character.GetOrCreateCharacter("You");
                Owner.CharacterType = CharacterType.PlayerCharacter;
            }
        }

        public async Task Update()
        {
            string line;
            while ((line = _logStreamReader.ReadLine()) != null)
            {
                await CurrentFight.AddFightEvent(line);
            }
            CurrentFight.UpdateCharactersTime();
        }

        public void Dispose()
        {
            _logFileStream.Dispose();
            _logStreamReader.Dispose();
        }
    }
}
