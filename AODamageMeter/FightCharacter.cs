using AODamageMeter.FightEvents;
using AODamageMeter.Helpers;
using System;
using System.Collections.Generic;
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
        }

        public DamageMeter DamageMeter => Fight.DamageMeter;
        public Fight Fight { get; }
        public Character Character { get; }
        public DateTime EnteredTime { get; }

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

        protected readonly HashSet<FightCharacter> _pets = new HashSet<FightCharacter>();
        public IReadOnlyCollection<FightCharacter> Pets => _pets;
        public void RegisterPet(FightCharacter pet)
        {
            Character.RegisterPet(pet.Character);
            _pets.Add(pet);
        }

        public int DamageDone { get; protected set; }
        public int HitCount { get; protected set; }
        public int CritCount { get; protected set; }
        public int GlanceCount { get; protected set; }
        public int IndirectHitCount { get; protected set; }
        public int MissCount { get; protected set; } // Note we only know about misses where the owner is a source or target.
        public int HitAttempts => HitCount + MissCount;
        public double HitChance => HitAttempts == 0 ? 0 : 100 * (HitCount / (double)HitAttempts);
        public double CritChance => HitAttempts == 0 ? 0 : 100 * (CritCount / (double)HitAttempts);
        public double GlanceChance => HitAttempts == 0 ? 0 : 100 * (GlanceCount / (double)HitAttempts);
        public double MissChance => HitAttempts == 0 ? 0 : 100 * (MissCount / (double)HitAttempts);

        public int DamageTaken { get; protected set; }
        public int HitOnCount { get; protected set; }
        public int CritOnCount { get; protected set; }
        public int GlanceOnCount { get; protected set; }
        public int IndirectHitOnCount { get; protected set; }
        public int MissOnCount { get; protected set; } // Note we only know about misses where the owner is a source or target.
        public int HitOnAttempts => HitOnCount + MissOnCount;
        public double HitOnChance => HitOnAttempts == 0 ? 0 : 100 * (HitOnCount / (double)HitOnAttempts);
        public double CritOnChance => HitOnAttempts == 0 ? 0 : 100 * (CritOnCount / (double)HitOnAttempts);
        public double GlanceOnChance => HitOnAttempts == 0 ? 0 : 100 * (GlanceOnCount / (double)HitOnAttempts);
        public double MissOnChance => HitOnAttempts == 0 ? 0 : 100 * (MissOnCount / (double)HitOnAttempts);
        public int DamageAbsorbed { get; protected set; }

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
        }

        public void AddHealEvent(HealEvent healEvent)
        {

        }

        public void AddLevelEvent(LevelEvent levelEvent)
        {

        }

        public void AddNanoEvent(AttackEvent nanoEvent)
        {

        }

        public void AddSystemEvent(SystemEvent systemEvent)
        {

        }

        public double ActiveSeconds;

        public long CurrentTime;


        public int HealingDone;
        public int HealingReceived;

        public double PercentOfDamageDone;
        public double PercentOfMaxDamage;
        public double DPSrelativeToPlayerStart;
        public double DPSRelativeToFightStart;

        public void AddEvent(FightEvent loggedEvent, bool isSource)
        {
            if (isSource)
            {
                if (loggedEvent.ActionType == "Damage" || loggedEvent.ActionType == "Nano")
                {
                    DamageDoneEvents.Add(loggedEvent);
                    HitAttempts++;

                    if (loggedEvent.Modifier == "Crit")
                        CriticalStrikeCount++;

                    if (loggedEvent.Modifier == "Glance")
                        GlanceCount++;

                    if (loggedEvent.Modifier == "Miss")
                        MissCount++;

                    CriticalStrikeChance = 100 * (CriticalStrikeChance / HitAttempts);
                    MissChance = 100 * (MissCount / HitAttempts);
                }
                else if (loggedEvent.ActionType == "Heal")
                {
                    HealingDone += loggedEvent.Amount;
                    HealingDoneEvents.Add(loggedEvent);
                }
                else if (loggedEvent.ActionType == "Absorb")
                {
                    AbsorbEvents.Add(loggedEvent);
                    AbsorbAmount += loggedEvent.Amount;
                }
            }
            else
            {
                if (loggedEvent.ActionType == "Damage" || loggedEvent.ActionType == "Nano")
                {
                    DamageTakenEvents.Add(loggedEvent);
                    DamageTaken += loggedEvent.Amount;
                }
                else if (loggedEvent.ActionType == "Heal")
                {
                    HealingReceivedEvents.Add(loggedEvent);
                    HealingReceived += loggedEvent.Amount;
                }
            }
        }

        public void Update(long currentTime)
        {
            ActiveSeconds = (double)(currentTime - TimeOfFirstEvent) / 1000L;
            DPSrelativeToPlayerStart = Math.Round(ActiveSeconds > 1 ? DamageDone / ActiveSeconds : DamageDone, 0);
            DPSRelativeToFightStart = Math.Round(CurrentTime > 1 ? (double)DamageDone / CurrentTime / 1000L : DamageDone, 0);
        }

        public void SetPercentOfMaxDamage(int maxDamageDone)
        {
            PercentOfMaxDamage = (double)DamageDone / maxDamageDone;
        }

        public void SetPercentOfDamageDone(int totalDamageDone)
        {
            PercentOfDamageDone = Math.Round(100 * (double)DamageDone / totalDamageDone, 2);
        }
    }
}
