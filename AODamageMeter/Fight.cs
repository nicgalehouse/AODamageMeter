using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AODamageMeter
{
    public class Fight
    {
        private readonly DamageMeter _damageMeter;
        private Stopwatch _stopwatch;
        private readonly List<FightEvent> _events = new List<FightEvent>();
        private readonly Dictionary<Character, FightCharacter> _characters = new Dictionary<Character, FightCharacter>();

        public DateTime? StartTime => _events.FirstOrDefault()?.Timestamp;
        public DateTime? LatestEventTime => _events.LastOrDefault()?.Timestamp;
        public TimeSpan? Duration => _stopwatch?.Elapsed;
        public IReadOnlyList<FightEvent> Events => _events;
        public ICollection<FightCharacter> Characters => _characters.Values;

        public Fight(DamageMeter damageMeter)
            => _damageMeter = damageMeter;

        public void AddFightEvent(string line)
        {
            _stopwatch = _stopwatch ?? Stopwatch.StartNew();

            _events.Add(new FightEvent(_damageMeter, this, line));

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
