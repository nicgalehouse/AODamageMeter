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

        public double WeaponDamageDonePM => WeaponDamageDone / ActiveDuration.TotalMinutes;
        public double NanoDamageDonePM => NanoDamageDone / ActiveDuration.TotalMinutes;
        public double IndirectDamageDonePM => IndirectDamageDone / ActiveDuration.TotalMinutes;
        public double TotalDamageDonePM => TotalDamageDone / ActiveDuration.TotalMinutes;

        public double WeaponDamageDonePMPlusPets => WeaponDamageDonePlusPets / ActiveDuration.TotalMinutes;
        public double NanoDamageDonePMPlusPets => NanoDamageDonePlusPets / ActiveDuration.TotalMinutes;
        public double IndirectDamageDonePMPlusPets => IndirectDamageDonePlusPets / ActiveDuration.TotalMinutes;
        public double TotalDamageDonePMPlusPets => TotalDamageDonePlusPets / ActiveDuration.TotalMinutes;

        public double? WeaponPercentOfTotalDamageDone => WeaponDamageDone / TotalDamageDone.NullIfZero();
        public double? NanoPercentOfTotalDamageDone => NanoDamageDone / TotalDamageDone.NullIfZero();
        public double? IndirectPercentOfTotalDamageDone => IndirectDamageDone / TotalDamageDone.NullIfZero();

        public double? WeaponPercentOfTotalDamageDonePlusPets => WeaponDamageDonePlusPets / TotalDamageDonePlusPets.NullIfZero();
        public double? NanoPercentOfTotalDamageDonePlusPets => NanoDamageDonePlusPets / TotalDamageDonePlusPets.NullIfZero();
        public double? IndirectPercentOfTotalDamageDonePlusPets => IndirectDamageDonePlusPets / TotalDamageDonePlusPets.NullIfZero();

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

        public double WeaponHitsDonePM => WeaponHitsDone / ActiveDuration.TotalMinutes;
        public double CritsDonePM => CritsDone / ActiveDuration.TotalMinutes;
        public double GlancesDonePM => GlancesDone / ActiveDuration.TotalMinutes;
        public double MissesDonePM => MissesDone / ActiveDuration.TotalMinutes;
        public double WeaponHitAttemptsDonePM => WeaponHitAttemptsDone / ActiveDuration.TotalMinutes;
        public double NanoHitsDonePM => NanoHitsDone / ActiveDuration.TotalMinutes;
        public double IndirectHitsDonePM => IndirectHitsDone / ActiveDuration.TotalMinutes;
        public double TotalHitsDonePM => TotalHitsDone / ActiveDuration.TotalMinutes;

        public double WeaponHitsDonePMPlusPets => WeaponHitsDonePlusPets / ActiveDuration.TotalMinutes;
        public double CritsDonePMPlusPets => CritsDonePlusPets / ActiveDuration.TotalMinutes;
        public double GlancesDonePMPlusPets => GlancesDonePlusPets / ActiveDuration.TotalMinutes;
        public double MissesDonePMPlusPets => MissesDonePlusPets / ActiveDuration.TotalMinutes;
        public double WeaponHitAttemptsDonePMPlusPets => WeaponHitAttemptsDonePlusPets / ActiveDuration.TotalMinutes;
        public double NanoHitsDonePMPlusPets => NanoHitsDonePlusPets / ActiveDuration.TotalMinutes;
        public double IndirectHitsDonePMPlusPets => IndirectHitsDonePlusPets / ActiveDuration.TotalMinutes;
        public double TotalHitsDonePMPlusPets => TotalHitsDonePlusPets / ActiveDuration.TotalMinutes;

        public double? WeaponHitDoneChance => WeaponHitsDone / WeaponHitAttemptsDone.NullIfZero();
        public double? CritDoneChance => CritsDone / WeaponHitsDone.NullIfZero();
        public double? GlanceDoneChance => GlancesDone / WeaponHitsDone.NullIfZero();
        public double? MissDoneChance => MissesDone / WeaponHitAttemptsDone.NullIfZero();

        public double? WeaponHitDoneChancePlusPets => WeaponHitsDonePlusPets / WeaponHitAttemptsDonePlusPets.NullIfZero();
        public double? CritDoneChancePlusPets => CritsDonePlusPets / WeaponHitsDonePlusPets.NullIfZero();
        public double? GlanceDoneChancePlusPets => GlancesDonePlusPets / WeaponHitsDonePlusPets.NullIfZero();
        public double? MissDoneChancePlusPets => MissesDonePlusPets / WeaponHitAttemptsDonePlusPets.NullIfZero();

        public double? AverageWeaponDamageDone => WeaponDamageDone / WeaponHitsDone.NullIfZero();
        public double? AverageCritDamageDone => CritDamageDone / CritsDone.NullIfZero();
        public double? AverageGlanceDamageDone => GlanceDamageDone / GlancesDone.NullIfZero();
        public double? AverageNanoDamageDone => NanoDamageDone / NanoHitsDone.NullIfZero();
        public double? AverageIndirectDamageDone => IndirectDamageDone / IndirectHitsDone.NullIfZero();

        public double? AverageWeaponDamageDonePlusPets => WeaponDamageDonePlusPets / WeaponHitsDonePlusPets.NullIfZero();
        public double? AverageCritDamageDonePlusPets => CritDamageDonePlusPets / CritsDonePlusPets.NullIfZero();
        public double? AverageGlanceDamageDonePlusPets => GlanceDamageDonePlusPets / GlancesDonePlusPets.NullIfZero();
        public double? AverageNanoDamageDonePlusPets => NanoDamageDonePlusPets / NanoHitsDonePlusPets.NullIfZero();
        public double? AverageIndirectDamageDonePlusPets => IndirectDamageDonePlusPets / IndirectHitsDonePlusPets.NullIfZero();

        public double? PercentOfOwnersOrOwnTotalDamageDonePlusPets => TotalDamageDone / OwnersOrOwnTotalDamageDonePlusPets.NullIfZero();

        public double? PercentOfFightsTotalDamageDone => TotalDamageDone / Fight.TotalDamageDone.NullIfZero();
        public double? PercentOfFightsTotalPlayerDamageDonePlusPets => TotalDamageDone / Fight.TotalPlayerDamageDonePlusPets.NullIfZero();

        public double? PercentPlusPetsOfFightsTotalDamageDone => TotalDamageDonePlusPets / Fight.TotalDamageDone.NullIfZero();
        public double? PercentPlusPetsOfFightsTotalPlayerDamageDonePlusPets => TotalDamageDonePlusPets / Fight.TotalPlayerDamageDonePlusPets.NullIfZero();

        public double? PercentOfFightsMaxDamageDone => TotalDamageDone / Fight.MaxDamageDone.NullIfZero();
        public double? PercentOfFightsMaxDamageDonePlusPets => TotalDamageDone / Fight.MaxDamageDonePlusPets.NullIfZero();
        public double? PercentOfFightsMaxPlayerDamageDonePlusPets => TotalDamageDone / Fight.MaxPlayerDamageDonePlusPets.NullIfZero();

        public double? PercentPlusPetsOfFightsMaxDamageDonePlusPets => TotalDamageDonePlusPets / Fight.MaxDamageDonePlusPets.NullIfZero();
        public double? PercentPlusPetsOfFightsMaxPlayerDamageDonePlusPets => TotalDamageDonePlusPets / Fight.MaxPlayerDamageDonePlusPets.NullIfZero();

        protected readonly Dictionary<FightCharacter, DamageInfo> _damageDoneInfosByTarget = new Dictionary<FightCharacter, DamageInfo>();
        public IReadOnlyDictionary<FightCharacter, DamageInfo> DamageDoneInfosByTarget => _damageDoneInfosByTarget;
        public IReadOnlyCollection<DamageInfo> DamageDoneInfos => _damageDoneInfosByTarget.Values;

        protected long? _maxDamageDone, _maxDamageDonePlusPets;
        public long? MaxDamageDone => _maxDamageDone ?? (_maxDamageDone = DamageDoneInfos.NullableMax(i => i.TotalDamage));
        public long? MaxDamageDonePlusPets => _maxDamageDonePlusPets ?? (_maxDamageDonePlusPets = DamageDoneInfos.NullableMax(i => i.TotalDamagePlusPets));

        protected readonly Dictionary<DamageType, int> _damageTypeHitsDone = new Dictionary<DamageType, int>();
        protected readonly Dictionary<DamageType, long> _damageTypeDamagesDone = new Dictionary<DamageType, long>();
        public IReadOnlyDictionary<DamageType, int> DamageTypeHitsDone => _damageTypeHitsDone;
        public IReadOnlyDictionary<DamageType, long> DamageTypeDamagesDone => _damageTypeDamagesDone;

        // Only interested in specials right now, and I'm pretty sure pets don't do those, so don't need pet versions of these methods.
        public bool HasDamageTypeDamageDone(DamageType damageType) => DamageTypeDamagesDone.ContainsKey(damageType);
        public bool HasSpecialsDone => DamageTypeHelpers.SpecialDamageTypes.Any(HasDamageTypeDamageDone);
        public int? GetDamageTypeHitsDone(DamageType damageType) => DamageTypeHitsDone.TryGetValue(damageType, out int damageTypeHitsDone) ? damageTypeHitsDone : (int?)null;
        public long? GetDamageTypeDamageDone(DamageType damageType) => DamageTypeDamagesDone.TryGetValue(damageType, out long damageTypeDamageDone) ? damageTypeDamageDone : (long?)null;
        public double? GetAverageDamageTypeDamageDone(DamageType damageType) => GetDamageTypeDamageDone(damageType) / (double?)GetDamageTypeHitsDone(damageType);
        public double? GetSecondsPerDamageTypeHitDone(DamageType damageType) => ActiveDuration.TotalSeconds / GetDamageTypeHitsDone(damageType);

        public long WeaponDamageTaken { get; protected set; }
        public long CritDamageTaken { get; protected set; }
        public long GlanceDamageTaken { get; protected set; }
        public long NanoDamageTaken { get; protected set; }
        public long IndirectDamageTaken { get; protected set; }
        public long TotalDamageTaken => WeaponDamageTaken + NanoDamageTaken + IndirectDamageTaken;
        // Absorbed sucks, only have it for the owner and don't have a source along with it. Keeping it independent of damage taken.
        public long DamageAbsorbed { get; protected set; }

        public double WeaponDamageTakenPM => WeaponDamageTaken / ActiveDuration.TotalMinutes;
        public double NanoDamageTakenPM => NanoDamageTaken / ActiveDuration.TotalMinutes;
        public double IndirectDamageTakenPM => IndirectDamageTaken / ActiveDuration.TotalMinutes;
        public double TotalDamageTakenPM => TotalDamageTaken / ActiveDuration.TotalMinutes;
        public double DamageAbsorbedPM => DamageAbsorbed / ActiveDuration.TotalMinutes;

        public double? WeaponPercentOfTotalDamageTaken => WeaponDamageTaken / TotalDamageTaken.NullIfZero();
        public double? NanoPercentOfTotalDamageTaken => NanoDamageTaken / TotalDamageTaken.NullIfZero();
        public double? IndirectPercentOfTotalDamageTaken => IndirectDamageTaken / TotalDamageTaken.NullIfZero();

        public int WeaponHitsTaken { get; protected set; }
        public int CritsTaken { get; protected set; }
        public int GlancesTaken { get; protected set; }
        public int MissesTaken { get; protected set; } // We only know about misses where the owner is a source or target.
        public int WeaponHitAttemptsTaken => WeaponHitsTaken + MissesTaken;
        public int NanoHitsTaken { get; protected set; }
        public int IndirectHitsTaken { get; protected set; }
        public int TotalHitsTaken => WeaponHitsTaken + NanoHitsTaken + IndirectHitsTaken;
        public int HitsAbsorbed { get; protected set; }

        public double WeaponHitsTakenPM => WeaponHitsTaken / ActiveDuration.TotalMinutes;
        public double CritsTakenPM => CritsTaken / ActiveDuration.TotalMinutes;
        public double GlancesTakenPM => GlancesTaken / ActiveDuration.TotalMinutes;
        public double MissesTakenPM => MissesTaken / ActiveDuration.TotalMinutes;
        public double WeaponHitAttemptsTakenPM => WeaponHitAttemptsTaken / ActiveDuration.TotalMinutes;
        public double NanoHitsTakenPM => NanoHitsTaken / ActiveDuration.TotalMinutes;
        public double IndirectHitsTakenPM => IndirectHitsTaken / ActiveDuration.TotalMinutes;
        public double TotalHitsTakenPM => TotalHitsTaken / ActiveDuration.TotalMinutes;
        public double HitsAbsorbedPM => HitsAbsorbed / ActiveDuration.TotalMinutes;

        public double? WeaponHitTakenChance => WeaponHitsTaken / WeaponHitAttemptsTaken.NullIfZero();
        public double? CritTakenChance => CritsTaken / WeaponHitsTaken.NullIfZero();
        public double? GlanceTakenChance => GlancesTaken / WeaponHitsTaken.NullIfZero();
        public double? MissTakenChance => MissesTaken / WeaponHitAttemptsTaken.NullIfZero();

        public double? AverageWeaponDamageTaken => WeaponDamageTaken / WeaponHitsTaken.NullIfZero();
        public double? AverageCritDamageTaken => CritDamageTaken / CritsTaken.NullIfZero();
        public double? AverageGlanceDamageTaken => GlanceDamageTaken / GlancesTaken.NullIfZero();
        public double? AverageNanoDamageTaken => NanoDamageTaken / NanoHitsTaken.NullIfZero();
        public double? AverageIndirectDamageTaken => IndirectDamageTaken / IndirectHitsTaken.NullIfZero();
        public double? AverageDamageAbsorbed => DamageAbsorbed / HitsAbsorbed.NullIfZero();

        public double? PercentOfFightsTotalDamageTaken => TotalDamageTaken / Fight.TotalDamageTaken.NullIfZero();
        public double? PercentOfFightsTotalPlayerOrPetDamageTaken => TotalDamageTaken / Fight.TotalPlayerOrPetDamageTaken.NullIfZero();

        public double? PercentOfFightsMaxDamageTaken => TotalDamageTaken / Fight.MaxDamageTaken.NullIfZero();
        public double? PercentOfFightsMaxPlayerOrPetDamageTaken => TotalDamageTaken / Fight.MaxPlayerOrPetDamageTaken.NullIfZero();

        protected readonly Dictionary<FightCharacter, DamageInfo> _damageTakenInfosBySource = new Dictionary<FightCharacter, DamageInfo>();
        public IReadOnlyDictionary<FightCharacter, DamageInfo> DamageTakenInfosBySource => _damageTakenInfosBySource;
        public IReadOnlyCollection<DamageInfo> DamageTakenInfos => _damageTakenInfosBySource.Values;

        protected long? _maxDamageTaken, _maxDamagePlusPetsTaken;
        public long? MaxDamageTaken => _maxDamageTaken ?? (_maxDamageTaken = DamageTakenInfos.NullableMax(i => i.TotalDamage));
        // Intentionally weird naming. It's not max (this + pets) have taken from a source, it's max this has taken from a (source + pets).
        public long? MaxDamagePlusPetsTaken => _maxDamagePlusPetsTaken ?? (_maxDamagePlusPetsTaken = DamageTakenInfos.NullableMax(i => i.TotalDamagePlusPets));

        protected readonly Dictionary<DamageType, int> _damageTypeHitsTaken = new Dictionary<DamageType, int>();
        protected readonly Dictionary<DamageType, long> _damageTypeDamagesTaken = new Dictionary<DamageType, long>();
        public IReadOnlyDictionary<DamageType, int> DamageTypeHitsTaken => _damageTypeHitsTaken;
        public IReadOnlyDictionary<DamageType, long> DamageTypeDamagesTaken => _damageTypeDamagesTaken;

        public bool HasDamageTypeDamageTaken(DamageType damageType) => DamageTypeDamagesTaken.ContainsKey(damageType);
        public bool HasSpecialsTaken => DamageTypeHelpers.SpecialDamageTypes.Any(HasDamageTypeDamageTaken);
        public int? GetDamageTypeHitsTaken(DamageType damageType) => DamageTypeHitsTaken.TryGetValue(damageType, out int damageTypeHitsTaken) ? damageTypeHitsTaken : (int?)null;
        public long? GetDamageTypeDamageTaken(DamageType damageType) => DamageTypeDamagesTaken.TryGetValue(damageType, out long damageTypeDamageTaken) ? damageTypeDamageTaken : (long?)null;
        public double? GetAverageDamageTypeDamageTaken(DamageType damageType) => GetDamageTypeDamageTaken(damageType) / (double?)GetDamageTypeHitsTaken(damageType);
        public double? GetSecondsPerDamageTypeHitTaken(DamageType damageType) => ActiveDuration.TotalSeconds / GetDamageTypeHitsTaken(damageType);

        // We only know about misses where the owner is a source or target.
        public bool HasIncompleteMissStats => !IsDamageMeterOwner;
        public bool HasIncompleteMissStatsPlusPets => !IsDamageMeterOwner || FightPets.Any();

        //                    1                    2                3
        // Potential healing: owner --> non-owner, owner -/> owner, non-owner --> owner
        // Realized healing:  owner -/> non-owner, owner --> owner, non-owner --> owner
        // Overhealing:       owner -/> non-owner, owner -/> owner, non-owner --> owner
        // 1. Potential healing for owner to non-owner, but no realized, so can't do overhealing.
        // 2. No potential healing for owner to owner, but realized, so can't do overhealing.
        // 3. Potential healing and realized for non-owner to owner, so can do overhealing.
        // In practice, for the owner's aggregate done stats, what we use corresponds to reality (r) like so:
        // r >= potential healing, r >= realized healing, r >= overhealing (== 0). Not to mention pets...
        // In practice, for the owner's aggregate taken stats, what we use corresponds to reality (r) like so:
        // r >= potential healing, r == realized healing, r >= overhealing.
        public long PotentialHealingDone { get; protected set; }
        public long RealizedHealingDone { get; protected set; }
        public long OverhealingDone { get; protected set; }
        public long NanoHealingDone { get; protected set; }

        // Pets for healing done aren't as important as pets for healing taken. We can only see when the owner's pet heals the owner, not other characters.
        public long PotentialHealingDonePlusPets => PotentialHealingDone + FightPets.Sum(p => p.PotentialHealingDone);
        public long RealizedHealingDonePlusPets => RealizedHealingDone + FightPets.Sum(p => p.RealizedHealingDone);
        public long OverhealingDonePlusPets => OverhealingDone + FightPets.Sum(p => p.OverhealingDone);
        public long NanoHealingDonePlusPets => NanoHealingDone + FightPets.Sum(p => p.NanoHealingDone);

        public double PotentialHealingDonePM => PotentialHealingDone / ActiveDuration.TotalMinutes;
        public double RealizedHealingDonePM => RealizedHealingDone / ActiveDuration.TotalMinutes;
        public double OverhealingDonePM => OverhealingDone / ActiveDuration.TotalMinutes;
        public double NanoHealingDonePM => NanoHealingDone / ActiveDuration.TotalMinutes;

        public double PotentialHealingDonePMPlusPets => PotentialHealingDonePlusPets / ActiveDuration.TotalMinutes;
        public double RealizedHealingDonePMPlusPets => RealizedHealingDonePlusPets / ActiveDuration.TotalMinutes;
        public double OverhealingDonePMPlusPets => OverhealingDonePlusPets / ActiveDuration.TotalMinutes;
        public double NanoHealingDonePMPlusPets => NanoHealingDonePlusPets / ActiveDuration.TotalMinutes;

        public double? PercentOfOverhealingDone => OverhealingDone / PotentialHealingDone.NullIfZero();
        public double? PercentOfOverhealingDonePlusPets => OverhealingDonePlusPets / PotentialHealingDonePlusPets.NullIfZero();

        protected readonly Dictionary<FightCharacter, HealingInfo> _healingDoneInfosByTarget = new Dictionary<FightCharacter, HealingInfo>();
        public IReadOnlyDictionary<FightCharacter, HealingInfo> HealingDoneInfosByTarget => _healingDoneInfosByTarget;
        public IReadOnlyCollection<HealingInfo> HealingDoneInfos => _healingDoneInfosByTarget.Values;

        protected long? _maxPotentialHealingDone, _maxPotentialHealingDonePlusPets;
        public long? MaxPotentialHealingDone => _maxPotentialHealingDone ?? (_maxPotentialHealingDone = HealingDoneInfos.NullableMax(i => i.PotentialHealing));
        public long? MaxPotentialHealingDonePlusPets => _maxPotentialHealingDonePlusPets ?? (_maxPotentialHealingDonePlusPets = HealingDoneInfos.NullableMax(i => i.PotentialHealingPlusPets));

        public long PotentialHealingTaken { get; protected set; }
        public long RealizedHealingTaken { get; protected set; }
        public long OverhealingTaken { get; protected set; }
        public long NanoHealingTaken { get; protected set; }

        public double PotentialHealingTakenPM => PotentialHealingTaken / ActiveDuration.TotalMinutes;
        public double RealizedHealingTakenPM => RealizedHealingTaken / ActiveDuration.TotalMinutes;
        public double OverhealingTakenPM => OverhealingTaken / ActiveDuration.TotalMinutes;
        public double NanoHealingTakenPM => NanoHealingTaken / ActiveDuration.TotalMinutes;

        public double? PercentOfOverhealingTaken => OverhealingTaken / PotentialHealingTaken.NullIfZero();

        protected readonly Dictionary<FightCharacter, HealingInfo> _healingTakenInfosBySource = new Dictionary<FightCharacter, HealingInfo>();
        public IReadOnlyDictionary<FightCharacter, HealingInfo> HealingTakenInfosBySource => _healingTakenInfosBySource;
        public IReadOnlyCollection<HealingInfo> HealingTakenInfos => _healingTakenInfosBySource.Values;

        protected long? _maxPotentialHealingTaken, _maxPotentialHealingPlusPetsTaken;
        public long? MaxPotentialHealingTaken => _maxPotentialHealingTaken ?? (_maxPotentialHealingTaken = HealingTakenInfos.NullableMax(i => i.PotentialHealing));
        // Intentionally weird naming. It's not max (this + pets) have taken from a source, it's max this has taken from a (source + pets).
        public long? MaxPotentialHealingPlusPetsTaken => _maxPotentialHealingPlusPetsTaken ?? (_maxPotentialHealingPlusPetsTaken = HealingTakenInfos.NullableMax(i => i.PotentialHealingPlusPets));

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
        public double? CastSuccessChance => CastSuccessCount / CastAttempts.NullIfZero();
        public double? CastResistedChance => CastResistedCount / CastAttempts.NullIfZero();
        public double? CastCounteredChance => CastCounteredCount / CastAttempts.NullIfZero();
        public double? CastAbortedChance => CastAbortedCount / CastAttempts.NullIfZero();
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
                case AttackResult.Absorbed:
                    // Only an ⦗Unknown⦘ source for events where the attack results in an absorb, so don't bother.
                    break;
                default: throw new NotImplementedException();
            }

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

            _maxDamageDone = _maxDamageDonePlusPets = null;
            attackEvent.Target._maxDamageTaken = attackEvent.Target._maxDamagePlusPetsTaken = null;

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

        public void AddSourceHealEvent(HealEvent healEvent)
        {
            switch (healEvent.HealType)
            {
                case HealType.PotentialHealth:
                    PotentialHealingDone += healEvent.Amount.Value;
                    break;
                case HealType.RealizedHealth:
                    RealizedHealingDone += healEvent.Amount.Value;
                    if (healEvent.StartEvent != null)
                    {
                        OverhealingDone += healEvent.StartEvent.Amount.Value - healEvent.Amount.Value;
                    }
                    else // No corresponding potential, so adding realized is best we can do (happens when owner heals themselves).
                    {
                        PotentialHealingDone += healEvent.Amount.Value;
                    }
                    break;
                case HealType.Nano:
                    NanoHealingDone += healEvent.Amount.Value;
                    break;
                default: throw new NotImplementedException();
            }

            if (_healingDoneInfosByTarget.TryGetValue(healEvent.Target, out HealingInfo healingInfo))
            {
                healingInfo.AddHealEvent(healEvent);
            }
            else
            {
                if (IsFightPet && !FightPetOwner._healingDoneInfosByTarget.ContainsKey(healEvent.Target))
                {
                    var fightPetOwnerHealingInfo = new HealingInfo(FightPetOwner, healEvent.Target);
                    FightPetOwner._healingDoneInfosByTarget[healEvent.Target] = fightPetOwnerHealingInfo;
                    healEvent.Target._healingTakenInfosBySource[FightPetOwner] = fightPetOwnerHealingInfo;
                }

                healingInfo = new HealingInfo(this, healEvent.Target);
                healingInfo.AddHealEvent(healEvent);
                this._healingDoneInfosByTarget[healEvent.Target] = healingInfo;
                healEvent.Target._healingTakenInfosBySource[this] = healingInfo;
            }

            _maxPotentialHealingDone = _maxPotentialHealingDonePlusPets = null;
            healEvent.Target._maxPotentialHealingTaken = healEvent.Target._maxPotentialHealingPlusPetsTaken = null;
        }

        public void AddTargetHealEvent(HealEvent healEvent)
        {
            switch (healEvent.HealType)
            {
                case HealType.PotentialHealth:
                    PotentialHealingTaken += healEvent.Amount.Value;
                    break;
                case HealType.RealizedHealth:
                    RealizedHealingTaken += healEvent.Amount.Value;
                    if (healEvent.StartEvent != null)
                    {
                        OverhealingTaken += healEvent.StartEvent.Amount.Value - healEvent.Amount.Value;
                    }
                    else // No corresponding potential, so adding realized is best we can do (happens when owner heals themselves).
                    {
                        PotentialHealingTaken += healEvent.Amount.Value;
                    }
                    break;
                case HealType.Nano:
                    NanoHealingTaken += healEvent.Amount.Value;
                    break;
                default: throw new NotImplementedException();
            }
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
