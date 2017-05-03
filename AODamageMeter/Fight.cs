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
        protected readonly List<FightEvent> _unmatchedEvents = new List<FightEvent>();
        public IReadOnlyList<AttackEvent> AttackEvents => _attackEvents;
        public IReadOnlyList<HealEvent> HealEvents => _healEvents;
        public IReadOnlyList<LevelEvent> LevelEvents => _levelEvents;
        public IReadOnlyList<NanoEvent> NanoEvents => _nanoEvents;
        public IReadOnlyList<SystemEvent> SystemEvents => _systemEvents;
        public IReadOnlyList<FightEvent> UnmatchedEvents => _unmatchedEvents;

        public DateTime? StartTime { get; protected set; }

        protected DateTime? _latestTime;
        public DateTime? LatestTime
        {
            get => DamageMeter.Mode == DamageMeterMode.RealTime ? DateTime.Now : _latestTime;
            protected set => _latestTime = value;
        }
        public TimeSpan? Duration => LatestTime - StartTime;
        public bool HasStarted => StartTime.HasValue;

        protected bool _isTotalDamageDoneCurrent;
        protected int _totalDamageDone;
        public int TotalDamageDone
        {
            get
            {
                if (!_isTotalDamageDoneCurrent)
                {
                    _totalDamageDone = FightCharacters.Sum(c => c.DamageDone);
                    _isTotalDamageDoneCurrent = true;
                }

                return _totalDamageDone;
            }
        }

        protected bool _isMaxDamageDoneCurrent;
        protected int _maxDamageDone;
        public int MaxDamageDone
        {
            get
            {
                if (!_isMaxDamageDoneCurrent)
                {
                    _maxDamageDone = FightCharacters.Max(c => c.DamageDone);
                    _isMaxDamageDoneCurrent = true;
                }

                return _maxDamageDone;
            }
        }

        protected bool _isMaxDamageDonePlusPetsCurrent;
        protected int _maxDamageDonePlusPets;
        public int MaxDamageDonePlusPets
        {
            get
            {
                if (!_isMaxDamageDonePlusPetsCurrent)
                {
                    _maxDamageDonePlusPets = FightCharacters.Max(c => c.DamageDonePlusPets);
                    _isMaxDamageDonePlusPetsCurrent = true;
                }

                return _maxDamageDonePlusPets;
            }
        }

        public async Task AddFightEvent(string line)
        {
            var fightEvent = await FightEvent.Create(this, line).ConfigureAwait(false);
            if (DamageMeter.Mode == DamageMeterMode.RealTime)
            {
                StartTime = StartTime ?? DateTime.Now;
            }
            else
            {
                StartTime = StartTime ?? fightEvent.Timestamp;
                LatestTime = fightEvent.Timestamp;
            }

            if (fightEvent.IsUnmatched)
            {
                _unmatchedEvents.Add(fightEvent);
#if DEBUG
                if (!(fightEvent is SystemEvent))
                {
                    Console.WriteLine($"{fightEvent.Name}: {fightEvent.Description}");
                }
#endif
                return;
            }

            switch (fightEvent)
            {
                case AttackEvent attackEvent:
                    attackEvent.Source?.AddSourceAttackEvent(attackEvent);
                    attackEvent.Target?.AddTargetAttackEvent(attackEvent);
                    _attackEvents.Add(attackEvent);
                    _isTotalDamageDoneCurrent = false;
                    _isMaxDamageDoneCurrent = false;
                    _isMaxDamageDonePlusPetsCurrent = false;
                    break;
                case HealEvent healEvent:
                    if (healEvent.Source == healEvent.Target)
                    {
                        healEvent.Source.AddSelfHealEvent(healEvent);
                    }
                    else
                    {
                        healEvent.Source.AddSourceHealEvent(healEvent);
                        healEvent.Target.AddTargetHealEvent(healEvent);
                    }
                    _healEvents.Add(healEvent);
                    break;
                case LevelEvent levelEvent:
                    levelEvent.Source.AddLevelEvent(levelEvent);
                    _levelEvents.Add(levelEvent);
                    break;
                case NanoEvent nanoEvent:
                    nanoEvent.Source.AddNanoEvent(nanoEvent);
                    _nanoEvents.Add(nanoEvent);
                    break;
                case SystemEvent systemEvent:
                    _systemEvents.Add(systemEvent);
                    break;
                default: throw new NotImplementedException();
            }
        }

        public async Task<FightCharacter> GetOrCreateFightCharacter(string name, DateTime enteredTime)
            => GetOrCreateFightCharacter(await Character.GetOrCreateCharacter(name).ConfigureAwait(false), enteredTime);

        public FightCharacter GetOrCreateFightCharacter(Character character, DateTime enteredTime)
        {
            if (_fightCharacters.TryGetValue(character, out FightCharacter fightCharacter))
                return fightCharacter;

            fightCharacter = new FightCharacter(this, character, enteredTime);
            _fightCharacters[character] = fightCharacter;
            return fightCharacter;
        }
    }
}
