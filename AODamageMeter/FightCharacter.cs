using AODamageMeter.FightEvents;
using System;
using System.Collections.Generic;

namespace AODamageMeter
{
    public class FightCharacter
    {
        public FightCharacter(Fight fight, Character character, DateTime enteredTime)
        {
            Fight = fight;
            Character = character;
            EnteredTime = enteredTime;
        }

        public DamageMeter DamageMeter => Fight.DamageMeter;
        public Fight Fight { get; }
        public Character Character { get; }
        public string Name => Character.Name;
        public CharacterType CharacterType => Character.CharacterType;
        public string ID => Character.ID;
        public Profession Profession => Character.Profession;
        public string Organization => Character.Organization;
        public DateTime EnteredTime { get; }
        public TimeSpan ActiveDuration => Fight.LatestTime.Value - EnteredTime;

        protected readonly HashSet<FightCharacter> _pets = new HashSet<FightCharacter>();
        public IReadOnlyCollection<FightCharacter> Pets => _pets;
        public void RegisterPet(FightCharacter pet)
        {
            Character.RegisterPet(pet.Character);
            _pets.Add(pet);
        }

        protected readonly List<AttackEvent> _sourceAttackEvents = new List<AttackEvent>();
        protected readonly List<AttackEvent> _targetAttackEvents = new List<AttackEvent>();
        protected readonly List<HealEvent> _sourceHealEvents = new List<HealEvent>();
        protected readonly List<HealEvent> _targetHealEvents = new List<HealEvent>();
        protected readonly List<LevelEvent> _levelEvents = new List<LevelEvent>();
        protected readonly List<NanoEvent> _nanoEvents = new List<NanoEvent>();
        public IReadOnlyList<AttackEvent> SourceAttackEvents => _sourceAttackEvents;
        public IReadOnlyList<AttackEvent> TargetAttackEvents => _targetAttackEvents;
        public IReadOnlyList<HealEvent> SourceHealEvents => _sourceHealEvents;
        public IReadOnlyList<HealEvent> TargetHealEvents => _targetHealEvents;
        public IReadOnlyList<LevelEvent> LevelEvents => _levelEvents;
        public IReadOnlyList<NanoEvent> NanoEvents => _nanoEvents;

        public int DamageDone { get; protected set; }
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
        public double PercentOfMaxDamageDone => Fight.MaxDamageDone == 0 ? 0 : DamageDone / (double)Fight.MaxDamageDone;
        public double ActiveDPS => ActiveDuration.TotalSeconds <= 1 ? DamageDone : DamageDone / ActiveDuration.TotalSeconds;
        public double FullDPS => Fight.Duration.Value.TotalSeconds <= 1 ? DamageDone : DamageDone / Fight.Duration.Value.TotalSeconds;
        public double ActiveDPM => 60 * ActiveDPS;
        public double FullDPM => 60 * FullDPS;

        public int DamageTaken { get; protected set; }
        public int HitOnCount { get; protected set; }
        public int CritOnCount { get; protected set; }
        public int GlanceOnCount { get; protected set; }
        public int IndirectHitOnCount { get; protected set; }
        // We only know about misses where the owner is a source or target.
        public int MissOnCount { get; protected set; } 
        public int HitOnAttempts => HitOnCount + MissOnCount;
        public double HitOnChance => HitOnAttempts == 0 ? 0 : HitOnCount / (double)HitOnAttempts;
        public double CritOnChance => HitOnAttempts == 0 ? 0 : CritOnCount / (double)HitOnAttempts;
        public double GlanceOnChance => HitOnAttempts == 0 ? 0 : GlanceOnCount / (double)HitOnAttempts;
        public double MissOnChance => HitOnAttempts == 0 ? 0 : MissOnCount / (double)HitOnAttempts;
        public int DamageAbsorbed { get; protected set; }

        // We only know about healing where the owner is a source or target.
        public int HealthHealingDone { get; protected set; }
        public int NanoHealingDone { get; protected set; }

        // We only know about healing where the owner is a source or target.
        // See the comment in MeGotHealth.cs. HealthHealingReceived is an approximation for how much others have healed you.
        // HealthHealingRealized is how much you've actually been healed, by yourself, by others, by whatever.
        public int HealthHealingReceived { get; protected set; }
        public int NanoHealingReceived { get; protected set; }
        public int HealthHealingRealized { get; protected set; }

        // We only know about level events where the source is the owner (there's no target).
        public int NormalXPGained { get; protected set; }
        public int ShadowXPGained { get; protected set; }
        public int AlienXPGained { get; protected set; }
        public int ResearchXPGained { get; protected set; }
        public int PvpSoloXPGained { get; protected set; }
        public int PvpTeamXPGained { get; protected set; }

