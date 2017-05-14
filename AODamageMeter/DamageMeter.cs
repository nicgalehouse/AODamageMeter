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
        protected readonly StreamReader _logStreamReader;

        public DamageMeter(string logFilePath, DamageMeterMode mode = DamageMeterMode.RealTime)
        {
            _logFilePath = logFilePath;
            _logStreamReader = new StreamReader(File.Open(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            Mode = mode;
        }

        public DamageMeter(string characterName, string logFilePath, DamageMeterMode mode = DamageMeterMode.RealTime)
            : this(logFilePath, mode)
        {
            Owner = Character.GetOrCreateCharacter(characterName);
            Owner.CharacterType = CharacterType.PlayerCharacter;
        }

        public DamageMeterMode Mode { get; }
        public bool IsRealTimeMode => Mode == DamageMeterMode.RealTime;
        public bool IsParsedTimeMode => Mode == DamageMeterMode.ParsedTime;
        public Character Owner { get; protected set; }

        protected readonly List<Fight> _previousFights = new List<Fight>();
        public IReadOnlyList<Fight> PreviousFights => _previousFights;
        public Fight CurrentFight { get; protected set; }

        public void InitializeNewFight(bool savePreviousFight = false, bool skipToEndOfLog = true)
        {
            if (savePreviousFight && (CurrentFight?.HasStarted ?? false))
            {
                CurrentFight.IsPaused = IsRealTimeMode;
                _previousFights.Add(CurrentFight);
            }

            if (skipToEndOfLog)
            {
                _logStreamReader.BaseStream.Seek(0, SeekOrigin.End);
            }

            CurrentFight = new Fight(this)
            {
                IsPaused = IsPaused
            };
        }

        protected bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (IsParsedTimeMode && !value) return;
                if (IsParsedTimeMode) throw new NotSupportedException("Pausing for parsed-time meters isn't supported yet.");

                _isPaused = value;

                if (CurrentFight != null)
                {
                    CurrentFight.IsPaused = IsPaused;
                }
            }
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

                CurrentFight.AddFightEvent(line);
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
            var charactersAndBioRetrievers = Character.GetOrCreateCharactersAndBioRetrievers(potentialCharacterNames);
            await Task.WhenAll(charactersAndBioRetrievers.bioRetrievers).ConfigureAwait(false);
            var characters = charactersAndBioRetrievers.characters;
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
                Owner = Character.GetOrCreateCharacter("You");
                Owner.CharacterType = CharacterType.PlayerCharacter;
            }
        }

        public void Dispose()
            => _logStreamReader.Dispose();

        public override string ToString()
            => $"{Mode} damage meter owned by {Owner}, with {PreviousFights.Count + 1} fights.";
    }
}
