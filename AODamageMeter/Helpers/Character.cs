using System;
using System.Collections.Generic;

namespace AODamageMeter.Helpers
{
    public class Character
    {
        public List<Event> DamageTakenEvents = new List<Event>();
        public List<Event> DamageDoneEvents = new List<Event>();
        public List<Event> HealingDoneEvents = new List<Event>();
        public List<Event> HealingReceivedEvents = new List<Event>();
        public List<Event> AbsorbEvents = new List<Event>();

        public double ActiveSeconds;

        public long TimeOfFirstEvent;
        public long CurrentTime;

        public string Name;
        public Profession Profession;

        public int DamageDone;
        public int DamageTaken;
        public int CriticalStrikeCount;
        public int MissCount;
        public int HealingDone;
        public int HealingReceived;
        public int AbsorbAmount;
        public int HitAttempts;
        public int GlanceCount;

        public double CriticalStrikeChance;
        public double PercentOfDamageDone;
        public double PercentOfMaxDamage;
        public double DPSrelativeToPlayerStart;
        public double DPSRelativeToFightStart;
        public double MissChance;

        public Character(Event loggedEvent, bool isSource, long elapsedTime)
        {
            Name = isSource ? loggedEvent.Source
                : loggedEvent.Target;

            Profession = Professions.SetProfession(Name);

            TimeOfFirstEvent = elapsedTime;

            AddEvent(loggedEvent, isSource);
        }

        public void AddEvent(Event loggedEvent, bool isSource)
        {
            if (isSource)
            {
                if (loggedEvent.ActionType == "Damage" || loggedEvent.ActionType == "Nano")
                {
                    DamageDoneEvents.Add(loggedEvent);
                    HitAttempts++;
                    DamageDone += loggedEvent.Amount;

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
