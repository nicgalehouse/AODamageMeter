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

        public DamageMeter DamageMeter => Fight.DamageMeter;
        public Fight Fight { get; }
        public Character Character { get; }
        public bool IsDamageMeterOwner => Character == DamageMeter.Owner;
        public string Name => Character.Name;
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
        public TimeSpan ActiveDurationPlusPets => new[] { ActiveDuration }.Concat(FightPets.Select(p => p.ActiveDuration)).Max();

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

        public FightCharacter FightPetOwner { get; protected set; }
        protected readonly HashSet<FightCharacter> _fightPets = new HashSet<FightCharacter>();
        public IReadOnlyCollection<FightCharacter> FightPets => _fightPets;
        public void RegisterFightPet(FightCharacter fightPet)
        {
            Character.RegisterPet(fightPet.Character);
            fightPet.FightPetOwner = this;
            _fightPets.Add(fightPet);
        }

        public long DamageDone { get; protected set; }
        public long DamageDonePlusPets => DamageDone + FightPets.Sum(p => p.DamageDone);
        public long OwnOrOwnersDamageDonePlusPets => FightPetOwner?.DamageDonePlusPets ?? DamageDonePlusPets;
        public long HitDamageDone { get; protected set; }
        public long HitDamageDonePlusPets => HitDamageDone + FightPets.Sum(p => p.HitDamageDone);
        public long NanoHitDamageDone { get; protected set; }
        public long NanoHitDamageDonePlusPets => NanoHitDamageDone + FightPets.Sum(p => p.NanoHitDamageDone);
        public long IndirectHitDamageDone { get; protected set; }
        public long IndirectHitDamageDonePlusPets => IndirectHitDamageDone + FightPets.Sum(p => p.IndirectHitDamageDone);
        public int HitCount { get; protected set; }
        public int HitCountPlusPets => HitCount + FightPets.Sum(p => p.HitCount);
        public double ActiveHPS => ActiveDuration.TotalSeconds <= 1 ? HitCount : HitCount / ActiveDuration.TotalSeconds;
        public double ActiveHPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? HitCountPlusPets : HitCountPlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveHPM => 60 * ActiveHPS;
        public double ActiveHPMPlusPets => 60 * ActiveHPSPlusPets;
        public int CritCount { get; protected set; }
        public int CritCountPlusPets => CritCount + FightPets.Sum(p => p.CritCount);
        public double ActiveCPS => ActiveDuration.TotalSeconds <= 1 ? CritCount : CritCount / ActiveDuration.TotalSeconds;
        public double ActiveCPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? CritCountPlusPets : CritCountPlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveCPM => 60 * ActiveCPS;
        public double ActiveCPMPlusPets => 60 * ActiveCPSPlusPets;
        public int GlanceCount { get; protected set; }
        public int GlanceCountPlusPets => GlanceCount + FightPets.Sum(p => p.GlanceCount);
        public double ActiveGPS => ActiveDuration.TotalSeconds <= 1 ? GlanceCount : GlanceCount / ActiveDuration.TotalSeconds;
        public double ActiveGPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? GlanceCountPlusPets : GlanceCountPlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveGPM => 60 * ActiveGPS;
        public double ActiveGPMPlusPets => 60 * ActiveGPSPlusPets;
        // We only know about misses where the owner is a source or target.
        public int MissCount { get; protected set; }
        public int MissCountPlusPets => MissCount + FightPets.Sum(p => p.MissCount);
        public double ActiveMPS => ActiveDuration.TotalSeconds <= 1 ? MissCount : MissCount / ActiveDuration.TotalSeconds;
        public double ActiveMPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? MissCountPlusPets : MissCountPlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveMPM => 60 * ActiveMPS;
        public double ActiveMPMPlusPets => 60 * ActiveMPSPlusPets;
        public int HitAttempts => HitCount + MissCount;
        public int HitAttemptsPlusPets => HitAttempts + FightPets.Sum(p => p.HitAttempts);
        public double ActiveHAPS => ActiveDuration.TotalSeconds <= 1 ? HitAttempts : HitAttempts / ActiveDuration.TotalSeconds;
        public double ActiveHAPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? HitAttemptsPlusPets : HitAttemptsPlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveHAPM => 60 * ActiveHAPS;
        public double ActiveHAPMPlusPets => 60 * ActiveHAPSPlusPets;
        public int NanoHitCount { get; protected set; }
        public int NanoHitCountPlusPets => NanoHitCount + FightPets.Sum(p => p.NanoHitCount); 
        public double ActiveNHPS => ActiveDuration.TotalSeconds <= 1 ? NanoHitCount : NanoHitCount / ActiveDuration.TotalSeconds;
        public double ActiveNHPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? NanoHitCountPlusPets : NanoHitCountPlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveNHPM => 60 * ActiveNHPS;
        public double ActiveNHPMPlusPets => 60 * ActiveNHPSPlusPets;
        public int IndirectHitCount { get; protected set; }
        public int IndirectHitCountPlusPets => IndirectHitCount + FightPets.Sum(p => p.IndirectHitCount);
        public double ActiveIHPS => ActiveDuration.TotalSeconds <= 1 ? IndirectHitCount : IndirectHitCount / ActiveDuration.TotalSeconds;
        public double ActiveIHPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? IndirectHitCountPlusPets : IndirectHitCountPlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveIHPM => 60 * ActiveIHPS;
        public double ActiveIHPMPlusPets => 60 * ActiveIHPSPlusPets;
        public int TotalHitCount => HitCount + NanoHitCount + IndirectHitCount;
        public int TotalHitCountPlusPets => HitCountPlusPets + NanoHitCountPlusPets + IndirectHitCountPlusPets;
        public double ActiveTHPS => ActiveDuration.TotalSeconds <= 1 ? TotalHitCount : TotalHitCount / ActiveDuration.TotalSeconds;
        public double ActiveTHPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? TotalHitCountPlusPets : TotalHitCountPlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveTHPM => 60 * ActiveTHPS;
        public double ActiveTHPMPlusPets => 60 * ActiveTHPSPlusPets;
        public double HitChance => HitAttempts == 0 ? 0 : HitCount / (double)HitAttempts;
        public double HitChancePlusPets => HitAttemptsPlusPets == 0 ? 0 : HitCountPlusPets / (double)HitAttemptsPlusPets;
        public double CritChance => HitAttempts == 0 ? 0 : CritCount / (double)HitAttempts;
        public double CritChancePlusPets => HitAttemptsPlusPets == 0 ? 0 : CritCountPlusPets / (double)HitAttemptsPlusPets;
        public double GlanceChance => HitAttempts == 0 ? 0 : GlanceCount / (double)HitAttempts;
        public double GlanceChancePlusPets => HitAttemptsPlusPets == 0 ? 0 : GlanceCountPlusPets / (double)HitAttemptsPlusPets;
        public double MissChance => HitAttempts == 0 ? 0 : MissCount / (double)HitAttempts;
        public double MissChancePlusPets => HitAttemptsPlusPets == 0 ? 0 : MissCountPlusPets / (double)HitAttemptsPlusPets;
        public double PercentOfTotalDamageDone => Fight.TotalDamageDone == 0 ? 0 : DamageDone / (double)Fight.TotalDamageDone;
        public double PercentPlusPetsOfTotalDamageDone => Fight.TotalDamageDone == 0 ? 0 : DamageDonePlusPets / (double)Fight.TotalDamageDone;
        public double PercentOfMaxDamageDone => Fight.MaxDamageDone == 0 ? 0 : DamageDone / (double)Fight.MaxDamageDone;
        public double PercentOfMaxDamageDonePlusPets => Fight.MaxDamageDonePlusPets == 0 ? 0 : DamageDone / (double)Fight.MaxDamageDonePlusPets;
        public double PercentPlusPetsOfMaxDamageDonePlusPets => Fight.MaxDamageDonePlusPets == 0 ? 0 : DamageDonePlusPets / (double)Fight.MaxDamageDonePlusPets;
        public double PercentOfOwnOrOwnersDamageDonePlusPets => OwnOrOwnersDamageDonePlusPets == 0 ? 0 : DamageDone / (double)OwnOrOwnersDamageDonePlusPets;
        public double ActiveDPS => ActiveDuration.TotalSeconds <= 1 ? DamageDone : DamageDone / ActiveDuration.TotalSeconds;
        public double ActiveDPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? DamageDonePlusPets : DamageDonePlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveDPM => 60 * ActiveDPS;
        public double ActiveDPMPlusPets => 60 * ActiveDPSPlusPets;
        public double FullDPS => Fight.Duration.Value.TotalSeconds <= 1 ? DamageDone : DamageDone / Fight.Duration.Value.TotalSeconds;
        public double FullDPM => 60 * FullDPS;
        public double ActiveHDPS => ActiveDuration.TotalSeconds <= 1 ? HitDamageDone : HitDamageDone / ActiveDuration.TotalSeconds;
        public double ActiveHDPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? HitDamageDonePlusPets : HitDamageDonePlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveHDPM => 60 * ActiveHDPS;
        public double ActiveHDPMPlusPets => 60 * ActiveHDPSPlusPets;
        public double ActiveNHDPS => ActiveDuration.TotalSeconds <= 1 ? NanoHitDamageDone : NanoHitDamageDone / ActiveDuration.TotalSeconds;
        public double ActiveNHDPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? NanoHitDamageDonePlusPets : NanoHitDamageDonePlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveNHDPM => 60 * ActiveNHDPS;
        public double ActiveNHDPMPlusPets => 60 * ActiveNHDPSPlusPets;
        public double ActiveIHDPS => ActiveDuration.TotalSeconds <= 1 ? IndirectHitDamageDone : IndirectHitDamageDone / ActiveDuration.TotalSeconds;
        public double ActiveIHDPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? IndirectHitDamageDonePlusPets : IndirectHitDamageDonePlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double ActiveIHDPM => 60 * ActiveIHDPS;
        public double ActiveIHDPMPlusPets => 60 * ActiveIHDPSPlusPets;
        public double PercentOfDamageDoneViaHits => DamageDone == 0 ? 0 : HitDamageDone / (double)DamageDone;
        public double PercentOfDamageDoneViaNanoHits => DamageDone == 0 ? 0 : NanoHitDamageDone / (double)DamageDone;
        public double PercentOfDamageDoneViaIndirectHits => DamageDone == 0 ? 0 : IndirectHitDamageDone / (double)DamageDone;
        public double PercentOfDamageDonePlusPetsViaHits => DamageDonePlusPets == 0 ? 0 : HitDamageDonePlusPets / (double)DamageDonePlusPets;
        public double PercentOfDamageDonePlusPetsViaNanoHits => DamageDonePlusPets == 0 ? 0 : NanoHitDamageDonePlusPets / (double)DamageDonePlusPets;
        public double PercentOfDamageDonePlusPetsViaIndirectHits => DamageDonePlusPets == 0 ? 0 : IndirectHitDamageDonePlusPets / (double)DamageDonePlusPets;

        protected Dictionary<DamageType, int> _damageTypeDamageDoneCounts = new Dictionary<DamageType, int>();
        protected Dictionary<DamageType, long> _damageTypeDamageDones = new Dictionary<DamageType, long>();
        public IReadOnlyDictionary<DamageType, int> DamageTypeDamageDoneCounts => _damageTypeDamageDoneCounts;
        public IReadOnlyDictionary<DamageType, long> DamageTypeDamageDones => _damageTypeDamageDones;

        public bool HasDamageTypeDamageDone(DamageType damageType)
            => DamageTypeDamageDoneCounts.ContainsKey(damageType);

        public bool HasSpecialsDone
            => DamageTypeHelpers.SpecialDamageTypes.Any(HasDamageTypeDamageDone);

        public int? GetAverageDamageTypeDamageDone(DamageType damageType)
            => DamageTypeDamageDoneCounts.TryGetValue(damageType, out int damageTypeDamageDoneCount)
            ? (int?)(DamageTypeDamageDones[damageType] / damageTypeDamageDoneCount) : null;

        public double? GetActiveSecondsPerDamageTypeDamageDone(DamageType damageType) // For special damage types this approximates the 'recharge'.
            => DamageTypeDamageDoneCounts.TryGetValue(damageType, out int damageTypeDamageDoneCount)
            ? ActiveDuration.TotalSeconds / damageTypeDamageDoneCount : (double?)null;

        public long DamageTaken { get; protected set; }
        public long HitDamageTaken { get; protected set; }
        public long NanoHitDamageTaken { get; protected set; }
        public long IndirectHitDamageTaken { get; protected set; }
        public int HitCountTaken { get; protected set; }
        public int CritCountTaken { get; protected set; }
        public int GlanceCountTaken { get; protected set; }
        // We only know about misses where the owner is a source or target.
        public int MissCountTaken { get; protected set; }
        public int HitAttemptsTaken => HitCountTaken + MissCountTaken;
        public int NanoHitCountTaken { get; protected set; }
        public int IndirectHitCountTaken { get; protected set; }
        public double HitChanceTaken => HitAttemptsTaken == 0 ? 0 : HitCountTaken / (double)HitAttemptsTaken;
        public double CritChanceTaken => HitAttemptsTaken == 0 ? 0 : CritCountTaken / (double)HitAttemptsTaken;
        public double GlanceChanceTaken => HitAttemptsTaken == 0 ? 0 : GlanceCountTaken / (double)HitAttemptsTaken;
        public double MissChanceTaken => HitAttemptsTaken == 0 ? 0 : MissCountTaken / (double)HitAttemptsTaken;
        public long DamageAbsorbed { get; protected set; }

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
                case AttackResult.Hit:
                    DamageDone += attackEvent.Amount.Value;
                    HitDamageDone += attackEvent.Amount.Value;
                    ++HitCount;
                    if (attackEvent.AttackModifier == AttackModifier.Crit)
                    {
                        ++CritCount;
                    }
                    else if (attackEvent.AttackModifier == AttackModifier.Glance)
                    {
                        ++GlanceCount;
                    }
                    break;
                case AttackResult.Missed:
                    ++MissCount;
                    break;
                case AttackResult.NanoHit:
                    DamageDone += attackEvent.Amount.Value;
                    NanoHitDamageDone += attackEvent.Amount.Value;
                    ++NanoHitCount;
                    break;
                case AttackResult.IndirectHit:
                    DamageDone += attackEvent.Amount.Value;
                    IndirectHitDamageDone += attackEvent.Amount.Value;
                    ++IndirectHitCount;
                    break;
                // No sources for events where the attack results in an absorb.
                default: throw new NotImplementedException();
            }

            if (attackEvent.DamageType.HasValue)
            {
                _damageTypeDamageDoneCounts.Increment(attackEvent.DamageType.Value, 1);
                _damageTypeDamageDones.Increment(attackEvent.DamageType.Value, attackEvent.Amount ?? 0);
            }
        }

        public void AddTargetAttackEvent(AttackEvent attackEvent)
        {
            switch (attackEvent.AttackResult)
            {
                case AttackResult.Hit:
                    DamageTaken += attackEvent.Amount.Value;
                    HitDamageTaken += attackEvent.Amount.Value;
                    ++HitCountTaken;
                    if (attackEvent.AttackModifier == AttackModifier.Crit)
                    {
                        ++CritCountTaken;
                    }
                    else if (attackEvent.AttackModifier == AttackModifier.Glance)
                    {
                        ++GlanceCountTaken;
                    }
                    break;
                case AttackResult.Missed:
                    ++MissCountTaken;
                    break;
                case AttackResult.NanoHit:
                    DamageTaken += attackEvent.Amount.Value;
                    NanoHitDamageTaken += attackEvent.Amount.Value;
                    ++NanoHitCountTaken;
                    break;
                case AttackResult.IndirectHit:
                    DamageTaken += attackEvent.Amount.Value;
                    IndirectHitDamageTaken += attackEvent.Amount.Value;
                    ++IndirectHitCountTaken;
                    break;
                case AttackResult.Absorbed:
                    DamageAbsorbed += attackEvent.Amount.Value;
                    break;
                default: throw new NotImplementedException();
            }
        }

        public void AddSelfHealEvent(HealEvent healEvent)
        {
            if (healEvent.Source != healEvent.Target)
                throw new InvalidOperationException("Use AddSourceHealEvent and AddTargetHealEvent when the source and target differ.");

            if (healEvent.HealType == HealType.RealizedHealth)
            {
                SelfHealingDone += healEvent.Amount.Value;
            }
            else throw new NotImplementedException();
        }

        public void AddSourceHealEvent(HealEvent healEvent)
        {
            if (healEvent.Source == healEvent.Target)
                throw new InvalidOperationException("Use AddSelfHealEvent for heal events where the source equals the target.");

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
                throw new InvalidOperationException("Use AddSelfHealEvent for heal events where the source equals the target.");

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
            => $"{Character}: {PercentOfTotalDamageDone:P1} of total damage.";
    }
}
