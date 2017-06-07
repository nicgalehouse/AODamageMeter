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
        public int OmniPlayerCount => FightCharacters.Count(c => c.Faction == Faction.Omni);
        public int ClanPlayerCount => FightCharacters.Count(c => c.Faction == Faction.Clan);
        public int NeutralPlayerCount => FightCharacters.Count(c => c.Faction == Faction.Neutral);
        public int UnknownPlayerCount => FightCharacters.Count(c => c.Faction == Faction.Unknown);
        public int NPCCount => FightCharacters.Count(c => c.IsNPC);
        public double? AveragePlayerLevel => FightCharacters.Sum(c => c.Level) / PlayerCount.NullIfZero();
        public double? AveragePlayerAlienLevel => FightCharacters.Sum(c => c.AlienLevel) / PlayerCount.NullIfZero();
        public double? AverageOmniPlayerLevel => FightCharacters.Where(c => c.Faction == Faction.Omni).Sum(c => c.Level) / OmniPlayerCount.NullIfZero();
        public double? AverageOmniPlayerAlienLevel => FightCharacters.Where(c => c.Faction == Faction.Omni).Sum(c => c.AlienLevel) / OmniPlayerCount.NullIfZero();
        public double? AverageClanPlayerLevel => FightCharacters.Where(c => c.Faction == Faction.Clan).Sum(c => c.Level) / ClanPlayerCount.NullIfZero();
        public double? AverageClanPlayerAlienLevel => FightCharacters.Where(c => c.Faction == Faction.Clan).Sum(c => c.AlienLevel) / ClanPlayerCount.NullIfZero();
        public double? AverageNeutralPlayerLevel => FightCharacters.Where(c => c.Faction == Faction.Neutral).Sum(c => c.Level) / NeutralPlayerCount.NullIfZero();
        public double? AverageNeutralPlayerAlienLevel => FightCharacters.Where(c => c.Faction == Faction.Neutral).Sum(c => c.AlienLevel) / NeutralPlayerCount.NullIfZero();
        public double? AverageUnknownPlayerLevel => FightCharacters.Where(c => c.Faction == Faction.Unknown).NullableSum(c => c.Level) / UnknownPlayerCount.NullIfZero();
        public double? AverageUnknownPlayerAlienLevel => FightCharacters.Where(c => c.Faction == Faction.Unknown).NullableSum(c => c.AlienLevel) / UnknownPlayerCount.NullIfZero();

        public bool HasProfession(Profession profession) => FightCharacters.Any(c => c.Profession == profession);
        public int GetProfessionCount(Profession profession) => FightCharacters.Count(c => c.Profession == profession);
        protected int? GetTotalProfessionLevel(Profession profession) => FightCharacters.Where(c => c.Profession == profession).NullableSum(c => c.Level);
        protected int? GetTotalProfessionAlienLevel(Profession profession) => FightCharacters.Where(c => c.Profession == profession).NullableSum(c => c.AlienLevel);
        public double? GetAverageProfessionLevel(Profession profession) => GetTotalProfessionLevel(profession) / GetProfessionCount(profession).NullIfZero();
        public double? GetAverageProfessionAlienLevel(Profession profession) => GetTotalProfessionAlienLevel(profession) / GetProfessionCount(profession).NullIfZero();

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

        protected long? _weaponDamage, _critDamage, _glanceDamage, _nanoDamage, _indirectDamage;
        public long WeaponDamage => _weaponDamage ?? (_weaponDamage = FightCharacters.Sum(c => c.WeaponDamageDone)).Value;
        public long CritDamage => _critDamage ?? (_critDamage = FightCharacters.Sum(c => c.CritDamageDone)).Value;
        public long GlanceDamage => _glanceDamage ?? (_glanceDamage = FightCharacters.Sum(c => c.GlanceDamageDone)).Value;
        public long NanoDamage => _nanoDamage ?? (_nanoDamage = FightCharacters.Sum(c => c.NanoDamageDone)).Value;
        public long IndirectDamage => _indirectDamage ?? (_indirectDamage = FightCharacters.Sum(c => c.IndirectDamageDone)).Value;
        public long TotalDamage => WeaponDamage + NanoDamage + IndirectDamage;

        public double? WeaponDamagePM => WeaponDamage / Duration?.TotalMinutes;
        public double? NanoDamagePM => NanoDamage / Duration?.TotalMinutes;
        public double? IndirectDamagePM => IndirectDamage / Duration?.TotalMinutes;
        public double? TotalDamagePM => TotalDamage / Duration?.TotalMinutes;

        public double? WeaponPercentOfTotalDamage => WeaponDamage / TotalDamage.NullIfZero();
        public double? NanoPercentOfTotalDamage => NanoDamage / TotalDamage.NullIfZero();
        public double? IndirectPercentOfTotalDamage => IndirectDamage / TotalDamage.NullIfZero();

        protected int? _weaponHits, _crits, _glances, _misses, _nanoHits, _indirectHits;
        public int WeaponHits => _weaponHits ?? (_weaponHits = FightCharacters.Sum(c => c.WeaponHitsDone)).Value;
        public int Crits => _crits ?? (_crits = FightCharacters.Sum(c => c.CritsDone)).Value;
        public int Glances => _glances ?? (_glances = FightCharacters.Sum(c => c.GlancesDone)).Value;
        public int Misses => _misses ?? (_misses = FightCharacters.Sum(c => c.MissesDone)).Value;
        public int WeaponHitAttempts => WeaponHits + Misses;
        public int NanoHits => _nanoHits ?? (_nanoHits = FightCharacters.Sum(c => c.NanoHitsDone)).Value;
        public int IndirectHits => _indirectHits ?? (_indirectHits = FightCharacters.Sum(c => c.IndirectHitsDone)).Value;
        public int TotalHits => WeaponHits + NanoHits + IndirectHits;

        public double? WeaponHitsPM => WeaponHits / Duration?.TotalMinutes;
        public double? CritsPM => Crits / Duration?.TotalMinutes;
        public double? GlancesPM => Glances / Duration?.TotalMinutes;
        public double? MissesPM => Misses / Duration?.TotalMinutes;
        public double? WeaponHitAttemptsPM => WeaponHitAttempts / Duration?.TotalMinutes;
        public double? NanoHitsPM => NanoHits / Duration?.TotalMinutes;
        public double? IndirectHitsPM => IndirectHits / Duration?.TotalMinutes;
        public double? TotalHitsPM => TotalHits / Duration?.TotalMinutes;

        public double? WeaponHitChance => WeaponHits / WeaponHitAttempts.NullIfZero();
        public double? CritChance => Crits / WeaponHitAttempts.NullIfZero();
        public double? GlanceChance => Glances / WeaponHitAttempts.NullIfZero();
        public double? MissChance => Misses / WeaponHitAttempts.NullIfZero();

        public double? AverageWeaponDamage => WeaponDamage / WeaponHits.NullIfZero();
        public double? AverageCritDamage => CritDamage / Crits.NullIfZero();
        public double? AverageGlanceDamage => GlanceDamage / Glances.NullIfZero();
        public double? AverageNanoDamage => NanoDamage / NanoHits.NullIfZero();
        public double? AverageIndirectDamage => IndirectDamage / IndirectHits.NullIfZero();

        public bool HasDamageTypeDamage(DamageType damageType) => FightCharacters.Any(c => c.DamageTypeDamagesDone.ContainsKey(damageType));
        public bool HasSpecials => DamageTypeHelpers.SpecialDamageTypes.Any(HasDamageTypeDamage);
        public int? GetDamageTypeHits(DamageType damageType) => FightCharacters.NullableSum(c => c.GetDamageTypeHitsDone(damageType));
        public long? GetDamageTypeDamage(DamageType damageType) => FightCharacters.NullableSum(c => c.GetDamageTypeDamageDone(damageType));
        public double? GetAverageDamageTypeDamage(DamageType damageType) => GetDamageTypeDamage(damageType) / (double?)GetDamageTypeHits(damageType);
        public double? GetSecondsPerDamageTypeHit(DamageType damageType) => Duration?.TotalSeconds / GetDamageTypeHits(damageType);

        protected long? _maxDamageDone, _maxDamageDonePlusPets;
        public long? MaxDamageDone => _maxDamageDone ?? (_maxDamageDone = FightCharacters.NullableMax(c => c.TotalDamageDone));
        public long? MaxDamageDonePlusPets => _maxDamageDonePlusPets ?? (_maxDamageDonePlusPets = FightCharacters.NullableMax(c => c.TotalDamageDonePlusPets));

        protected long? _maxDamageTaken;
        public long? MaxDamageTaken => _maxDamageTaken ?? (_maxDamageTaken = FightCharacters.NullableMax(c => c.TotalDamageTaken));

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
                    _weaponDamage = _critDamage = _glanceDamage = _nanoDamage = _indirectDamage = null;
                    _weaponHits = _crits = _glances = _misses = _nanoHits = _indirectHits = null;
                    _maxDamageDone = _maxDamageDonePlusPets = null;
                    _maxDamageTaken = null;
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
