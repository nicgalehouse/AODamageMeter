using AODamageMeter.FightEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        public DateTime? LatestEventTime { get; protected set; }
        public bool HasStarted => StartTime.HasValue;

        protected Stopwatch _stopwatch;
        public TimeSpan? Duration => DamageMeter.IsRealTimeMode ? _stopwatch.Elapsed : LatestEventTime - StartTime;

        protected bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (DamageMeter.IsParsedTimeMode && !value) return;
                if (DamageMeter.IsParsedTimeMode) throw new NotSupportedException("Pausing for parsed-time meters isn't supported yet.");

                _isPaused = value;
                if (!HasStarted) return;
                if (IsPaused) _stopwatch.Stop();
                else _stopwatch.Start();

                foreach (var fightCharacter in FightCharacters)
                {
                    fightCharacter.IsPaused = IsPaused;
                }
            }
        }

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

        public void AddFightEvent(string line)
        {
            if (IsPaused) return;

            var fightEvent = FightEvent.Create(this, line);
            if (!HasStarted)
            {
                StartTime = fightEvent.Timestamp;
                _stopwatch = DamageMeter.IsRealTimeMode ? Stopwatch.StartNew() : null;
            }
            LatestEventTime = fightEvent.Timestamp;

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

        public FightCharacter GetOrCreateFightCharacter(string name, DateTime enteredTime)
            => GetOrCreateFightCharacter(Character.GetOrCreateCharacter(name), enteredTime);

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
