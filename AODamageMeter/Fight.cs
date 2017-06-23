using AODamageMeter.FightEvents;
using AODamageMeter.Helpers;
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
        public IEnumerable<FightCharacter> PlayerFightCharacters => _fightCharacters.Values.Where(c => c.IsPlayer);
        public IEnumerable<FightCharacter> PlayerOrPetFightCharacters => _fightCharacters.Values.Where(c => c.IsPlayer || c.IsPet);

        public int GetFightCharacterCount(
            bool includeNPCs = true,
            bool includeZeroDamageDones = true, bool includeZeroDamageTakens = true)
            => FightCharacters.Count(c => (includeNPCs || !c.IsNPC)
                && (includeZeroDamageDones || c.OwnersOrOwnTotalDamageDonePlusPets != 0)
                && (includeZeroDamageTakens || c.TotalDamageTaken != 0));

        public FightCharacterCounts GetFightCharacterCounts(
            bool includeNPCs = true,
            bool includeZeroDamageDones = true, bool includeZeroDamageTakens = true)
            => new FightCharacterCounts(this, includeNPCs, includeZeroDamageDones, includeZeroDamageTakens);

        public DateTime? StartTime { get; protected set; }
        public DateTime? LatestEventTime { get; protected set; }
        public DateTime? EndTime { get; set; }
        public bool HasStarted => StartTime.HasValue;
        public bool HasEnded => EndTime.HasValue;

        protected Stopwatch _stopwatch;
        public TimeSpan? Duration => DamageMeter.IsRealTimeMode ? _stopwatch?.Elapsed : LatestEventTime - StartTime;

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

        public FightDamageDoneStats GetDamageDoneStats(bool includeNPCs = true, bool includeZeroDamageDones = true)
            => new FightDamageDoneStats(this, includeNPCs, includeZeroDamageDones);

        public FightDamageTakenStats GetDamageTakenStats(bool includeNPCs = true, bool includeZeroDamageTakens = true)
            => new FightDamageTakenStats(this, includeNPCs, includeZeroDamageTakens);

        protected long? _totalDamage;
        protected long TotalDamage => _totalDamage ?? (_totalDamage = FightCharacters.Sum(c => c.TotalDamageDone)).Value;
        public long TotalDamageDone => TotalDamage;
        public long TotalDamageTaken => TotalDamage;

        public double? TotalDamageDonePM => TotalDamageDone / Duration?.TotalMinutes;
        public double? TotalDamageTakenPM => TotalDamageTaken / Duration?.TotalMinutes;

        protected long? _totalPlayerDamageDone, _totalPlayerDamageDonePlusPets;
        public long TotalPlayerDamageDone => _totalPlayerDamageDone ?? (_totalPlayerDamageDone = PlayerFightCharacters.Sum(c => c.TotalDamageDone)).Value;
        public long TotalPlayerDamageDonePlusPets => _totalPlayerDamageDonePlusPets ?? (_totalPlayerDamageDonePlusPets = PlayerFightCharacters.Sum(c => c.TotalDamageDonePlusPets)).Value;

        public double? TotalPlayerDamageDonePM => TotalPlayerDamageDone / Duration?.TotalMinutes;
        public double? TotalPlayerDamageDonePMPlusPets => TotalPlayerDamageDonePlusPets / Duration?.TotalMinutes;

        protected long? _totalPlayerDamageTaken, _totalPlayerOrPetDamageTaken;
        public long TotalPlayerDamageTaken => _totalPlayerDamageTaken ?? (_totalPlayerDamageTaken = PlayerFightCharacters.Sum(c => c.TotalDamageTaken)).Value;
        public long TotalPlayerOrPetDamageTaken => _totalPlayerOrPetDamageTaken ?? (_totalPlayerOrPetDamageTaken = PlayerOrPetFightCharacters.Sum(c => c.TotalDamageTaken)).Value;

        public double? TotalPlayerDamageTakenPM => TotalPlayerDamageTaken / Duration?.TotalMinutes;
        public double? TotalPlayerOrPetDamageTakenPM => TotalPlayerOrPetDamageTaken / Duration?.TotalMinutes;

        protected long? _maxDamageDone, _maxDamageDonePlusPets, _maxPlayerDamageDone, _maxPlayerDamageDonePlusPets;
        public long? MaxDamageDone => _maxDamageDone ?? (_maxDamageDone = FightCharacters.NullableMax(c => c.TotalDamageDone));
        public long? MaxDamageDonePlusPets => _maxDamageDonePlusPets ?? (_maxDamageDonePlusPets = FightCharacters.NullableMax(c => c.TotalDamageDonePlusPets));
        public long? MaxPlayerDamageDone => _maxPlayerDamageDone ?? (_maxPlayerDamageDone = PlayerFightCharacters.NullableMax(c => c.TotalDamageDone));
        public long? MaxPlayerDamageDonePlusPets => _maxPlayerDamageDonePlusPets ?? (_maxPlayerDamageDonePlusPets = PlayerFightCharacters.NullableMax(c => c.TotalDamageDonePlusPets));

        protected long? _maxDamageTaken, _maxPlayerDamageTaken, _maxPlayerOrPetDamageTaken;
        public long? MaxDamageTaken => _maxDamageTaken ?? (_maxDamageTaken = FightCharacters.NullableMax(c => c.TotalDamageTaken));
        public long? MaxPlayerDamageTaken => _maxPlayerDamageTaken ?? (_maxPlayerDamageTaken = PlayerFightCharacters.NullableMax(c => c.TotalDamageTaken));
        public long? MaxPlayerOrPetDamageTaken => _maxPlayerOrPetDamageTaken ?? (_maxPlayerOrPetDamageTaken = PlayerOrPetFightCharacters.NullableMax(c => c.TotalDamageTaken));

        public void AddFightEvent(string line)
        {
            if (IsPaused) return;

            var fightEvent = FightEvent.Create(this, line);

            // We know these events can't cause any fight characters to enter, so don't let them start the fight.
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
                    attackEvent.Source.AddSourceAttackEvent(attackEvent);
                    attackEvent.Target.AddTargetAttackEvent(attackEvent);
                    _totalDamage = null;
                    _totalPlayerDamageDone = _totalPlayerDamageDonePlusPets = null;
                    _totalPlayerDamageTaken = _totalPlayerOrPetDamageTaken = null;
                    _maxDamageDone = _maxDamageDonePlusPets = _maxPlayerDamageDone = _maxPlayerDamageDonePlusPets = null;
                    _maxDamageTaken = _maxPlayerDamageTaken = _maxPlayerOrPetDamageTaken = null;
                    break;
                case HealEvent healEvent:
                    healEvent.Source.AddSourceHealEvent(healEvent);
                    healEvent.Target.AddTargetHealEvent(healEvent);
                    break;
                case LevelEvent levelEvent:
                    levelEvent.Source.AddLevelEvent(levelEvent);
                    break;
                case MeCastNano castEvent:
                    castEvent.Source.AddCastEvent(castEvent);
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

            if (character.IsPet)
            {
                if (character.PetOwner == null && Character.TryFitPetNamingRequirements(character.Name, out string petOwnerName))
                {
                    Character.GetOrCreateCharacter(petOwnerName)
                        .RegisterPet(character);
                }

                if (character.PetOwner == null)
                    throw new ArgumentException($"{character} is a pet but doesn't have a pet owner and one can't be deduced."
                        + " Pets need owners before being added to fights.");

                var fightPetOwner = GetOrCreateFightCharacter(character.PetOwner, enteredTime);
                fightCharacter = new FightCharacter(this, character, enteredTime, fightPetOwner);
            }
            else
            {
                fightCharacter = new FightCharacter(this, character, enteredTime);
            }

            _fightCharacters[character] = fightCharacter;

            return fightCharacter;
        }

        public bool TryGetFightCharacter(string name, out FightCharacter fightCharacter)
        {
            if (Character.TryGetCharacter(name, out Character character))
                return TryGetFightCharacter(character, out fightCharacter);

            fightCharacter = null;
            return false;
        }

        public bool TryGetFightCharacter(Character character, out FightCharacter fightCharacter)
            => _fightCharacters.TryGetValue(character, out fightCharacter);

        public bool TryGetFightOwner(out FightCharacter fightOwner)
            => _fightCharacters.TryGetValue(DamageMeter.Owner, out fightOwner);
    }
}
