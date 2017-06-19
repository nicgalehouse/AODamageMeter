using AODamageMeter.FightEvents;
using AODamageMeter.Helpers;
using System;

namespace AODamageMeter
{
    public class CastInfo
    {
        public CastInfo(FightCharacter source, string nanoProgram)
        {
            Source = source;
            NanoProgram = nanoProgram;
        }

        public DamageMeter DamageMeter => Source.DamageMeter;
        public FightCharacter Source { get; }
        public string NanoProgram { get; }

        public int CastAttempts => CastSuccesses + CastCountereds + CastAborteds;
        public int CastSuccesses { get; protected set; }
        public int CastCountereds { get; protected set; }
        public int CastAborteds { get; protected set; }
        public double? CastSuccessChance => CastSuccesses / CastAttempts.NullIfZero();
        public double? CastCounteredChance => CastCountereds / CastAttempts.NullIfZero();
        public double? CastAbortedChance => CastAborteds / CastAttempts.NullIfZero();

        public double CastAttemptsPM => CastAttempts / Source.ActiveDuration.TotalMinutes;
        public double CastSuccessesPM => CastSuccesses / Source.ActiveDuration.TotalMinutes;
        public double CastCounteredsPM => CastCountereds / Source.ActiveDuration.TotalMinutes;
        public double CastAbortedsPM => CastAborteds / Source.ActiveDuration.TotalMinutes;

        public double? PercentOfSourcesCastSuccesses => CastSuccesses / Source.CastSuccesses.NullIfZero();
        public double? PercentOfSourcesMaxCastSuccesses => CastSuccesses / Source.MaxCastSuccesses.NullIfZero();

        public void AddCastEvent(MeCastNano castEvent)
        {
            switch (castEvent.CastResult.Value)
            {
                case CastResult.Success: ++CastSuccesses; break;
                case CastResult.Countered: ++CastCountereds; break;
                case CastResult.Aborted: ++CastAborteds; break;
                default: throw new NotImplementedException();
            }
        }
    }
}
