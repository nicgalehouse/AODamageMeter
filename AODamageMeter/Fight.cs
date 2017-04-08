using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class Fight
    {
        private readonly DamageMeter _damageMeter;
        private readonly List<FightEvent> _fightEvents = new List<FightEvent>();
        private readonly Dictionary<Character, FightCharacter> _fightCharacters = new Dictionary<Character, FightCharacter>();
        private Stopwatch _stopwatch;

        public DateTime? StartTime => _fightEvents.FirstOrDefault()?.Timestamp;
        public DateTime? LatestTime => _fightEvents.LastOrDefault()?.Timestamp;
        public TimeSpan? Duration => _stopwatch?.Elapsed;
        public IReadOnlyList<FightEvent> FightEvents => _fightEvents;
        public ICollection<FightCharacter> FightCharacters => _fightCharacters.Values;
        public ICollection<Character> Characters => _fightCharacters.Keys;

        public Fight(DamageMeter damageMeter)
            => _damageMeter = damageMeter;

        public async Task AddFightEvent(string line)
        {
            _stopwatch = _stopwatch ?? Stopwatch.StartNew();

            _fightEvents.Add(new FightEvent(_damageMeter, this, line));

            int sourceIndex = CharactersList.FindIndex(Character => Character.Name == loggedEvent.Source);
            int targestIndex = CharactersList.FindIndex(Character => Character.Name == loggedEvent.Target);

            if (sourceIndex != -1)
            {
                CharactersList[sourceIndex].AddEvent(loggedEvent, true);
            }
            else
            {
                CharactersList.Add(new FightCharacter(loggedEvent, true, Duration.ElapsedMilliseconds));
            }

            if (targestIndex != -1)
            {
                CharactersList[targestIndex].AddEvent(loggedEvent, false);
            }
            else
            {
                CharactersList.Add(new FightCharacter(loggedEvent, false, Duration.ElapsedMilliseconds));
            }

            UpdateCharacters();
        }

        public Task<FightCharacter[]> GetOrCreateFightCharacters(IEnumerable<string> names)
            => GetOrCreateFightCharacters(names.ToArray());

        public async Task<FightCharacter[]> GetOrCreateFightCharacters(params string[] names)
        {
            var characters = await Character.GetOrCreateCharacters(names);
            var fightCharacters = new FightCharacter[characters.Length];
            for (int i = 0; i < characters.Length; ++i)
            {
                if (_fightCharacters.TryGetValue(characters[i], out FightCharacter fightCharacter))
                {
                    fightCharacters[i] = fightCharacter;
                }
                else
                {
                    fightCharacters[i] = new FightCharacter();
                    _fightCharacters[characters[i]] = fightCharacters[i];
                }
            }

            return fightCharacters;
        }

        public async Task<FightCharacter> GetOrCreateFightCharacter(string name)
        {
            var character = await Character.GetOrCreateCharacter(name);
            if (_fightCharacters.TryGetValue(character, out FightCharacter fightCharacter))
                return fightCharacter;

            fightCharacter = new FightCharacter();
            _fightCharacters[character] = fightCharacter;
            return fightCharacter;
        }

        public void UpdateCharactersTime()
        {
            foreach (FightCharacter character in CharactersList)
            {
                character.Update(Duration.ElapsedMilliseconds);
            }
        }

        public void UpdateCharacters()
        {
            int maxDamage = CharactersList.Max(x => x.DamageDone);
            int totalDamage = CharactersList.Sum(x => x.DamageDone);

            foreach (FightCharacter character in CharactersList)
            {
                character.SetPercentOfMaxDamage(maxDamage);
                character.SetPercentOfDamageDone(totalDamage);
            }
        }
    }
}
