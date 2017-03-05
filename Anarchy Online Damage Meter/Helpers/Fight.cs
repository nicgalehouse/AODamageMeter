using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Anarchy_Online_Damage_Meter.Helpers
{
    public class Fight
    {
        private List<Event> history = new List<Event>();

        public List<Character> CharactersList = new List<Character>();
        public int StartTime { get; set; }
        public int TimeOfLatestAction { get; set; }
        public Stopwatch Duration { get; set; } = new Stopwatch();

        public Fight() { }

        public void AddEvent(Event loggedEvent)
        {
            if (!Duration.IsRunning)
                Duration.Start();

            history.Add(loggedEvent);

            int sourceIndex = CharactersList.FindIndex(Character => Character.Name == loggedEvent.Source);
            int targestIndex = CharactersList.FindIndex(Character => Character.Name == loggedEvent.Target);

            if (sourceIndex != -1)
            {
                CharactersList[sourceIndex].AddEvent(loggedEvent, true);
            }
            else
            {
                CharactersList.Add(new Character(loggedEvent, true, Duration.ElapsedMilliseconds));
            }

            if (targestIndex != -1)
            {
                CharactersList[targestIndex].AddEvent(loggedEvent, false);
            }
            else
            {
                CharactersList.Add(new Character(loggedEvent, false, Duration.ElapsedMilliseconds));
            }

            UpdateCharacters();
        }

        public void UpdateCharactersTime()
        {
            foreach (Character character in CharactersList)
            {
                character.Update(Duration.ElapsedMilliseconds);
            }
        }

        public void UpdateCharacters()
        {
            int maxDamage = CharactersList.Max(x => x.DamageDone);
            int totalDamage = CharactersList.Sum(x => x.DamageDone);

            foreach (Character character in CharactersList)
            {
                character.SetPercentOfMaxDamage(maxDamage);
                character.SetPercentOfDamageDone(totalDamage);
            }
        }
    }
}
