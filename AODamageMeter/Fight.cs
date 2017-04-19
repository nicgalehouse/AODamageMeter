using AODamageMeter.FightEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class Fight
    {
        public Fight(DamageMeter damageMeter)
            => DamageMeter = damageMeter;

        public DamageMeter DamageMeter { get; }

        protected readonly Dictionary<Character, FightCharacter> _fightCharacters = new Dictionary<Character, FightCharacter>();
        public IReadOnlyCollection<FightCharacter> FightCharacters => _fightCharacters.Values;

        protected readonly List<AttackEvent> _attackEvents = new List<AttackEvent>();
        protected readonly List<HealEvent> _healEvents = new List<HealEvent>();
        protected readonly List<LevelEvent> _levelEvents = new List<LevelEvent>();
        protected readonly List<NanoEvent> _nanoEvents = new List<NanoEvent>();
        protected readonly List<SystemEvent> _systemEvents = new List<SystemEvent>();
        public IReadOnlyList<AttackEvent> AttackEvents => _attackEvents;
        public IReadOnlyList<HealEvent> HealEvents => _healEvents;
        public IReadOnlyList<LevelEvent> LevelEvents => _levelEvents;
        public IReadOnlyList<NanoEvent> NanoEvents => _nanoEvents;
        public IReadOnlyList<SystemEvent> SystemEvents => _systemEvents;

        public DateTime? StartTime { get; protected set; }
        public DateTime? LatestTime { get; protected set; }
        public TimeSpan? Duration => LatestTime - StartTime;
        public bool HasStarted => StartTime.HasValue;

        public async Task<FightCharacter> GetOrCreateFightCharacter(string name, DateTime enteredTime)
            => GetOrCreateFightCharacter(await Character.GetOrCreateCharacter(name), enteredTime);

        public FightCharacter GetOrCreateFightCharacter(Character character, DateTime enteredTime)
        {
            if (_fightCharacters.TryGetValue(character, out FightCharacter fightCharacter))
                return fightCharacter;

            fightCharacter = new FightCharacter(this, character, enteredTime);
            _fightCharacters[character] = fightCharacter;
            return fightCharacter;
        }

        public async Task AddFightEvent(string line)
        {
            FightEvent fightEvent = await FightEvent.Create(this, line);
            StartTime = StartTime ?? fightEvent.Timestamp;
            LatestTime = fightEvent.Timestamp;

            switch (fightEvent)
            {
                case AttackEvent attackEvent:
                    _attackEvents.Add(attackEvent);
                    attackEvent.Source?.AddSourceAttackEvent(attackEvent);
                    attackEvent.Target?.AddTargetAttackEvent(attackEvent);
                    break;
            }

            else if (fightEvent is HealEvent) _healEvents.Add((HealEvent)fightEvent);
            else if (fightEvent is LevelEvent) _levelEvents.Add((LevelEvent)fightEvent);
            else if (fightEvent is NanoEvent) _nanoEvents.Add((NanoEvent)fightEvent);
            else if (fightEvent is SystemEvent) _systemEvents.Add((SystemEvent)fightEvent);
            else throw new NotImplementedException();


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
