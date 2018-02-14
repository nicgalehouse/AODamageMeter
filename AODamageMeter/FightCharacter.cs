using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Attack;
using AODamageMeter.FightEvents.Heal;
using AODamageMeter.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AODamageMeter
{
    public class FightCharacter
    {
        public const string UnknownCharacterName = "〈Unknown〉";

        public FightCharacter(Fight fight, Character character, DateTime enteredTime)
        {
            Fight = fight;
            Character = character;
            EnteredTime = enteredTime;
            _stopwatch = _stopwatchPlusPets = DamageMeter.IsRealTimeMode ? Stopwatch.StartNew() : null;
        }

        public DamageMeter DamageMeter => Fight.DamageMeter;
        public Fight Fight { get; }
        public Character Character { get; }
        public bool IsOwner => Character == DamageMeter.Owner;
        public string Name => Character.Name;
        public string UncoloredName => Character.UncoloredName;
        public bool IsPlayer => Character.IsPlayer;
        public bool IsPlayerOrFightPet => Character.IsPlayer || IsFightPet;
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
        public bool IsUnknown => Name == UnknownCharacterName;

        public DateTime EnteredTime { get; }

        protected Stopwatch _stopwatch;
        protected Stopwatch _stopwatchPlusPets;
        public TimeSpan ActiveDuration => DamageMeter.IsRealTimeMode
            ? _stopwatch.Elapsed : Fight.LatestEventTime.Value - EnteredTime;
        public TimeSpan ActiveDurationPlusPets => DamageMeter.IsRealTimeMode
            // We maintain _stopwatchPlusPets for real time mode so we don't have to find the max active duration all the time.
            ? _stopwatchPlusPets.Elapsed : SelfAndFightPets.Select(c => c.ActiveDuration).Max();

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

        // Notice we're keeping pet status scoped to a fight. This is to support a more abstract concept of pets. In some fights a character might be
        // your pet, and in others it might not. In some fights you might even make a player your pet, like when measuring dual-logged/team performance.
        // Using it this way is possible, but pets are still always marked as non-players. Usually when a character marked as a player is added as
        // a pet, it's because you're using an actual pet that happens to meet the player naming requirements, rather than an actual player.
        public FightCharacter FightPetMaster { get; protected set; }
        protected readonly HashSet<FightCharacter> _fightPets = new HashSet<FightCharacter>();
        public IReadOnlyCollection<FightCharacter> FightPets => _fightPets;
        public bool IsFightPetMaster => FightPets.Count != 0;
        public bool IsFightPet => FightPetMaster != null;
        public FightCharacter FightPetMasterOrSelf => FightPetMaster ?? this;
        public IEnumerable<FightCharacter> SelfAndFightPets
        {
            get
            {
                yield return this;

                foreach (var fightPet in FightPets)
                    yield return fightPet;
            }
        }

        protected internal bool TryRegisterFightPet(FightCharacter fightPet)
        {
            if (fightPet == this) return false;
            if (fightPet.FightPetMaster == this) return true;
            if (fightPet.IsFightPet || fightPet.IsFightPetMaster || IsFightPet) return false;

            _fightPets.Add(fightPet);
            fightPet.FightPetMaster = this;

            if (DamageMeter.IsRealTimeMode)
            {
                _stopwatchPlusPets = SelfAndFightPets.Select(c => c._stopwatch)
                    .OrderByDescending(s => s.Elapsed)
                    .First();
            }

            _maxDamageDonePlusPets = null;
            foreach (var damageTargetOfPetAndMaster in fightPet.DamageDoneInfos.Select(i => i.Target)
                .Intersect(DamageDoneInfos.Select(i => i.Target)))
            {
                damageTargetOfPetAndMaster._maxDamagePlusPetsTaken = null;
            }

            _maxPotentialHealingDonePlusPets = null;
            foreach (var healingTargetOfPetAndMaster in fightPet.HealingDoneInfos.Select(i => i.Target)
                .Intersect(HealingDoneInfos.Select(i => i.Target)))
            {
                healingTargetOfPetAndMaster._maxPotentialHealingPlusPetsTaken = null;
            }

            foreach (var damageTargetOfPetButNotMaster in fightPet.DamageDoneInfos.Select(i => i.Target)
                .Except(DamageDoneInfos.Select(i => i.Target)))
            {
                AddDamageDoneInfo(damageTargetOfPetButNotMaster);
            }

            foreach (var healingTargetOfPetButNotMaster in fightPet.HealingDoneInfos.Select(i => i.Target)
                .Except(HealingDoneInfos.Select(i => i.Target)))
            {
                AddHealingDoneInfo(healingTargetOfPetButNotMaster);
            }

            return true;
        }

        protected internal bool TryDeregisterFightPet(FightCharacter fightPet)
        {
            if (fightPet.FightPetMaster != this) return false;

            _fightPets.Remove(fightPet);
            fightPet.FightPetMaster = null;

            if (DamageMeter.IsRealTimeMode)
            {
                _stopwatchPlusPets = SelfAndFightPets.Select(c => c._stopwatch)
                    .OrderByDescending(s => s.Elapsed)
                    .First();
            }

            _maxDamageDonePlusPets = null;
            foreach (var damageTargetOfPetAndMaster in fightPet.DamageDoneInfos.Select(i => i.Target)
                .Intersect(DamageDoneInfos.Select(i => i.Target)))
            {
                damageTargetOfPetAndMaster._maxDamagePlusPetsTaken = null;
            }

            _maxPotentialHealingDonePlusPets = null;
            foreach (var healingTargetOfPetAndMaster in fightPet.HealingDoneInfos.Select(i => i.Target)
                .Intersect(HealingDoneInfos.Select(i => i.Target)))
            {
                healingTargetOfPetAndMaster._maxPotentialHealingPlusPetsTaken = null;
            }

            // Don't bother checking for damage done or healing done infos and verifying they can be removed. The verification
            // process would require checking that all other pets of master don't have the info, and that the master has never
            // used the info directly either.

            return true;
        }

        public long WeaponDamageDone { get; protected set; }
        public long RegularDamageDone { get; protected set; }
        public long NormalDamageDone { get; protected set; }
        public long CritDamageDone { get; protected set; }
        public long GlanceDamageDone { get; protected set; }
        public long SpecialDamageDone { get; protected set; }
        public long NanoDamageDone { get; protected set; }
        public long IndirectDamageDone { get; protected set; }
        // Absorbed sucks, only have the target when it's the owner, and the source is always unknown. We also don't know
        // what's being absorbed--weapon, nano, or indirect--so it hurts the accuracy of our hit chance stats.
        public long AbsorbedDamageDone { get; protected set; }
        public long TotalDamageDone => WeaponDamageDone + NanoDamageDone + IndirectDamageDone + AbsorbedDamageDone;

        public long WeaponDamageDonePlusPets => WeaponDamageDone + FightPets.Sum(p => p.WeaponDamageDone);
        public long RegularDamageDonePlusPets => RegularDamageDone + FightPets.Sum(p => p.RegularDamageDone);
        public long NormalDamageDonePlusPets => NormalDamageDone + FightPets.Sum(p => p.NormalDamageDone);
        public long CritDamageDonePlusPets => CritDamageDone + FightPets.Sum(p => p.CritDamageDone);
        public long GlanceDamageDonePlusPets => GlanceDamageDone + FightPets.Sum(p => p.GlanceDamageDone);
        public long SpecialDamageDonePlusPets => SpecialDamageDone + FightPets.Sum(p => p.SpecialDamageDone);
        public long NanoDamageDonePlusPets => NanoDamageDone + FightPets.Sum(p => p.NanoDamageDone);
        public long IndirectDamageDonePlusPets => IndirectDamageDone + FightPets.Sum(p => p.IndirectDamageDone);
        public long AbsorbedDamageDonePlusPets => AbsorbedDamageDone + FightPets.Sum(p => p.AbsorbedDamageDone);
        public long TotalDamageDonePlusPets => TotalDamageDone + FightPets.Sum(p => p.TotalDamageDone);
        public long MastersOrOwnTotalDamageDonePlusPets => FightPetMaster?.TotalDamageDonePlusPets ?? TotalDamageDonePlusPets;

        public double WeaponDamageDonePM => WeaponDamageDone / ActiveDuration.TotalMinutes;
        public double NanoDamageDonePM => NanoDamageDone / ActiveDuration.TotalMinutes;
        public double IndirectDamageDonePM => IndirectDamageDone / ActiveDuration.TotalMinutes;
        public double AbsorbedDamageDonePM => AbsorbedDamageDone / ActiveDuration.TotalMinutes;
        public double TotalDamageDonePM => TotalDamageDone / ActiveDuration.TotalMinutes;

        public double WeaponDamageDonePMPlusPets => WeaponDamageDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double NanoDamageDonePMPlusPets => NanoDamageDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double IndirectDamageDonePMPlusPets => IndirectDamageDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double AbsorbedDamageDonePMPlusPets => AbsorbedDamageDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double TotalDamageDonePMPlusPets => TotalDamageDonePlusPets / ActiveDurationPlusPets.TotalMinutes;

        public double? WeaponPercentOfTotalDamageDone => WeaponDamageDone / TotalDamageDone.NullIfZero();
        public double? NanoPercentOfTotalDamageDone => NanoDamageDone / TotalDamageDone.NullIfZero();
        public double? IndirectPercentOfTotalDamageDone => IndirectDamageDone / TotalDamageDone.NullIfZero();
        public double? AbsorbedPercentOfTotalDamageDone => AbsorbedDamageDone / TotalDamageDone.NullIfZero();

        public double? WeaponPercentOfTotalDamageDonePlusPets => WeaponDamageDonePlusPets / TotalDamageDonePlusPets.NullIfZero();
        public double? NanoPercentOfTotalDamageDonePlusPets => NanoDamageDonePlusPets / TotalDamageDonePlusPets.NullIfZero();
        public double? IndirectPercentOfTotalDamageDonePlusPets => IndirectDamageDonePlusPets / TotalDamageDonePlusPets.NullIfZero();
        public double? AbsorbedPercentOfTotalDamageDonePlusPets => AbsorbedDamageDonePlusPets / TotalDamageDonePlusPets.NullIfZero();

        public int WeaponHitsDone { get; protected set; }
        public int RegularsDone { get; protected set; }
        public int NormalsDone { get; protected set; }
        public int CritsDone { get; protected set; }
        public int GlancesDone { get; protected set; }
        public int BlockedHitsDone { get; protected set; } // We only know about blocked hits where the owner is a source or target and other is unknown.
        public int NonBlockedRegularsDone => RegularsDone - BlockedHitsDone;
        public int SpecialsDone { get; protected set; }
        public int MissesDone { get; protected set; } // We only know about misses where the owner is a source or target.
        public int WeaponHitAttemptsDone => WeaponHitsDone + MissesDone;
        public int NanoHitsDone { get; protected set; }
        public int IndirectHitsDone { get; protected set; }
        public int AbsorbedHitsDone { get; protected set; }
        public int TotalHitsDone => WeaponHitsDone + NanoHitsDone + IndirectHitsDone + AbsorbedHitsDone;
        public int TotalNonBlockedHitsDone => TotalHitsDone - BlockedHitsDone;

        public int WeaponHitsDonePlusPets => WeaponHitsDone + FightPets.Sum(p => p.WeaponHitsDone);
        public int RegularsDonePlusPets => RegularsDone + FightPets.Sum(p => p.RegularsDone);
        public int NormalsDonePlusPets => NormalsDone + FightPets.Sum(p => p.NormalsDone);
        public int CritsDonePlusPets => CritsDone + FightPets.Sum(p => p.CritsDone);
        public int GlancesDonePlusPets => GlancesDone + FightPets.Sum(p => p.GlancesDone);
        public int BlockedHitsDonePlusPets => BlockedHitsDone + FightPets.Sum(p => p.BlockedHitsDone);
        public int NonBlockedRegularsDonePlusPets => NonBlockedRegularsDone + FightPets.Sum(p => p.NonBlockedRegularsDone);
        public int SpecialsDonePlusPets => SpecialsDone + FightPets.Sum(p => p.SpecialsDone);
        public int MissesDonePlusPets => MissesDone + FightPets.Sum(p => p.MissesDone);
        public int WeaponHitAttemptsDonePlusPets => WeaponHitAttemptsDone + FightPets.Sum(p => p.WeaponHitAttemptsDone);
        public int NanoHitsDonePlusPets => NanoHitsDone + FightPets.Sum(p => p.NanoHitsDone); 
        public int IndirectHitsDonePlusPets => IndirectHitsDone + FightPets.Sum(p => p.IndirectHitsDone);
        public int AbsorbedHitsDonePlusPets => AbsorbedHitsDone + FightPets.Sum(p => p.AbsorbedHitsDone);
        public int TotalHitsDonePlusPets => TotalHitsDone + FightPets.Sum(p => p.TotalHitsDone);
        public int TotalNonBlockedHitsDonePlusPets => TotalNonBlockedHitsDone + FightPets.Sum(p => p.TotalNonBlockedHitsDone);

        public double WeaponHitsDonePM => WeaponHitsDone / ActiveDuration.TotalMinutes;
        public double RegularsDonePM => RegularsDone / ActiveDuration.TotalMinutes;
        public double NormalsDonePM => NormalsDone / ActiveDuration.TotalMinutes;
        public double CritsDonePM => CritsDone / ActiveDuration.TotalMinutes;
        public double GlancesDonePM => GlancesDone / ActiveDuration.TotalMinutes;
        public double BlockedHitsDonePM => BlockedHitsDone / ActiveDuration.TotalMinutes;
        public double NonBlockedRegularsDonePM => NonBlockedRegularsDone / ActiveDuration.TotalMinutes;
        public double SpecialsDonePM => SpecialsDone / ActiveDuration.TotalMinutes;
        public double MissesDonePM => MissesDone / ActiveDuration.TotalMinutes;
        public double WeaponHitAttemptsDonePM => WeaponHitAttemptsDone / ActiveDuration.TotalMinutes;
        public double NanoHitsDonePM => NanoHitsDone / ActiveDuration.TotalMinutes;
        public double IndirectHitsDonePM => IndirectHitsDone / ActiveDuration.TotalMinutes;
        public double AbsorbedHitsDonePM => AbsorbedHitsDone / ActiveDuration.TotalMinutes;
        public double TotalHitsDonePM => TotalHitsDone / ActiveDuration.TotalMinutes;
        public double TotalNonBlockedHitsDonePM => TotalNonBlockedHitsDone / ActiveDuration.TotalMinutes;

        public double WeaponHitsDonePMPlusPets => WeaponHitsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double RegularsDonePMPlusPets => RegularsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double NormalsDonePMPlusPets => NormalsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double CritsDonePMPlusPets => CritsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double GlancesDonePMPlusPets => GlancesDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double BlockedHitsDonePMPlusPets => BlockedHitsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double NonBlockedRegularsDonePMPlusPets => NonBlockedRegularsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double SpecialsDonePMPlusPets => SpecialsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double MissesDonePMPlusPets => MissesDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double WeaponHitAttemptsDonePMPlusPets => WeaponHitAttemptsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double NanoHitsDonePMPlusPets => NanoHitsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double IndirectHitsDonePMPlusPets => IndirectHitsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double AbsorbedHitsDonePMPlusPets => AbsorbedHitsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double TotalHitsDonePMPlusPets => TotalHitsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double TotalNonBlockedHitsDonePMPlusPets => TotalNonBlockedHitsDonePlusPets / ActiveDurationPlusPets.TotalMinutes;

        public double? WeaponHitDoneChance => WeaponHitsDone / WeaponHitAttemptsDone.NullIfZero();
        public double? CritDoneChance => CritsDone / NonBlockedRegularsDone.NullIfZero();
        public double? GlanceDoneChance => GlancesDone / NonBlockedRegularsDone.NullIfZero();
        public double? BlockedHitDoneChance => BlockedHitsDone / RegularsDone.NullIfZero();
        public double? MissDoneChance => MissesDone / WeaponHitAttemptsDone.NullIfZero();

        public double? WeaponHitDoneChancePlusPets => WeaponHitsDonePlusPets / WeaponHitAttemptsDonePlusPets.NullIfZero();
        public double? CritDoneChancePlusPets => CritsDonePlusPets / NonBlockedRegularsDonePlusPets.NullIfZero();
        public double? GlanceDoneChancePlusPets => GlancesDonePlusPets / NonBlockedRegularsDonePlusPets.NullIfZero();
        public double? BlockedHitDoneChancePlusPets => BlockedHitsDonePlusPets / RegularsDonePlusPets.NullIfZero();
        public double? MissDoneChancePlusPets => MissesDonePlusPets / WeaponHitAttemptsDonePlusPets.NullIfZero();

        public double? AverageWeaponDamageDone => WeaponDamageDone / WeaponHitsDone.NullIfZero();
        public double? AverageRegularDamageDone => RegularDamageDone / RegularsDone.NullIfZero();
        public double? AverageNormalDamageDone => NormalDamageDone / NormalsDone.NullIfZero();
        public double? AverageCritDamageDone => CritDamageDone / CritsDone.NullIfZero();
        public double? AverageGlanceDamageDone => GlanceDamageDone / GlancesDone.NullIfZero();
        public double? AverageSpecialDamageDone => SpecialDamageDone / SpecialsDone.NullIfZero();
        public double? AverageNanoDamageDone => NanoDamageDone / NanoHitsDone.NullIfZero();
        public double? AverageIndirectDamageDone => IndirectDamageDone / IndirectHitsDone.NullIfZero();
        public double? AverageAbsorbedDamageDone => AbsorbedDamageDone / AbsorbedHitsDone.NullIfZero();

        public double? AverageWeaponDamageDonePlusPets => WeaponDamageDonePlusPets / WeaponHitsDonePlusPets.NullIfZero();
        public double? AverageRegularDamageDonePlusPets => RegularDamageDonePlusPets / RegularsDonePlusPets.NullIfZero();
        public double? AverageNormalDamageDonePlusPets => NormalDamageDonePlusPets / NormalsDonePlusPets.NullIfZero();
        public double? AverageCritDamageDonePlusPets => CritDamageDonePlusPets / CritsDonePlusPets.NullIfZero();
        public double? AverageGlanceDamageDonePlusPets => GlanceDamageDonePlusPets / GlancesDonePlusPets.NullIfZero();
        public double? AverageSpecialDamageDonePlusPets => SpecialDamageDonePlusPets / SpecialsDonePlusPets.NullIfZero();
        public double? AverageNanoDamageDonePlusPets => NanoDamageDonePlusPets / NanoHitsDonePlusPets.NullIfZero();
        public double? AverageIndirectDamageDonePlusPets => IndirectDamageDonePlusPets / IndirectHitsDonePlusPets.NullIfZero();
        public double? AverageAbsorbedDamageDonePlusPets => AbsorbedDamageDonePlusPets / AbsorbedHitsDonePlusPets.NullIfZero();

        public double? PercentOfMastersOrOwnTotalDamageDonePlusPets => TotalDamageDone / MastersOrOwnTotalDamageDonePlusPets.NullIfZero();

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
        protected void AddDamageDoneInfo(FightCharacter target, AttackEvent attackEvent = null)
        {
            var damageInfo = new DamageInfo(this, target);
            _damageDoneInfosByTarget.Add(target, damageInfo);
            target._damageTakenInfosBySource.Add(this, damageInfo);

            if (attackEvent != null)
            {
                damageInfo.AddAttackEvent(attackEvent);
            }
        }

        protected long? _maxDamageDone, _maxDamageDonePlusPets;
        public long? MaxDamageDone => _maxDamageDone ?? (_maxDamageDone = DamageDoneInfos.NullableMax(i => i.TotalDamage));
        public long? MaxDamageDonePlusPets => _maxDamageDonePlusPets ?? (_maxDamageDonePlusPets = DamageDoneInfos.NullableMax(i => i.TotalDamagePlusPets));

        protected readonly Dictionary<DamageType, int> _damageTypeHitsDone = new Dictionary<DamageType, int>();
        protected readonly Dictionary<DamageType, long> _damageTypeDamagesDone = new Dictionary<DamageType, long>();
        public IReadOnlyDictionary<DamageType, int> DamageTypeHitsDone => _damageTypeHitsDone;
        public IReadOnlyDictionary<DamageType, long> DamageTypeDamagesDone => _damageTypeDamagesDone;

        public bool HasDamageTypeDamageDone(DamageType damageType) => DamageTypeDamagesDone.ContainsKey(damageType);
        public bool HasSpecialsDone => DamageTypeDamagesDone.Keys.Any(DamageTypeHelpers.IsSpecialDamageType);
        public int? GetDamageTypeHitsDone(DamageType damageType) => DamageTypeHitsDone.TryGetValue(damageType, out int damageTypeHitsDone) ? damageTypeHitsDone : (int?)null;
        public long? GetDamageTypeDamageDone(DamageType damageType) => DamageTypeDamagesDone.TryGetValue(damageType, out long damageTypeDamageDone) ? damageTypeDamageDone : (long?)null;
        public double? GetAverageDamageTypeDamageDone(DamageType damageType) => GetDamageTypeDamageDone(damageType) / (double?)GetDamageTypeHitsDone(damageType);
        public double? GetSecondsPerDamageTypeHitDone(DamageType damageType) => ActiveDuration.TotalSeconds / GetDamageTypeHitsDone(damageType);
        public double? GetPercentDamageTypeDamageDone(DamageType damageType) => GetDamageTypeDamageDone(damageType) / (double?)TotalDamageDone;
        public double? GetPercentDamageTypeHitsDone(DamageType damageType) => GetDamageTypeHitsDone(damageType) / (double?)TotalNonBlockedHitsDone;

        public bool HasDamageTypeDamageDonePlusPets(DamageType damageType) => SelfAndFightPets.Any(c => c.HasDamageTypeDamageDone(damageType));
        public bool HasSpecialsDonePlusPets => SelfAndFightPets.Any(c => c.HasSpecialsDone);
        public int? GetDamageTypeHitsDonePlusPets(DamageType damageType) => SelfAndFightPets.NullableSum(c => c.GetDamageTypeHitsDone(damageType));
        public long? GetDamageTypeDamageDonePlusPets(DamageType damageType) => SelfAndFightPets.NullableSum(c => c.GetDamageTypeDamageDone(damageType));
        public double? GetAverageDamageTypeDamageDonePlusPets(DamageType damageType) => GetDamageTypeDamageDonePlusPets(damageType) / (double?)GetDamageTypeHitsDonePlusPets(damageType);
        public double? GetSecondsPerDamageTypeHitDonePlusPets(DamageType damageType) => ActiveDurationPlusPets.TotalSeconds / GetDamageTypeHitsDonePlusPets(damageType);
        public double? GetPercentDamageTypeDamageDonePlusPets(DamageType damageType) => GetDamageTypeDamageDonePlusPets(damageType) / (double?)TotalDamageDonePlusPets;
        public double? GetPercentDamageTypeHitsDonePlusPets(DamageType damageType) => GetDamageTypeHitsDonePlusPets(damageType) / (double?)TotalNonBlockedHitsDonePlusPets;

        public long HealthDrained { get; protected set; }
        public long NanoDrained { get; protected set; }

        public double HealthDrainedPM => HealthDrained / ActiveDuration.TotalMinutes;
        public double NanoDrainedPM => NanoDrained / ActiveDuration.TotalMinutes;

        public long WeaponDamageTaken { get; protected set; }
        public long RegularDamageTaken { get; protected set; }
        public long NormalDamageTaken { get; protected set; }
        public long CritDamageTaken { get; protected set; }
        public long GlanceDamageTaken { get; protected set; }
        public long SpecialDamageTaken { get; protected set; }
        public long NanoDamageTaken { get; protected set; }
        public long IndirectDamageTaken { get; protected set; }
        // Absorbed sucks, only have the target when it's the owner, and the source is always unknown. We also don't know
        // what's being absorbed--weapon, nano, or indirect--so it hurts the accuracy of our hit chance stats.
        public long AbsorbedDamageTaken { get; protected set; }
        public long TotalDamageTaken => WeaponDamageTaken + NanoDamageTaken + IndirectDamageTaken + AbsorbedDamageTaken;

        public double WeaponDamageTakenPM => WeaponDamageTaken / ActiveDuration.TotalMinutes;
        public double NanoDamageTakenPM => NanoDamageTaken / ActiveDuration.TotalMinutes;
        public double IndirectDamageTakenPM => IndirectDamageTaken / ActiveDuration.TotalMinutes;
        public double AbsorbedDamageTakenPM => AbsorbedDamageTaken / ActiveDuration.TotalMinutes;
        public double TotalDamageTakenPM => TotalDamageTaken / ActiveDuration.TotalMinutes;

        public double? WeaponPercentOfTotalDamageTaken => WeaponDamageTaken / TotalDamageTaken.NullIfZero();
        public double? NanoPercentOfTotalDamageTaken => NanoDamageTaken / TotalDamageTaken.NullIfZero();
        public double? IndirectPercentOfTotalDamageTaken => IndirectDamageTaken / TotalDamageTaken.NullIfZero();
        public double? AbsorbedPercentOfTotalDamageTaken => AbsorbedDamageTaken / TotalDamageTaken.NullIfZero();

        public int WeaponHitsTaken { get; protected set; }
        public int RegularsTaken { get; protected set; }
        public int NormalsTaken { get; protected set; }
        public int CritsTaken { get; protected set; }
        public int GlancesTaken { get; protected set; }
        public int BlockedHitsTaken { get; protected set; } // We only know about blocked hits where the owner is a source or target and other is unknown.
        public int NonBlockedRegularsTaken => RegularsTaken - BlockedHitsTaken;
        public int SpecialsTaken { get; protected set; }
        public int MissesTaken { get; protected set; } // We only know about misses where the owner is a source or target.
        public int WeaponHitAttemptsTaken => WeaponHitsTaken + MissesTaken;
        public int NanoHitsTaken { get; protected set; }
        public int IndirectHitsTaken { get; protected set; }
        public int AbsorbedHitsTaken { get; protected set; }
        public int TotalHitsTaken => WeaponHitsTaken + NanoHitsTaken + IndirectHitsTaken + AbsorbedHitsTaken;
        public int TotalNonBlockedHitsTaken => TotalHitsTaken - BlockedHitsTaken;

        public double WeaponHitsTakenPM => WeaponHitsTaken / ActiveDuration.TotalMinutes;
        public double RegularsTakenPM => RegularsTaken / ActiveDuration.TotalMinutes;
        public double NormalsTakenPM => NormalsTaken / ActiveDuration.TotalMinutes;
        public double CritsTakenPM => CritsTaken / ActiveDuration.TotalMinutes;
        public double GlancesTakenPM => GlancesTaken / ActiveDuration.TotalMinutes;
        public double BlockedHitsTakenPM => BlockedHitsTaken / ActiveDuration.TotalMinutes;
        public double NonBlockedRegularsTakenPM => NonBlockedRegularsTaken / ActiveDuration.TotalMinutes;
        public double SpecialsTakenPM => SpecialsTaken / ActiveDuration.TotalMinutes;
        public double MissesTakenPM => MissesTaken / ActiveDuration.TotalMinutes;
        public double WeaponHitAttemptsTakenPM => WeaponHitAttemptsTaken / ActiveDuration.TotalMinutes;
        public double NanoHitsTakenPM => NanoHitsTaken / ActiveDuration.TotalMinutes;
        public double IndirectHitsTakenPM => IndirectHitsTaken / ActiveDuration.TotalMinutes;
        public double AbsorbedHitsTakenPM => AbsorbedHitsTaken / ActiveDuration.TotalMinutes;
        public double TotalHitsTakenPM => TotalHitsTaken / ActiveDuration.TotalMinutes;
        public double TotalNonBlockedHitsTakenPM => TotalNonBlockedHitsTaken / ActiveDuration.TotalMinutes;

        public double? WeaponHitTakenChance => WeaponHitsTaken / WeaponHitAttemptsTaken.NullIfZero();
        public double? CritTakenChance => CritsTaken / NonBlockedRegularsTaken.NullIfZero();
        public double? GlanceTakenChance => GlancesTaken / NonBlockedRegularsTaken.NullIfZero();
        public double? BlockedHitTakenChance => BlockedHitsTaken / RegularsTaken.NullIfZero();
        public double? MissTakenChance => MissesTaken / WeaponHitAttemptsTaken.NullIfZero();

        public double? AverageWeaponDamageTaken => WeaponDamageTaken / WeaponHitsTaken.NullIfZero();
        public double? AverageRegularDamageTaken => RegularDamageTaken / RegularsTaken.NullIfZero();
        public double? AverageNormalDamageTaken => NormalDamageTaken / NormalsTaken.NullIfZero();
        public double? AverageCritDamageTaken => CritDamageTaken / CritsTaken.NullIfZero();
        public double? AverageGlanceDamageTaken => GlanceDamageTaken / GlancesTaken.NullIfZero();
        public double? AverageSpecialDamageTaken => SpecialDamageTaken / SpecialsTaken.NullIfZero();
        public double? AverageNanoDamageTaken => NanoDamageTaken / NanoHitsTaken.NullIfZero();
        public double? AverageIndirectDamageTaken => IndirectDamageTaken / IndirectHitsTaken.NullIfZero();
        public double? AverageAbsorbedDamageTaken => AbsorbedDamageTaken / AbsorbedHitsTaken.NullIfZero();

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
        public IReadOnlyDictionary<DamageType, int> DamageTypeHitsTaken=> _damageTypeHitsTaken;
        public IReadOnlyDictionary<DamageType, long> DamageTypeDamagesTaken => _damageTypeDamagesTaken;

        public bool HasDamageTypeDamageTaken(DamageType damageType) => DamageTypeDamagesTaken.ContainsKey(damageType);
        public bool HasSpecialsTaken => DamageTypeDamagesTaken.Keys.Any(DamageTypeHelpers.IsSpecialDamageType);
        public int? GetDamageTypeHitsTaken(DamageType damageType) => DamageTypeHitsTaken.TryGetValue(damageType, out int damageTypeHitsTaken) ? damageTypeHitsTaken : (int?)null;
        public long? GetDamageTypeDamageTaken(DamageType damageType) => DamageTypeDamagesTaken.TryGetValue(damageType, out long damageTypeDamageTaken) ? damageTypeDamageTaken : (long?)null;
        public double? GetAverageDamageTypeDamageTaken(DamageType damageType) => GetDamageTypeDamageTaken(damageType) / (double?)GetDamageTypeHitsTaken(damageType);
        public double? GetSecondsPerDamageTypeHitTaken(DamageType damageType) => ActiveDuration.TotalSeconds / GetDamageTypeHitsTaken(damageType);
        public double? GetPercentDamageTypeDamageTaken(DamageType damageType) => GetDamageTypeDamageTaken(damageType) / (double?)TotalDamageTaken;
        public double? GetPercentDamageTypeHitsTaken(DamageType damageType) => GetDamageTypeHitsTaken(damageType) / (double?)TotalNonBlockedHitsTaken;

        // We only know about blocked hits where the owner is a source or target and other is unknown.
        public bool HasCompleteBlockedHitStats => IsOwner;
        public bool HasCompleteBlockedHitStatsPlusPets => IsOwner && !FightPets.Any();

        // We only know about misses where the owner is a source or target.
        public bool HasCompleteMissStats => IsOwner;
        public bool HasCompleteMissStatsPlusPets => IsOwner && !FightPets.Any();

        // We only know about the target of absorbed damage when it's the owner, and we never know the source.
        public bool HasCompleteAbsorbedDamageDoneStats => IsUnknown;
        public bool HasCompleteAbsorbedDamageTakenStats => IsOwner || IsUnknown;

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

        public double PotentialHealingDonePMPlusPets => PotentialHealingDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double RealizedHealingDonePMPlusPets => RealizedHealingDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double OverhealingDonePMPlusPets => OverhealingDonePlusPets / ActiveDurationPlusPets.TotalMinutes;
        public double NanoHealingDonePMPlusPets => NanoHealingDonePlusPets / ActiveDurationPlusPets.TotalMinutes;

        public double? PercentOfOverhealingDone => OverhealingDone / PotentialHealingDone.NullIfZero();
        public double? PercentOfOverhealingDonePlusPets => OverhealingDonePlusPets / PotentialHealingDonePlusPets.NullIfZero();

        protected readonly Dictionary<FightCharacter, HealingInfo> _healingDoneInfosByTarget = new Dictionary<FightCharacter, HealingInfo>();
        public IReadOnlyDictionary<FightCharacter, HealingInfo> HealingDoneInfosByTarget => _healingDoneInfosByTarget;
        public IReadOnlyCollection<HealingInfo> HealingDoneInfos => _healingDoneInfosByTarget.Values;
        protected void AddHealingDoneInfo(FightCharacter target, HealEvent healEvent = null)
        {
            var healingInfo = new HealingInfo(this, target);
            _healingDoneInfosByTarget.Add(target, healingInfo);
            target._healingTakenInfosBySource.Add(this, healingInfo);

            if (healEvent != null)
            {
                healingInfo.AddHealEvent(healEvent);
            }
        }

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
        public long ResearchXPGained { get; protected set; }
        public long EffectiveXPGained => NormalXPGained + ShadowXPGained * 1000 + ResearchXPGained;
        public int AlienXPGained { get; protected set; }
        public int PvpDuelXPGained { get; protected set; }
        public int PvpSoloXPGained { get; protected set; }
        public int PvpTeamXPGained { get; protected set; }

        public double NormalXPGainedPM => NormalXPGained / ActiveDuration.TotalMinutes;
        public double ShadowXPGainedPM => ShadowXPGained / ActiveDuration.TotalMinutes;
        public double ResearchXPGainedPM => ResearchXPGained / ActiveDuration.TotalMinutes;
        public double EffectiveXPGainedPM => EffectiveXPGained / ActiveDuration.TotalMinutes;
        public double AlienXPGainedPM => AlienXPGained / ActiveDuration.TotalMinutes;
        public double PvpDuelXPGainedPM => PvpDuelXPGained / ActiveDuration.TotalMinutes;
        public double PvpSoloXPGainedPM => PvpSoloXPGained / ActiveDuration.TotalMinutes;
        public double PvpTeamXPGainedPM => PvpTeamXPGained / ActiveDuration.TotalMinutes;

        // We only know about cast events where the source is the owner (there's no target).
        public int CastAttempts => CastSuccesses + CastCountereds + CastResisteds + CastAborteds + CastInterrupteds;
        public int CastSuccesses { get; protected set; }
        public int CastCountereds { get; protected set; }
        public int CastResisteds { get; protected set; }
        public int CastAborteds { get; protected set; }
        public int CastInterrupteds { get; protected set; }

        public double CastAttemptsPM => CastAttempts / ActiveDuration.TotalMinutes;
        public double CastSuccessesPM => CastSuccesses / ActiveDuration.TotalMinutes;
        public double CastCounteredsPM => CastCountereds / ActiveDuration.TotalMinutes;
        public double CastResistedsPM => CastResisteds / ActiveDuration.TotalMinutes;
        public double CastAbortedsPM => CastAborteds / ActiveDuration.TotalMinutes;
        public double CastInterruptedsPM => CastInterrupteds / ActiveDuration.TotalMinutes;

        public double? CastSuccessChance => CastSuccesses / CastAttempts.NullIfZero();
        public double? CastCounteredChance => CastCountereds / CastAttempts.NullIfZero();
        public double? CastResistedChance => CastResisteds / CastAttempts.NullIfZero();
        public double? CastAbortedChance => CastAborteds / CastAttempts.NullIfZero();
        public double? CastInterruptedChance => CastInterrupteds / CastAttempts.NullIfZero();

        public int CastUnavailables { get; protected set; }
        // A measure of how much useless button spamming you're doing, I guess?
        public double? PercentOfCastsUnavailable => CastUnavailables / (CastAttempts + CastUnavailables).NullIfZero();

        protected readonly Dictionary<string, CastInfo> _castInfosByNanoProgram = new Dictionary<string, CastInfo>();
        public IReadOnlyDictionary<string, CastInfo> CastInfosByNanoProgram => _castInfosByNanoProgram;
        public IReadOnlyCollection<CastInfo> CastInfos => _castInfosByNanoProgram.Values;

        protected int? _maxCastSuccesses;
        public int? MaxCastSuccesses => _maxCastSuccesses ?? (_maxCastSuccesses = CastInfos.NullableMax(i => i.CastSuccesses));

        public void AddSourceAttackEvent(AttackEvent attackEvent)
        {
            switch (attackEvent.AttackResult)
            {
                case AttackResult.WeaponHit:
                    WeaponDamageDone += attackEvent.Amount ?? 0;
                    ++WeaponHitsDone;
                    if (!attackEvent.IsSpecialDamage)
                    {
                        RegularDamageDone += attackEvent.Amount ?? 0;
                        ++RegularsDone;
                        if (!attackEvent.AttackModifier.HasValue)
                        {
                            NormalDamageDone += attackEvent.Amount.Value;
                            ++NormalsDone;
                        }
                        else if (attackEvent.AttackModifier == AttackModifier.Crit)
                        {
                            CritDamageDone += attackEvent.Amount.Value;
                            ++CritsDone;
                        }
                        else if (attackEvent.AttackModifier == AttackModifier.Glance)
                        {
                            GlanceDamageDone += attackEvent.Amount.Value;
                            ++GlancesDone;
                        }
                        else if (attackEvent.AttackModifier == AttackModifier.Block)
                        {
                            ++BlockedHitsDone;
                        }
                        else throw new NotImplementedException();
                    }
                    else
                    {
                        SpecialDamageDone += attackEvent.Amount.Value;
                        ++SpecialsDone;
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
                    AbsorbedDamageDone += attackEvent.Amount.Value;
                    ++AbsorbedHitsDone;
                    break;
                default: throw new NotImplementedException();
            }

            if (DamageDoneInfosByTarget.TryGetValue(attackEvent.Target, out DamageInfo damageInfo))
            {
                damageInfo.AddAttackEvent(attackEvent);
            }
            else
            {
                if (IsFightPet && !FightPetMaster.DamageDoneInfosByTarget.ContainsKey(attackEvent.Target))
                {
                    FightPetMaster.AddDamageDoneInfo(attackEvent.Target);
                }

                AddDamageDoneInfo(attackEvent.Target, attackEvent);
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
                    WeaponDamageTaken += attackEvent.Amount ?? 0;
                    ++WeaponHitsTaken;
                    if (!attackEvent.IsSpecialDamage)
                    {
                        RegularDamageTaken += attackEvent.Amount ?? 0;
                        ++RegularsTaken;
                        if (!attackEvent.AttackModifier.HasValue)
                        {
                            NormalDamageTaken += attackEvent.Amount.Value;
                            ++NormalsTaken;
                        }
                        else if (attackEvent.AttackModifier == AttackModifier.Crit)
                        {
                            CritDamageTaken += attackEvent.Amount.Value;
                            ++CritsTaken;
                        }
                        else if (attackEvent.AttackModifier == AttackModifier.Glance)
                        {
                            GlanceDamageTaken += attackEvent.Amount.Value;
                            ++GlancesTaken;
                        }
                        else if (attackEvent.AttackModifier == AttackModifier.Block)
                        {
                            ++BlockedHitsTaken;
                        }
                        else throw new NotImplementedException();
                    }
                    else
                    {
                        SpecialDamageTaken += attackEvent.Amount.Value;
                        ++SpecialsTaken;
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
                    AbsorbedDamageTaken += attackEvent.Amount.Value;
                    ++AbsorbedHitsTaken;
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

            if (HealingDoneInfosByTarget.TryGetValue(healEvent.Target, out HealingInfo healingInfo))
            {
                healingInfo.AddHealEvent(healEvent);
            }
            else
            {
                if (IsFightPet && !FightPetMaster.HealingDoneInfosByTarget.ContainsKey(healEvent.Target))
                {
                    FightPetMaster.AddHealingDoneInfo(healEvent.Target);
                }

                AddHealingDoneInfo(healEvent.Target, healEvent);
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
                case LevelType.Research: ResearchXPGained += levelEvent.Amount ?? 0; break;
                case LevelType.PvpDuel: PvpDuelXPGained += levelEvent.Amount.Value; break;
                case LevelType.PvpSolo: PvpSoloXPGained += levelEvent.Amount.Value; break;
                case LevelType.PvpTeam: PvpTeamXPGained += levelEvent.Amount.Value; break;
                default: throw new NotImplementedException();
            }
        }

        public void AddCastEvent(MeCastNano castEvent)
        {
            if (castEvent.IsEndOfCast)
            {
                switch (castEvent.CastResult.Value)
                {
                    case CastResult.Success: ++CastSuccesses; break;
                    case CastResult.Countered: ++CastCountereds; break;
                    case CastResult.Resisted: ++CastResisteds; break;
                    case CastResult.Aborted: ++CastAborteds; break;
                    case CastResult.Interrupted: ++CastInterrupteds; break;
                    default: throw new NotImplementedException();
                }

                if (_castInfosByNanoProgram.TryGetValue(castEvent.NanoProgram, out CastInfo castInfo))
                {
                    castInfo.AddCastEvent(castEvent);
                }
                else
                {
                    castInfo = new CastInfo(this, castEvent.NanoProgram);
                    castInfo.AddCastEvent(castEvent);
                    _castInfosByNanoProgram.Add(castEvent.NanoProgram, castInfo);
                }

                _maxCastSuccesses = null;
            }
            else if (castEvent.IsCastUnavailable)
            {
                ++CastUnavailables;
            }
            else if (!castEvent.IsStartOfCast && !castEvent.IsEndOfCast)
            {
                // Nothing to do here, this is how events that may eventually become start events comes in.
            }
            else throw new NotImplementedException();
        }

        public void AddSystemEvent(SystemEvent systemEvent)
        {
            if (systemEvent.IsSelfNanoHeal)
            {
                var healEvent = new MeGotNano(systemEvent);
                AddSourceHealEvent(healEvent);
                AddTargetHealEvent(healEvent);
            }
            else if (systemEvent.IsHealthDrain)
            {
                HealthDrained += systemEvent.Amount.Value;
            }
            else if (systemEvent.IsNanoDrain)
            {
                NanoDrained += systemEvent.Amount.Value;
            }
            else if (systemEvent.IsNanoInterrupt)
            {
                var castEvent = new MeCastNano(systemEvent);
                AddCastEvent(castEvent);
            }
            else if (systemEvent.IsYouBlockedRegular)
            {
                var attackEvent = new MeHitByMonster(systemEvent);
                attackEvent.Source.AddSourceAttackEvent(attackEvent);
                AddTargetAttackEvent(attackEvent);
            }
            else if (systemEvent.IsYourRegularBlocked)
            {
                var attackEvent = new YouHitOther(systemEvent);
                AddSourceAttackEvent(attackEvent);
                attackEvent.Target.AddTargetAttackEvent(attackEvent);
            }
            else throw new NotImplementedException();
        }

        public override string ToString()
            => $"{Character}: {PercentOfFightsTotalDamageDone:P1} of total damage.";
    }
}
