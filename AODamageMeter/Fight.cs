using AODamageMeter.FightEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class Fight
    {
        protected readonly List<NanoEvent> _nanoEvents = new List<NanoEvent>();
        protected readonly List<FightEvent> _fightEvents = new List<FightEvent>();
        protected readonly Dictionary<Character, FightCharacter> _fightCharacters = new Dictionary<Character, FightCharacter>();
        protected Stopwatch _stopwatch;

        public DamageMeter DamageMeter { get; }
        public DateTime? StartTime => _fightEvents.FirstOrDefault()?.Timestamp;
        public DateTime? LatestTime => _fightEvents.LastOrDefault()?.Timestamp;
        public TimeSpan? Duration => _stopwatch?.Elapsed;
        public IReadOnlyList<NanoEvent> NanoEvents => _nanoEvents;
        public IReadOnlyList<FightEvent> FightEvents => _fightEvents;
        public IReadOnlyCollection<FightCharacter> FightCharacters => _fightCharacters.Values;

        public Fight(DamageMeter damageMeter)
            => DamageMeter = damageMeter;

        public async Task AddFightEvent(string line)
        {
            _stopwatch = _stopwatch ?? Stopwatch.StartNew();

            _fightEvents.Add(new FightEvent(this, line));

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

        public async Task<FightCharacter> GetOrCreateFightCharacter(string name)
            => GetOrCreateFightCharacter(await Character.GetOrCreateCharacter(name));

        public Task<FightCharacter[]> GetOrCreateFightCharacters(params string[] names)
            => Task.WhenAll(names.Select(GetOrCreateFightCharacter).ToArray());

        public FightCharacter GetOrCreateFightCharacter(Character character)
        {
            if (_fightCharacters.TryGetValue(character, out FightCharacter fightCharacter))
                return fightCharacter;

            fightCharacter = new FightCharacter(character);
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
