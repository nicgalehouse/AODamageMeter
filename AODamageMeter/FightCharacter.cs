using AODamageMeter.FightEvents;
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

        protected readonly List<AttackEvent> _sourceAttackEvents = new List<AttackEvent>();
        protected readonly List<AttackEvent> _targetAttackEvents = new List<AttackEvent>();
        protected readonly List<HealEvent> _selfHealEvents = new List<HealEvent>();
        protected readonly List<HealEvent> _sourceHealEvents = new List<HealEvent>();
        protected readonly List<HealEvent> _targetHealEvents = new List<HealEvent>();
        protected readonly List<LevelEvent> _levelEvents = new List<LevelEvent>();
        protected readonly List<NanoEvent> _nanoEvents = new List<NanoEvent>();
        public IReadOnlyList<AttackEvent> SourceAttackEvents => _sourceAttackEvents;
        public IReadOnlyList<AttackEvent> TargetAttackEvents => _targetAttackEvents;
        public IReadOnlyList<HealEvent> SelfHealEvents => _selfHealEvents;
        public IReadOnlyList<HealEvent> SourceHealEvents => _sourceHealEvents;
        public IReadOnlyList<HealEvent> TargetHealEvents => _targetHealEvents;
        public IReadOnlyList<LevelEvent> LevelEvents => _levelEvents;
        public IReadOnlyList<NanoEvent> NanoEvents => _nanoEvents;

        public int DamageDone { get; protected set; }
        public int DamageDonePlusPets => DamageDone + FightPets.Sum(p => p.DamageDone);
        public int HitCount { get; protected set; }
        public int CritCount { get; protected set; }
        public int GlanceCount { get; protected set; }
        public int IndirectHitCount { get; protected set; }
        // We only know about misses where the owner is a source or target.
        public int MissCount { get; protected set; }
        public int HitAttempts => HitCount + MissCount;
        public double HitChance => HitAttempts == 0 ? 0 : HitCount / (double)HitAttempts;
        public double CritChance => HitAttempts == 0 ? 0 : CritCount / (double)HitAttempts;
        public double GlanceChance => HitAttempts == 0 ? 0 : GlanceCount / (double)HitAttempts;
        public double MissChance => HitAttempts == 0 ? 0 : MissCount / (double)HitAttempts;
        public double PercentOfTotalDamageDone => Fight.TotalDamageDone == 0 ? 0 : DamageDone / (double)Fight.TotalDamageDone;
        public double PercentPlusPetsOfTotalDamageDone => Fight.TotalDamageDone == 0 ? 0 : DamageDonePlusPets / (double)Fight.TotalDamageDone;
        public double PercentOfMaxDamageDone => Fight.MaxDamageDone == 0 ? 0 : DamageDone / (double)Fight.MaxDamageDone;
        public double PercentOfMaxDamageDonePlusPets => Fight.MaxDamageDonePlusPets == 0 ? 0 : DamageDone / (double)Fight.MaxDamageDonePlusPets;
        public double PercentPlusPetsOfMaxDamageDonePlusPets => Fight.MaxDamageDonePlusPets == 0 ? 0 : DamageDonePlusPets / (double)Fight.MaxDamageDonePlusPets;
        public double ActiveDPS => ActiveDuration.TotalSeconds <= 1 ? DamageDone : DamageDone / ActiveDuration.TotalSeconds;
        public double ActiveDPSPlusPets => ActiveDurationPlusPets.TotalSeconds <= 1 ? DamageDonePlusPets : DamageDonePlusPets / ActiveDurationPlusPets.TotalSeconds;
        public double FullDPS => Fight.Duration.Value.TotalSeconds <= 1 ? DamageDone : DamageDone / Fight.Duration.Value.TotalSeconds;
        public double ActiveDPM => 60 * ActiveDPS;
        public double ActiveDPMPlusPets => 60 * ActiveDPSPlusPets;
        public double FullDPM => 60 * FullDPS;

        public int DamageTaken { get; protected set; }
        public int HitCountTaken { get; protected set; }
        public int CritCountTaken { get; protected set; }
        public int GlanceCountTaken { get; protected set; }
        public int IndirectHitCountTaken { get; protected set; }
        // We only know about misses where the owner is a source or target.
        public int MissCountTaken { get; protected set; }
        public int HitAttemptsTaken => HitCountTaken + MissCountTaken;
        public double HitChanceTaken => HitAttemptsTaken == 0 ? 0 : HitCountTaken / (double)HitAttemptsTaken;
        public double CritChanceTaken => HitAttemptsTaken == 0 ? 0 : CritCountTaken / (double)HitAttemptsTaken;
        public double GlanceChanceTaken => HitAttemptsTaken == 0 ? 0 : GlanceCountTaken / (double)HitAttemptsTaken;
        public double MissChanceTaken => HitAttemptsTaken == 0 ? 0 : MissCountTaken / (double)HitAttemptsTaken;
        public int DamageAbsorbed { get; protected set; }

        // We only know about healing where the owner is a source or target. When the owner is the source, we don't know
        // about realized healing. So overhealing stats only when non-owner source and owner target--non-owner source has
        // OverhealingDone (to owner) stats, owner target has OverhealingTaken (from non-owners) stats. Since owner must
        // be source or target, it follows we only have SelfHealingDone for the owner.
        public int SelfHealingDone { get; protected set; }
        public int PotentialHealingDone { get; protected set; }
        public int RealizedHealingDone { get; protected set; }
        public int OverhealingDone { get; protected set; }
        public int NanoHealingDone { get; protected set; }
        public int PotentialHealingTaken { get; protected set; }
        public int RealizedHealingTaken { get; protected set; }
        public int OverhealingTaken { get; protected set; }
        public int NanoHealingTaken { get; protected set; }

        // We only know about level events where the source is the owner (there's no target).
        public int NormalXPGained { get; protected set; }
        public int ShadowXPGained { get; protected set; }
        public int AlienXPGained { get; protected set; }
        public int ResearchXPGained { get; protected set; }
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
            if (attackEvent.AttackResult == AttackResult.Hit)
            {
                DamageDone += attackEvent.Amount.Value;
                ++HitCount;
                if (attackEvent.AttackModifier == AttackModifier.Crit)
                {
                    ++CritCount;
                }
                else if (attackEvent.AttackModifier == AttackModifier.Glance)
                {
                    ++GlanceCount;
                }
            }
            else if (attackEvent.AttackResult == AttackResult.Missed)
            {
                ++MissCount;
            }
            else if (attackEvent.AttackResult == AttackResult.IndirectHit)
            {
                DamageDone += attackEvent.Amount.Value;
                ++IndirectHitCount;
            }
            // No sources for events where the attack results in an absorb.
            else throw new NotImplementedException();

            _sourceAttackEvents.Add(attackEvent);
        }

        public void AddTargetAttackEvent(AttackEvent attackEvent)
        {
            if (attackEvent.AttackResult == AttackResult.Hit)
            {
                DamageTaken += attackEvent.Amount.Value;
                ++HitCountTaken;
                if (attackEvent.AttackModifier == AttackModifier.Crit)
                {
                    ++CritCountTaken;
                }
                else if (attackEvent.AttackModifier == AttackModifier.Glance)
                {
                    ++GlanceCountTaken;
                }
            }
            else if (attackEvent.AttackResult == AttackResult.Missed)
            {
                ++MissCountTaken;
            }
            else if (attackEvent.AttackResult == AttackResult.IndirectHit)
            {
                DamageTaken += attackEvent.Amount.Value;
                ++IndirectHitCountTaken;
            }
            else if (attackEvent.AttackResult == AttackResult.Absorbed)
            {
                DamageAbsorbed += attackEvent.Amount.Value;
            }
            else throw new NotImplementedException();

            _targetAttackEvents.Add(attackEvent);
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

            _selfHealEvents.Add(healEvent);
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

            _sourceHealEvents.Add(healEvent);
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

            _targetHealEvents.Add(healEvent);
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

            _levelEvents.Add(levelEvent);
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

            _nanoEvents.Add(nanoEvent);
        }

        public override string ToString()
            => $"{Character}: {PercentOfTotalDamageDone:P1} of total damage.";
    }
}