        // We only know about nano events where the source is the owner (there's no target).
        // CastAttempts isn't calculated by summing because we don't yet track all the ways a cast can complete.
        public int CastAttempts { get; protected set; }
        public int CastSuccessCount { get; protected set; }
        public int CastResistedCount { get; protected set; }
        public int CastCounteredCount { get; protected set; }
        public int CastAbortedCount { get; protected set; }
        public double CastSuccessChance => CastAttempts == 0 ? 0 : CastSuccessCount / (double)CastAttempts;
        public double CastResistedChance => CastAttempts == 0 ? 0 : CastResistedCount / (double)CastAttempts;
        public double CastCounteredChance => CastAttempts == 0 ? 0 : CastCounteredCount / (double)CastAttempts;
        public double CastAbortedChance => CastAttempts == 0 ? 0 : CastAbortedCount / (double)CastAttempts;

        public void AddSourceAttackEvent(AttackEvent attackEvent)
        {
            _sourceAttackEvents.Add(attackEvent);

            if (attackEvent.AttackResult == AttackResult.Hit)
            {
                DamageDone += attackEvent.Amount ?? 0;
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
                DamageDone += attackEvent.Amount ?? 0;
                ++IndirectHitCount;
            }
            // No sources for events where the attack results in an absorb.
            else throw new NotImplementedException();
        }

        public void AddTargetAttackEvent(AttackEvent attackEvent)
        {
            _targetAttackEvents.Add(attackEvent);

            if (attackEvent.AttackResult == AttackResult.Hit)
            {
                DamageTaken += attackEvent.Amount ?? 0;
                ++HitOnCount;
                if (attackEvent.AttackModifier == AttackModifier.Crit)
                {
                    ++CritOnCount;
                }
                else if (attackEvent.AttackModifier == AttackModifier.Glance)
                {
                    ++GlanceOnCount;
                }
            }
            else if (attackEvent.AttackResult == AttackResult.Missed)
            {
                ++MissOnCount;
            }
            else if (attackEvent.AttackResult == AttackResult.IndirectHit)
            {
                DamageTaken += attackEvent.Amount ?? 0;
                ++IndirectHitOnCount;
            }
            else if (attackEvent.AttackResult == AttackResult.Absorbed)
            {
                DamageAbsorbed += attackEvent.Amount ?? 0;
            }
            else throw new NotImplementedException();
        }

        public void AddSourceHealEvent(HealEvent healEvent)
        {
            _sourceHealEvents.Add(healEvent);

            if (healEvent.HealType == HealType.Health)
            {
                HealthHealingDone += healEvent.Amount ?? 0;
            }
            else if (healEvent.HealType == HealType.Nano)
            {
                NanoHealingDone += healEvent.Amount ?? 0;
            }
            else throw new NotImplementedException();
        }

        public void AddTargetHealEvent(HealEvent healEvent)
        {
            _targetHealEvents.Add(healEvent);

            if (healEvent.HealType == HealType.Health)
            {
                if (healEvent.Source != null)
                {
                    HealthHealingReceived += healEvent.Amount ?? 0;
                }
                else
                {
                    HealthHealingRealized += healEvent.Amount ?? 0;
                }
            }
            else if (healEvent.HealType == HealType.Nano)
            {
                NanoHealingReceived += healEvent.Amount ?? 0;
            }
            else throw new NotImplementedException();
        }

        public void AddLevelEvent(LevelEvent levelEvent)
        {
            _levelEvents.Add(levelEvent);

            switch (levelEvent.LevelType)
            {
                case LevelType.Normal: NormalXPGained += levelEvent.Amount ?? 0; break;
                case LevelType.Shadow: ShadowXPGained += levelEvent.Amount ?? 0; break;
                case LevelType.Alien: AlienXPGained += levelEvent.Amount ?? 0; break;
                case LevelType.Research: ResearchXPGained += levelEvent.Amount ?? 0; break;
                case LevelType.PvpSolo: PvpSoloXPGained += levelEvent.Amount ?? 0; break;
                case LevelType.PvpTeam: PvpTeamXPGained += levelEvent.Amount ?? 0; break;
                default: throw new NotImplementedException();
            }
        }

        public void AddNanoEvent(NanoEvent nanoEvent)
        {
            if (nanoEvent.IsStartOfCast)
            {
                ++CastAttempts;
            }
            else if (nanoEvent.CastResult.HasValue)
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
        }
    }
}
