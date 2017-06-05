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
        public int FightCharacterCount => FightCharacters.Count;
        public int PlayerCount => FightCharacters.Count(c => c.IsPlayer);
        public int NPCCount => FightCharacters.Count(c => c.IsNPC);
        public double? AveragePlayerLevel => FightCharacters.Where(c => c.IsPlayer).Sum(c => c.Level) / PlayerCount.NullIfZero();
        public double? AverageAlienLevel => FightCharacters.Where(c => c.IsPlayer).Sum(c => c.AlienLevel) / PlayerCount.NullIfZero();

        public bool HasProfession(Profession profession) => FightCharacters.Any(c => c.Profession == profession);
        public int GetProfessionCount(Profession profession) => FightCharacters.Count(c => c.Profession == profession);
        protected int GetTotalProfessionLevel(Profession profession) => FightCharacters.Where(c => c.Profession == profession).Sum(c => c.Level) ?? 0;
        public double? GetAverageProfessionLevel(Profession profession) => GetTotalProfessionLevel(profession) / GetProfessionCount(profession).NullIfZero();

        public DateTime? StartTime { get; protected set; }
        public DateTime? LatestEventTime { get; protected set; }
        public bool HasStarted => StartTime.HasValue;

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

        protected long? _weaponDamageDone, _critDamageDone, _glanceDamageDone, _nanoDamageDone, _indirectDamageDone;
        public long WeaponDamageDone => _weaponDamageDone ?? (_weaponDamageDone = FightCharacters.Sum(c => c.WeaponDamageDone)).Value;
        public long CritDamageDone => _critDamageDone ?? (_critDamageDone = FightCharacters.Sum(c => c.CritDamageDone)).Value;
        public long GlanceDamageDone => _glanceDamageDone ?? (_glanceDamageDone = FightCharacters.Sum(c => c.GlanceDamageDone)).Value;
        public long NanoDamageDone => _nanoDamageDone ?? (_nanoDamageDone = FightCharacters.Sum(c => c.NanoDamageDone)).Value;
        public long IndirectDamageDone => _indirectDamageDone ?? (_indirectDamageDone = FightCharacters.Sum(c => c.IndirectDamageDone)).Value;
        public long TotalDamageDone => WeaponDamageDone + NanoDamageDone + IndirectDamageDone;

        protected long? _maxDamageDone, _maxDamageDonePlusPets;
        public long? MaxDamageDone => _maxDamageDone ?? (_maxDamageDone = FightCharacters.NullableMax(c => c.TotalDamageDone));
        public long? MaxDamageDonePlusPets => _maxDamageDonePlusPets ?? (_maxDamageDonePlusPets = FightCharacters.NullableMax(c => c.TotalDamageDonePlusPets));

        public double? WeaponDamageDonePM => WeaponDamageDone / Duration?.TotalMinutes;
        public double? NanoDamageDonePM => NanoDamageDone / Duration?.TotalMinutes;
        public double? IndirectDamageDonePM => IndirectDamageDone / Duration?.TotalMinutes;
        public double? TotalDamageDonePM => TotalDamageDone / Duration?.TotalMinutes;

        public double? WeaponPercentOfTotalDamageDone => WeaponDamageDone / TotalDamageDone.NullIfZero();
        public double? NanoPercentOfTotalDamageDone => NanoDamageDone / TotalDamageDone.NullIfZero();
        public double? IndirectPercentOfTotalDamageDone => IndirectDamageDone / TotalDamageDone.NullIfZero();

        protected int? _weaponHitsDone, _critsDone, _glancesDone, _missesDone, _nanoHitsDone, _indirectHitsDone;
        public int WeaponHitsDone => _weaponHitsDone ?? (_weaponHitsDone = FightCharacters.Sum(c => c.WeaponHitsDone)).Value;
        public int CritsDone => _critsDone ?? (_critsDone = FightCharacters.Sum(c => c.CritsDone)).Value;
        public int GlancesDone => _glancesDone ?? (_glancesDone = FightCharacters.Sum(c => c.GlancesDone)).Value;
        public int MissesDone => _missesDone ?? (_missesDone = FightCharacters.Sum(c => c.MissesDone)).Value;
        public int WeaponHitAttemptsDone => WeaponHitsDone + MissesDone;
        public int NanoHitsDone => _nanoHitsDone ?? (_nanoHitsDone = FightCharacters.Sum(c => c.NanoHitsDone)).Value;
        public int IndirectHitsDone => _indirectHitsDone ?? (_indirectHitsDone = FightCharacters.Sum(c => c.IndirectHitsDone)).Value;
        public int TotalHitsDone => WeaponHitsDone + NanoHitsDone + IndirectHitsDone;

        public double? WeaponHitsDonePM => WeaponHitsDone / Duration?.TotalMinutes;
        public double? CritsDonePM => CritsDone / Duration?.TotalMinutes;
        public double? GlancesDonePM => GlancesDone / Duration?.TotalMinutes;
        public double? MissesDonePM => MissesDone / Duration?.TotalMinutes;
        public double? WeaponHitAttemptsDonePM => WeaponHitAttemptsDone / Duration?.TotalMinutes;
        public double? NanoHitsDonePM => NanoHitsDone / Duration?.TotalMinutes;
        public double? IndirectHitsDonePM => IndirectHitsDone / Duration?.TotalMinutes;
        public double? TotalHitsDonePM => TotalHitsDone / Duration?.TotalMinutes ;

        public double? WeaponHitDoneChance => WeaponHitsDone / WeaponHitAttemptsDone.NullIfZero();
        public double? CritDoneChance => CritsDone / WeaponHitAttemptsDone.NullIfZero();
        public double? GlanceDoneChance => GlancesDone / WeaponHitAttemptsDone.NullIfZero();
        public double? MissDoneChance => MissesDone / WeaponHitAttemptsDone.NullIfZero();

        public double? AverageWeaponDamageDone => WeaponDamageDone / WeaponHitsDone.NullIfZero();
        public double? AverageCritDamageDone => CritDamageDone / CritsDone.NullIfZero();
        public double? AverageGlanceDamageDone => GlanceDamageDone / GlancesDone.NullIfZero();
        public double? AverageNanoDamageDone => NanoDamageDone / NanoHitsDone.NullIfZero();
        public double? AverageIndirectDamageDone => IndirectDamageDone / IndirectHitsDone.NullIfZero();

        public bool HasDamageTypeDamageDone(DamageType damageType) => FightCharacters.Any(c => c.DamageTypeDamagesDone.ContainsKey(damageType));
        public bool HasSpecialsDone => DamageTypeHelpers.SpecialDamageTypes.Any(HasDamageTypeDamageDone);
        public int? GetDamageTypeHitsDone(DamageType damageType) => FightCharacters.NullableSum(c => c.GetDamageTypeHitsDone(damageType));
        public long? GetDamageTypeDamageDone(DamageType damageType) => FightCharacters.NullableSum(c => c.GetDamageTypeDamageDone(damageType));
        public double? GetAverageDamageTypeDamageDone(DamageType damageType) => GetDamageTypeDamageDone(damageType) / (double?)GetDamageTypeHitsDone(damageType);
        public double? GetSecondsPerDamageTypeHitDone(DamageType damageType) => Duration?.TotalSeconds / GetDamageTypeHitsDone(damageType);

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
                    attackEvent.Source?.AddSourceAttackEvent(attackEvent);
                    attackEvent.Target?.AddTargetAttackEvent(attackEvent);
                    _weaponDamageDone = _critDamageDone = _glanceDamageDone = _nanoDamageDone = _indirectDamageDone = null;
                    _maxDamageDone = _maxDamageDonePlusPets = null;
                    _weaponHitsDone = _critsDone = _glancesDone = _missesDone = _nanoHitsDone = _indirectHitsDone = null;
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

            if (character.IsPet)
            {
                if (character.PetOwner == null && Character.TryFittingPetNamingRequirements(character.Name, out string petOwnerName))
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
    }
}
