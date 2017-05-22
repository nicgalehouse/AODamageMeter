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
        protected long _totalDamageDone;
        public long TotalDamageDone
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
        protected long _maxDamageDone;
        public long MaxDamageDone
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
        protected long _maxDamageDonePlusPets;
        public long MaxDamageDonePlusPets
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

            // We know these events can't cause any fight characters to enter (from FightEvent.Create), so don't let the fight start.
            if (fightEvent is SystemEvent || fightEvent is UnrecognizedEvent)
                return;

            if (!HasStarted)
            {
                StartTime = fightEvent.Timestamp;
                _stopwatch = DamageMeter.IsRealTimeMode ? Stopwatch.StartNew() : null;
            }
            LatestEventTime = fightEvent.Timestamp;

            if (fightEvent.IsUnmatched)
            {
#if DEBUG
                Debug.WriteLine($"{fightEvent.Name}: {fightEvent.Description}");
#endif
                return;
            }

            switch (fightEvent)
            {
                case AttackEvent attackEvent:
                    attackEvent.Source?.AddSourceAttackEvent(attackEvent);
                    attackEvent.Target?.AddTargetAttackEvent(attackEvent);
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
                    break;
                case LevelEvent levelEvent:
                    levelEvent.Source.AddLevelEvent(levelEvent);
                    break;
                case NanoEvent nanoEvent:
                    nanoEvent.Source.AddNanoEvent(nanoEvent);
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
