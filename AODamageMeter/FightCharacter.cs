using AODamageMeter.FightEvents;
using AODamageMeter.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AODamageMeter
{
    public class FightCharacter
    {
        public FightCharacter(Fight fight, Character character, DateTime enteredTime)
        {
            Fight = fight;
            Character = character;
            EnteredTime = enteredTime;
            _stopwatch = DamageMeter.IsRealTimeMode ? Stopwatch.StartNew() : null;
        }

        // Making FightPetOwner readonly goes back to the discussion in Character. We know by convention pets are recognized immediately.
        // If we guarantee the owner is in the fight if the pet is in the fight, then it simplifies things and offers better support for users
        // who only use their pets w/o engaging in the fight themselves. The simplification is mainly being able to create source/target info
        // pairs for the owner whenever we create one for the pet. Also allows us to simplify the ActiveDuration--the owner has been active
        // for at least as long as his pets, since he has to be constructed before them. To fully support marking pets mid-fight, we'd have
        // to propagate some side effects to the owner. Might become worth it to support bureaucrats who can't rename their charms?
        public FightCharacter(Fight fight, Character pet, DateTime enteredTime, FightCharacter fightPetOwner)
            : this(fight, pet, enteredTime)
        {
            fightPetOwner.Character.RegisterPet(pet); // We do this higher up in our own codebase, but here for completeness.
            fightPetOwner._fightPets.Add(this);
            FightPetOwner = fightPetOwner;
        }

        public DamageMeter DamageMeter => Fight.DamageMeter;
        public Fight Fight { get; }
        public Character Character { get; }
        public bool IsDamageMeterOwner => Character == DamageMeter.Owner;
        public string Name => Character.Name;
        public string UncoloredName => Character.UncoloredName;
        public CharacterType CharacterType => Character.CharacterType;
        public bool IsPlayer => Character.IsPlayer;
        public bool IsNPC => Character.IsNPC;
        public bool IsPet => Character.IsPet;
        public string ID => Character.ID;
        public Profession Profession => Character.Profession;
        public Breed? Breed => Character.Breed;
        public Gender? Gender => Character.Gender;
        public Faction? Faction => Character.Faction;
        public int? Level => Character.Level;
        public int? AlienLevel => Character.AlienLevel;
        public bool HasPlayerInfo => Character.HasPlayerInfo;
        public string Organization => Character.Organization;
        public string OrganizationRank => Character.OrganizationRank;
        public bool HasOrganizationInfo => Character.HasOrganizationInfo;

        public DateTime EnteredTime { get; }

        protected Stopwatch _stopwatch;
        public TimeSpan ActiveDuration => DamageMeter.IsRealTimeMode ? _stopwatch.Elapsed : Fight.LatestEventTime.Value - EnteredTime;

        protected bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (DamageMeter.IsParsedTimeMode && !value) return;
                if (DamageMeter.IsParsedTimeMode) throw new NotSupportedException("Pausing for parsed-time meters isn't supported yet.");
                if (!Fight.IsPaused && value) throw new NotSupportedException("Pausing a character while the fight is unpaused isn't supported yet.");
                if (Fight.IsPaused && !value) throw new InvalidOperationException("Unpausing a character while the fight is paused doesn't make sense.");

                _isPaused = value;
                if (IsPaused) _stopwatch.Stop();
                else _stopwatch.Start();
            }
        }

        public FightCharacter FightPetOwner { get; }
        protected readonly HashSet<FightCharacter> _fightPets = new HashSet<FightCharacter>();
        public IReadOnlyCollection<FightCharacter> FightPets => _fightPets;
        public bool IsFightPetOwner => FightPets.Count != 0;
        public bool IsFightPet => FightPetOwner != null;

        public long WeaponDamageDone { get; protected set; }
        public long CritDamageDone { get; protected set; }
        public long GlanceDamageDone { get; protected set; }
        public long NanoDamageDone { get; protected set; }
        public long IndirectDamageDone { get; protected set; }
        public long TotalDamageDone => WeaponDamageDone + NanoDamageDone + IndirectDamageDone;

        public long WeaponDamageDonePlusPets => WeaponDamageDone + FightPets.Sum(p => p.WeaponDamageDone);
        public long CritDamageDonePlusPets => CritDamageDone + FightPets.Sum(p => p.CritDamageDone);
        public long GlanceDamageDonePlusPets => GlanceDamageDone + FightPets.Sum(p => p.GlanceDamageDone);
        public long NanoDamageDonePlusPets => NanoDamageDone + FightPets.Sum(p => p.NanoDamageDone);
        public long IndirectDamageDonePlusPets => IndirectDamageDone + FightPets.Sum(p => p.IndirectDamageDone);
        public long TotalDamageDonePlusPets => TotalDamageDone + FightPets.Sum(p => p.TotalDamageDone);
        public long OwnersOrOwnTotalDamageDonePlusPets => FightPetOwner?.TotalDamageDonePlusPets ?? TotalDamageDonePlusPets;

        public double WeaponDamageDonePS => ActiveDuration.TotalSeconds <= 1 ? WeaponDamageDone : WeaponDamageDone / ActiveDuration.TotalSeconds;
        public double NanoDamageDonePS => ActiveDuration.TotalSeconds <= 1 ? NanoDamageDone : NanoDamageDone / ActiveDuration.TotalSeconds;
        public double IndirectDamageDonePS => ActiveDuration.TotalSeconds <= 1 ? IndirectDamageDone : IndirectDamageDone / ActiveDuration.TotalSeconds;
        public double TotalDamageDonePS => ActiveDuration.TotalSeconds <= 1 ? TotalDamageDone : TotalDamageDone / ActiveDuration.TotalSeconds;

        public double WeaponDamageDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? WeaponDamageDonePlusPets : WeaponDamageDonePlusPets / ActiveDuration.TotalSeconds;
        public double NanoDamageDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? NanoDamageDonePlusPets : NanoDamageDonePlusPets / ActiveDuration.TotalSeconds;
        public double IndirectDamageDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? IndirectDamageDonePlusPets : IndirectDamageDonePlusPets / ActiveDuration.TotalSeconds;
        public double TotalDamageDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? TotalDamageDonePlusPets : TotalDamageDonePlusPets / ActiveDuration.TotalSeconds;

        public double WeaponDamageDonePM => 60 * WeaponDamageDonePS;
        public double NanoDamageDonePM => 60 * NanoDamageDonePS;
        public double IndirectDamageDonePM => 60 * IndirectDamageDonePS;
        public double TotalDamageDonePM => 60 * TotalDamageDonePS;

        public double WeaponDamageDonePMPlusPets => 60 * WeaponDamageDonePSPlusPets;
        public double NanoDamageDonePMPlusPets => 60 * NanoDamageDonePSPlusPets;
        public double IndirectDamageDonePMPlusPets => 60 * IndirectDamageDonePSPlusPets;
        public double TotalDamageDonePMPlusPets => 60 * TotalDamageDonePSPlusPets;

        public double WeaponPercentOfTotalDamageDone => TotalDamageDone == 0 ? 0 : WeaponDamageDone / (double)TotalDamageDone;
        public double NanoPercentOfTotalDamageDone => TotalDamageDone == 0 ? 0 : NanoDamageDone / (double)TotalDamageDone;
        public double IndirectPercentOfTotalDamageDone => TotalDamageDone == 0 ? 0 : IndirectDamageDone / (double)TotalDamageDone;

        public double WeaponPercentOfTotalDamageDonePlusPets => TotalDamageDonePlusPets == 0 ? 0 : WeaponDamageDonePlusPets / (double)TotalDamageDonePlusPets;
        public double NanoPercentOfTotalDamageDonePlusPets => TotalDamageDonePlusPets == 0 ? 0 : NanoDamageDonePlusPets / (double)TotalDamageDonePlusPets;
        public double IndirectPercentOfTotalDamageDonePlusPets => TotalDamageDonePlusPets == 0 ? 0 : IndirectDamageDonePlusPets / (double)TotalDamageDonePlusPets;

        public int WeaponHitsDone { get; protected set; }
        public int CritsDone { get; protected set; }
        public int GlancesDone { get; protected set; }
        public int MissesDone { get; protected set; } // We only know about misses where the owner is a source or target.
        public int WeaponHitAttemptsDone => WeaponHitsDone + MissesDone;
        public int NanoHitsDone { get; protected set; }
        public int IndirectHitsDone { get; protected set; }
        public int TotalHitsDone => WeaponHitsDone + NanoHitsDone + IndirectHitsDone;

        public int WeaponHitsDonePlusPets => WeaponHitsDone + FightPets.Sum(p => p.WeaponHitsDone);
        public int CritsDonePlusPets => CritsDone + FightPets.Sum(p => p.CritsDone);
        public int GlancesDonePlusPets => GlancesDone + FightPets.Sum(p => p.GlancesDone);
        public int MissesDonePlusPets => MissesDone + FightPets.Sum(p => p.MissesDone);
        public int WeaponHitAttemptsDonePlusPets => WeaponHitAttemptsDone + FightPets.Sum(p => p.WeaponHitAttemptsDone);
        public int NanoHitsDonePlusPets => NanoHitsDone + FightPets.Sum(p => p.NanoHitsDone); 
        public int IndirectHitsDonePlusPets => IndirectHitsDone + FightPets.Sum(p => p.IndirectHitsDone);
        public int TotalHitsDonePlusPets => TotalHitsDone + FightPets.Sum(p => p.TotalHitsDone);

        public double WeaponHitsDonePS => ActiveDuration.TotalSeconds <= 1 ? WeaponHitsDone : WeaponHitsDone / ActiveDuration.TotalSeconds;
        public double CritsDonePS => ActiveDuration.TotalSeconds <= 1 ? CritsDone : CritsDone / ActiveDuration.TotalSeconds;
        public double GlancesDonePS => ActiveDuration.TotalSeconds <= 1 ? GlancesDone : GlancesDone / ActiveDuration.TotalSeconds;
        public double MissesDonePS => ActiveDuration.TotalSeconds <= 1 ? MissesDone : MissesDone / ActiveDuration.TotalSeconds;
        public double WeaponHitAttemptsDonePS => ActiveDuration.TotalSeconds <= 1 ? WeaponHitAttemptsDone : WeaponHitAttemptsDone / ActiveDuration.TotalSeconds;
        public double NanoHitsDonePS => ActiveDuration.TotalSeconds <= 1 ? NanoHitsDone : NanoHitsDone / ActiveDuration.TotalSeconds;
        public double IndirectHitsDonePS => ActiveDuration.TotalSeconds <= 1 ? IndirectHitsDone : IndirectHitsDone / ActiveDuration.TotalSeconds;
        public double TotalHitsDonePS => ActiveDuration.TotalSeconds <= 1 ? TotalHitsDone : TotalHitsDone / ActiveDuration.TotalSeconds;

        public double WeaponHitsDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? WeaponHitsDonePlusPets : WeaponHitsDonePlusPets / ActiveDuration.TotalSeconds;
        public double CritsDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? CritsDonePlusPets : CritsDonePlusPets / ActiveDuration.TotalSeconds;
        public double GlancesDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? GlancesDonePlusPets : GlancesDonePlusPets / ActiveDuration.TotalSeconds;
        public double MissesDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? MissesDonePlusPets : MissesDonePlusPets / ActiveDuration.TotalSeconds;
        public double WeaponHitAttemptsDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? WeaponHitAttemptsDonePlusPets : WeaponHitAttemptsDonePlusPets / ActiveDuration.TotalSeconds;
        public double NanoHitsDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? NanoHitsDonePlusPets : NanoHitsDonePlusPets / ActiveDuration.TotalSeconds;
        public double IndirectHitsDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? IndirectHitsDonePlusPets : IndirectHitsDonePlusPets / ActiveDuration.TotalSeconds;
        public double TotalHitsDonePSPlusPets => ActiveDuration.TotalSeconds <= 1 ? TotalHitsDonePlusPets : TotalHitsDonePlusPets / ActiveDuration.TotalSeconds;

        public double WeaponHitsDonePM => 60 * WeaponHitsDonePS;
        public double CritsDonePM => 60 * CritsDonePS;
        public double GlancesDonePM => 60 * GlancesDonePS;
        public double MissesDonePM => 60 * MissesDonePS;
        public double WeaponHitAttemptsDonePM => 60 * WeaponHitAttemptsDonePS;
        public double NanoHitsDonePM => 60 * NanoHitsDonePS;
        public double IndirectHitsDonePM => 60 * IndirectHitsDonePS;
        public double TotalHitsDonePM => 60 * TotalHitsDonePS;

        public double WeaponHitsDonePMPlusPets => 60 * WeaponHitsDonePSPlusPets;
        public double CritsDonePMPlusPets => 60 * CritsDonePSPlusPets;
        public double GlancesDonePMPlusPets => 60 * GlancesDonePSPlusPets;
        public double MissesDonePMPlusPets => 60 * MissesDonePSPlusPets;
        public double WeaponHitAttemptsDonePMPlusPets => 60 * WeaponHitAttemptsDonePSPlusPets;
        public double NanoHitsDonePMPlusPets => 60 * NanoHitsDonePSPlusPets;
        public double IndirectHitsDonePMPlusPets => 60 * IndirectHitsDonePSPlusPets;
        public double TotalHitsDonePMPlusPets => 60 * TotalHitsDonePSPlusPets;

        public double WeaponHitDoneChance => WeaponHitAttemptsDone == 0 ? 0 : WeaponHitsDone / (double)WeaponHitAttemptsDone;
        public double CritDoneChance => WeaponHitAttemptsDone == 0 ? 0 : CritsDone / (double)WeaponHitAttemptsDone;
        public double GlanceDoneChance => WeaponHitAttemptsDone == 0 ? 0 : GlancesDone / (double)WeaponHitAttemptsDone;
        public double MissDoneChance => WeaponHitAttemptsDone == 0 ? 0 : MissesDone / (double)WeaponHitAttemptsDone;

        public double WeaponHitDoneChancePlusPets => WeaponHitAttemptsDonePlusPets == 0 ? 0 : WeaponHitsDonePlusPets / (double)WeaponHitAttemptsDonePlusPets;
        public double CritDoneChancePlusPets => WeaponHitAttemptsDonePlusPets == 0 ? 0 : CritsDonePlusPets / (double)WeaponHitAttemptsDonePlusPets;
        public double GlanceDoneChancePlusPets => WeaponHitAttemptsDonePlusPets == 0 ? 0 : GlancesDonePlusPets / (double)WeaponHitAttemptsDonePlusPets;
        public double MissDoneChancePlusPets => WeaponHitAttemptsDonePlusPets == 0 ? 0 : MissesDonePlusPets / (double)WeaponHitAttemptsDonePlusPets;

        public double AverageWeaponDamageDone => WeaponHitsDone == 0 ? 0 : WeaponDamageDone / (double)WeaponHitsDone;
        public double AverageCritDamageDone => CritsDone == 0 ? 0 : CritDamageDone / (double)CritsDone;
        public double AverageGlanceDamageDone => GlancesDone == 0 ? 0 : GlanceDamageDone / (double)GlancesDone;
        public double AverageNanoDamageDone => NanoHitsDone == 0 ? 0 : NanoDamageDone / (double)NanoHitsDone;
        public double AverageIndirectDamageDone => IndirectHitsDone == 0 ? 0 : IndirectDamageDone / (double)IndirectHitsDone;

        public double AverageWeaponDamageDonePlusPets => WeaponHitsDonePlusPets == 0 ? 0 : WeaponDamageDonePlusPets / (double)WeaponHitsDonePlusPets;
        public double AverageCritDamageDonePlusPets => CritsDonePlusPets == 0 ? 0 : CritDamageDonePlusPets / (double)CritsDonePlusPets;
        public double AverageGlanceDamageDonePlusPets => GlancesDonePlusPets == 0 ? 0 : GlanceDamageDonePlusPets / (double)GlancesDonePlusPets;
        public double AverageNanoDamageDonePlusPets => NanoHitsDonePlusPets == 0 ? 0 : NanoDamageDonePlusPets / (double)NanoHitsDonePlusPets;
        public double AverageIndirectDamageDonePlusPets => IndirectHitsDonePlusPets == 0 ? 0 : IndirectDamageDonePlusPets / (double)IndirectHitsDonePlusPets;

        public double PercentOfOwnersOrOwnTotalDamageDonePlusPets => OwnersOrOwnTotalDamageDonePlusPets == 0 ? 0 : TotalDamageDone / (double)OwnersOrOwnTotalDamageDonePlusPets;

        public double PercentOfFightsTotalDamageDone => Fight.TotalDamageDone == 0 ? 0 : TotalDamageDone / (double)Fight.TotalDamageDone;
        public double PercentOfFightsMaxDamageDone => Fight.MaxDamageDone == 0 ? 0 : TotalDamageDone / (double)Fight.MaxDamageDone;
        public double PercentOfFightsMaxDamageDonePlusPets => Fight.MaxDamageDonePlusPets == 0 ? 0 : TotalDamageDone / (double)Fight.MaxDamageDonePlusPets;

        public double PercentPlusPetsOfFightsTotalDamageDone => Fight.TotalDamageDone == 0 ? 0 : TotalDamageDonePlusPets / (double)Fight.TotalDamageDone;
        public double PercentPlusPetsOfFightsMaxDamageDonePlusPets => Fight.MaxDamageDonePlusPets == 0 ? 0 : TotalDamageDonePlusPets / (double)Fight.MaxDamageDonePlusPets;

        protected readonly Dictionary<FightCharacter, DamageInfo> _damageDoneInfosByTarget = new Dictionary<FightCharacter, DamageInfo>();
        public IReadOnlyDictionary<FightCharacter, DamageInfo> DamageDoneInfosByTarget => _damageDoneInfosByTarget;
        public IReadOnlyCollection<DamageInfo> DamageDoneInfos => _damageDoneInfosByTarget.Values;

        protected bool _isMaxDamageDoneCurrent;
        protected long _maxDamageDone;
        public long MaxDamageDone
        {
            get
            {
                if (!_isMaxDamageDoneCurrent)
                {
                    _maxDamageDone = DamageDoneInfos.Any() ? DamageDoneInfos.Max(i => i.TotalDamage) : 0;
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
                    _maxDamageDonePlusPets = DamageDoneInfos.Any() ? DamageDoneInfos.Max(i => i.TotalDamagePlusPets) : 0;
                    _isMaxDamageDonePlusPetsCurrent = true;
                }

                return _maxDamageDonePlusPets;
            }
        }

        protected readonly Dictionary<DamageType, int> _damageTypeHitsDone = new Dictionary<DamageType, int>();
        protected readonly Dictionary<DamageType, long> _damageTypeDamagesDone = new Dictionary<DamageType, long>();
        public IReadOnlyDictionary<DamageType, int> DamageTypeHitsDone => _damageTypeHitsDone;
        public IReadOnlyDictionary<DamageType, long> DamageTypeDamagesDone => _damageTypeDamagesDone;

        public bool HasDamageTypeDamageDone(DamageType damageType)
            => DamageTypeDamagesDone.ContainsKey(damageType);

        public bool HasSpecialsDone
            => DamageTypeHelpers.SpecialDamageTypes.Any(HasDamageTypeDamageDone);

        public int? GetAverageDamageTypeDamageDone(DamageType damageType)
            => DamageTypeDamagesDone.TryGetValue(damageType, out long damageTypeDamageDone)
            ? (int?)(damageTypeDamageDone / DamageTypeHitsDone[damageType]) : null;

        public double? GetSecondsPerDamageTypeHitDone(DamageType damageType) // For special damage types this approximates the 'recharge'.
            => DamageTypeHitsDone.TryGetValue(damageType, out int damageTypeHitsDone)
            ? ActiveDuration.TotalSeconds / damageTypeHitsDone : (double?)null;

        public long WeaponDamageTaken { get; protected set; }
        public long CritDamageTaken { get; protected set; }
        public long GlanceDamageTaken { get; protected set; }
        public long NanoDamageTaken { get; protected set; }
        public long IndirectDamageTaken { get; protected set; }
        public long TotalDamageTaken => WeaponDamageTaken + NanoDamageTaken + IndirectDamageTaken;
        public long DamageAbsorbed { get; protected set; }

        public int WeaponHitsTaken { get; protected set; }
        public int CritsTaken { get; protected set; }
        public int GlancesTaken { get; protected set; }
        public int MissesTaken { get; protected set; } // We only know about misses where the owner is a source or target.
        public int WeaponHitAttemptsTaken => WeaponHitsTaken + MissesTaken;
        public int NanoHitsTaken { get; protected set; }
        public int IndirectHitsTaken { get; protected set; }
        public int TotalHitsTaken => WeaponHitsTaken + NanoHitsTaken + IndirectHitsTaken;
        public int HitsAbsorbed { get; protected set; }

        public double WeaponHitTakenChance => WeaponHitAttemptsTaken == 0 ? 0 : WeaponHitsTaken / (double)WeaponHitAttemptsTaken;
        public double CritTakenChance => WeaponHitAttemptsTaken == 0 ? 0 : CritsTaken / (double)WeaponHitAttemptsTaken;
        public double GlanceTakenChance => WeaponHitAttemptsTaken == 0 ? 0 : GlancesTaken / (double)WeaponHitAttemptsTaken;
        public double MissTakenChance => WeaponHitAttemptsTaken == 0 ? 0 : MissesTaken / (double)WeaponHitAttemptsTaken;

        protected readonly Dictionary<FightCharacter, DamageInfo> _damageTakenInfosBySource = new Dictionary<FightCharacter, DamageInfo>();
        public IReadOnlyDictionary<FightCharacter, DamageInfo> DamageTakenInfosBySource => _damageTakenInfosBySource;
        public IReadOnlyCollection<DamageInfo> DamageTakenInfos => _damageTakenInfosBySource.Values;

        protected bool _isMaxDamageTakenCurrent;
        protected long _maxDamageTaken;
        public long MaxDamageTaken
        {
            get
            {
                if (!_isMaxDamageTakenCurrent)
                {
                    _maxDamageTaken = DamageTakenInfos.Any() ? DamageTakenInfos.Max(i => i.TotalDamage) : 0;
                    _isMaxDamageTakenCurrent = true;
                }

                return _maxDamageTaken;
            }
        }

        // Intentionally weird naming. It's not max (this + pets have taken from a source), it's max (this has taken from a source + pets).
        protected bool _isMaxDamagePlusPetsTakenCurrent;
        protected long _maxDamagePlusPetsTaken;
        public long MaxDamagePlusPetsTaken
        {
            get
            {
                if (!_isMaxDamagePlusPetsTakenCurrent)
                {
                    _maxDamagePlusPetsTaken = DamageTakenInfosBySource.Any() ? DamageTakenInfosBySource.Values.Max(i => i.TotalDamagePlusPets) : 0;
                    _isMaxDamagePlusPetsTakenCurrent = true;
                }

                return _maxDamagePlusPetsTaken;
            }
        }

        protected readonly Dictionary<DamageType, int> _damageTypeHitsTaken = new Dictionary<DamageType, int>();
        protected readonly Dictionary<DamageType, long> _damageTypeDamagesTaken = new Dictionary<DamageType, long>();
        public IReadOnlyDictionary<DamageType, int> DamageTypeHitsTaken => _damageTypeHitsTaken;
        public IReadOnlyDictionary<DamageType, long> DamageTypeDamagesTaken => _damageTypeDamagesTaken;

        public bool HasDamageTypeDamageTaken(DamageType damageType)
            => DamageTypeDamagesTaken.ContainsKey(damageType);

        public bool HasSpecialsTaken
            => DamageTypeHelpers.SpecialDamageTypes.Any(HasDamageTypeDamageTaken);

        public int? GetAverageDamageTypeDamageTaken(DamageType damageType)
            => DamageTypeDamagesTaken.TryGetValue(damageType, out long damageTypeDamageTaken)
            ? (int?)(damageTypeDamageTaken / DamageTypeHitsTaken[damageType]) : null;

        public double? GetSecondsPerDamageTypeHitTaken(DamageType damageType) // For special damage types this approximates the 'recharge'.
            => DamageTypeHitsTaken.TryGetValue(damageType, out int damageTypeHitsTaken)
            ? ActiveDuration.TotalSeconds / damageTypeHitsTaken : (double?)null;

        // We only know about healing where the owner is a source or target. When the owner is the source, we don't know
        // about realized healing. So overhealing stats only when non-owner source and owner target--non-owner source has
        // OverhealingDone (to owner) stats, owner target has OverhealingTaken (from non-owners) stats. Since owner must
        // be source or target, it follows we only have SelfHealingDone for the owner.
        public long SelfHealingDone { get; protected set; }
        public long PotentialHealingDone { get; protected set; }
        public long RealizedHealingDone { get; protected set; }
        public long OverhealingDone { get; protected set; }
        public long NanoHealingDone { get; protected set; }
        public long PotentialHealingTaken { get; protected set; }
        public long RealizedHealingTaken { get; protected set; }
        public long OverhealingTaken { get; protected set; }
        public long NanoHealingTaken { get; protected set; }

        // We only know about level events where the source is the owner (there's no target).
        public long NormalXPGained { get; protected set; }
        public int ShadowXPGained { get; protected set; }
        public int AlienXPGained { get; protected set; }
        public long ResearchXPGained { get; protected set; }
        public int PvpDuelXPGained { get; protected set; }
        public int PvpSoloXPGained { get; protected set; }
        public int PvpTeamXPGained { get; protected set; }

        // We only know about nano events where the source is the owner (there's no target).
        public int CastAttempts => CastSuccessCount + CastResistedCount + CastCounteredCount + CastAbortedCount;
        public int CastSuccessCount { get; protected set; }
        public int CastResistedCount { get; protected set; }
        public int CastCounteredCount { get; protected set; }
        public int CastAbortedCount { get; protected set; }
        public double CastSuccessChance => CastAttempts == 0 ? 0 : CastSuccessCount / (double)CastAttempts;
        public double CastResistedChance => CastAttempts == 0 ? 0 : CastResistedCount / (double)CastAttempts;
        public double CastCounteredChance => CastAttempts == 0 ? 0 : CastCounteredCount / (double)CastAttempts;
        public double CastAbortedChance => CastAttempts == 0 ? 0 : CastAbortedCount / (double)CastAttempts;
        public int CastUnavailableCount { get; protected set; }

        public void AddSourceAttackEvent(AttackEvent attackEvent)
        {
            switch (attackEvent.AttackResult)
            {
                case AttackResult.WeaponHit:
                    WeaponDamageDone += attackEvent.Amount.Value;
                    ++WeaponHitsDone;
                    if (attackEvent.AttackModifier == AttackModifier.Crit)
                    {
                        CritDamageDone += attackEvent.Amount.Value;
                        ++CritsDone;
                    }
                    else if (attackEvent.AttackModifier == AttackModifier.Glance)
                    {
                        GlanceDamageDone += attackEvent.Amount.Value;
                        ++GlancesDone;
                    }
                    break;
                case AttackResult.Missed:
                    ++MissesDone;
                    break;
                case AttackResult.NanoHit:
                    NanoDamageDone += attackEvent.Amount.Value;
                    ++NanoHitsDone;
                    break;
                case AttackResult.IndirectHit:
                    IndirectDamageDone += attackEvent.Amount.Value;
                    ++IndirectHitsDone;
                    break;
                // No sources for events where the attack results in an absorb.
                default: throw new NotImplementedException();
            }

            if (attackEvent.Target != null)
            {
                if (_damageDoneInfosByTarget.TryGetValue(attackEvent.Target, out DamageInfo damageInfo))
                {
                    damageInfo.AddAttackEvent(attackEvent);
                }
                else
                {
                    if (IsFightPet && !FightPetOwner._damageDoneInfosByTarget.ContainsKey(attackEvent.Target))
                    {
                        var fightPetOwnerDamageInfo = new DamageInfo(FightPetOwner, attackEvent.Target);
                        FightPetOwner._damageDoneInfosByTarget[attackEvent.Target] = fightPetOwnerDamageInfo;
                        attackEvent.Target._damageTakenInfosBySource[FightPetOwner] = fightPetOwnerDamageInfo;
                    }

                    damageInfo = new DamageInfo(this, attackEvent.Target);
                    damageInfo.AddAttackEvent(attackEvent);
                    this._damageDoneInfosByTarget[attackEvent.Target] = damageInfo;
                    attackEvent.Target._damageTakenInfosBySource[this] = damageInfo;
                }
                _isMaxDamageDoneCurrent = false;
                _isMaxDamageDonePlusPetsCurrent = false;
                attackEvent.Target._isMaxDamageTakenCurrent = false;
                attackEvent.Target._isMaxDamagePlusPetsTakenCurrent = false;
            }

            if (attackEvent.DamageType.HasValue)
            {
                _damageTypeHitsDone.Increment(attackEvent.DamageType.Value, 1);
                _damageTypeDamagesDone.Increment(attackEvent.DamageType.Value, attackEvent.Amount ?? 0);
            }
        }

        public void AddTargetAttackEvent(AttackEvent attackEvent)
        {
            switch (attackEvent.AttackResult)
            {
                case AttackResult.WeaponHit:
                    WeaponDamageTaken += attackEvent.Amount.Value;
                    ++WeaponHitsTaken;
                    if (attackEvent.AttackModifier == AttackModifier.Crit)
                    {
                        CritDamageTaken += attackEvent.Amount.Value;
                        ++CritsTaken;
                    }
                    else if (attackEvent.AttackModifier == AttackModifier.Glance)
                    {
                        GlanceDamageTaken += attackEvent.Amount.Value;
                        ++GlancesTaken;
                    }
                    break;
                case AttackResult.Missed:
                    ++MissesTaken;
                    break;
                case AttackResult.NanoHit:
                    NanoDamageTaken += attackEvent.Amount.Value;
                    ++NanoHitsTaken;
                    break;
                case AttackResult.IndirectHit:
                    IndirectDamageTaken += attackEvent.Amount.Value;
                    ++IndirectHitsTaken;
                    break;
                case AttackResult.Absorbed:
                    DamageAbsorbed += attackEvent.Amount.Value;
                    ++HitsAbsorbed;
                    break;
                default: throw new NotImplementedException();
            }

            if (attackEvent.DamageType.HasValue)
            {
                _damageTypeHitsTaken.Increment(attackEvent.DamageType.Value, 1);
                _damageTypeDamagesTaken.Increment(attackEvent.DamageType.Value, attackEvent.Amount ?? 0);
            }
        }

        public void AddSelfHealEvent(HealEvent healEvent)
        {
            if (healEvent.Source != healEvent.Target)
                throw new ArgumentException("Use AddSourceHealEvent and AddTargetHealEvent when the source and target differ.");

            if (healEvent.HealType == HealType.RealizedHealth)
            {
                SelfHealingDone += healEvent.Amount.Value;
            }
            else throw new NotImplementedException();
        }

        public void AddSourceHealEvent(HealEvent healEvent)
        {
            if (healEvent.Source == healEvent.Target)
                throw new ArgumentException("Use AddSelfHealEvent for heal events where the source equals the target.");

            if (healEvent.HealType == HealType.PotentialHealth)
            {
                PotentialHealingDone += healEvent.Amount.Value;
            }
            else if (healEvent.HealType == HealType.RealizedHealth)
            {
                RealizedHealingDone += healEvent.Amount.Value;

                // Right now, realized healing where the source (!= owner) is healing the owner is the only way to
                // get here. In that case we know that the heal event must be an end event w/ a corresponding start event.
                OverhealingDone += healEvent.StartEvent.Amount.Value - healEvent.Amount.Value;
            }
            else if (healEvent.HealType == HealType.Nano)
            {
                NanoHealingDone += healEvent.Amount.Value;
            }
            else throw new NotImplementedException();
        }

        public void AddTargetHealEvent(HealEvent healEvent)
        {
            if (healEvent.Source == healEvent.Target)
                throw new ArgumentException("Use AddSelfHealEvent for heal events where the source equals the target.");

            if (healEvent.HealType == HealType.PotentialHealth)
            {
                PotentialHealingTaken += healEvent.Amount.Value;
            }
            else if (healEvent.HealType == HealType.RealizedHealth)
            {
                RealizedHealingTaken += healEvent.Amount.Value;

                // Right now, realized healing where the source (!= owner) is healing the owner is the only way to
                // get here. In that case we know that the heal event must be an end event w/ a corresponding start event.
                OverhealingTaken += healEvent.StartEvent.Amount.Value - healEvent.Amount.Value;
            }
            else if (healEvent.HealType == HealType.Nano)
            {
                NanoHealingTaken += healEvent.Amount.Value;
            }
            else throw new NotImplementedException();
        }

        public void AddLevelEvent(LevelEvent levelEvent)
        {
            switch (levelEvent.LevelType)
            {
                case LevelType.Normal: NormalXPGained += levelEvent.Amount.Value; break;
                case LevelType.Shadow: ShadowXPGained += levelEvent.Amount.Value; break;
                case LevelType.Alien: AlienXPGained += levelEvent.Amount.Value; break;
                case LevelType.Research: ResearchXPGained += levelEvent.Amount.Value; break;
                case LevelType.PvpDuel: PvpDuelXPGained += levelEvent.Amount.Value; break;
                case LevelType.PvpSolo: PvpSoloXPGained += levelEvent.Amount.Value; break;
                case LevelType.PvpTeam: PvpTeamXPGained += levelEvent.Amount.Value; break;
                default: throw new NotImplementedException();
            }
        }

        public void AddNanoEvent(NanoEvent nanoEvent)
        {
            if (nanoEvent.IsEndOfCast)
            {
                switch (nanoEvent.CastResult.Value)
                {
                    case CastResult.Success: ++CastSuccessCount; break;
                    case CastResult.Resisted: ++CastResistedCount; break;
                    case CastResult.Countered: ++CastCounteredCount; break;
                    case CastResult.Aborted: ++CastAbortedCount; break;
                    default: throw new NotImplementedException();
                }
            }
            else if (nanoEvent.IsCastUnavailable)
            {
                ++CastUnavailableCount;
            }
            else if (!nanoEvent.IsStartOfCast && !nanoEvent.IsEndOfCast)
            {
                // Nothing to do here, this is how events that may eventually become start events comes in.
            }
            else throw new NotImplementedException();
        }

        public override string ToString()
            => $"{Character}: {PercentOfFightsTotalDamageDone:P1} of total damage.";
    }
}
