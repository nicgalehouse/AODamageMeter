using AODamageMeter.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class DamageMeter : IDisposable
    {
        protected readonly string _logFilePath;
        protected readonly FileStream _logFileStream;
        protected readonly StreamReader _logStreamReader;

        public DamageMeter(string logFilePath, DamageMeterMode mode = DamageMeterMode.RealTime)
        {
            _logFilePath = logFilePath;
            _logFileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _logStreamReader = new StreamReader(_logFileStream);
            Mode = mode;
        }

        public DamageMeterMode Mode { get; }
        public Character Owner { get; protected set; }

        protected readonly List<Fight> _previousFights = new List<Fight>();
        public IReadOnlyList<Fight> PreviousFights { get; protected set; }
        public Fight CurrentFight { get; protected set; }

        public async Task InitializeNewFight(bool savePreviousFight = false, bool skipToEndOfLog = true)
        {
            if (savePreviousFight && (CurrentFight?.HasStarted ?? false))
            {
                _previousFights.Add(CurrentFight);
            }

            if (skipToEndOfLog)
            {
                await _logStreamReader.ReadToEndAsync();
            }

            CurrentFight = new Fight(this);
        }

        public async Task Update()
        {
            string line;
            while ((line = _logStreamReader.ReadLine()) != null)
            {
                // Set the owner as late as possible because it relies on AO being open, but some may open the damage meter before AO.
                if (Owner == null)
                {
                    await SetOwner();
                }

                await CurrentFight.AddFightEvent(line).ConfigureAwait(false);
            }
        }

        protected async Task SetOwner()
        {
            // We can (probably?) find the owner's ID from the specified log path...
            string ownersID = _logFilePath.Split('\\', '/')
                .LastOrDefault(d => d.StartsWith("Char"))
                ?.Substring("Char".Length);

            // ...And the character names that ID might correspond to from the currently opened instances of AO.
            var potentialCharacterNames = Process.GetProcessesByName("AnarchyOnline")
                .Where(p => p.MainWindowTitle.StartsWith("Anarchy Online - "))
                .Select(p => p.MainWindowTitle.Substring("Anarchy Online - ".Length))
                .ToArray();

            // Make the calls to people.anarchy-online.com concurrently.
            var characters = await Character.GetOrCreateCharacters(potentialCharacterNames).ConfigureAwait(false);
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
                Owner = await Character.GetOrCreateCharacter("You").ConfigureAwait(false);
                Owner.CharacterType = CharacterType.PlayerCharacter;
            }
        }

        public void Dispose()
        {
            _logFileStream.Dispose();
            _logStreamReader.Dispose();
        }
    }
}
