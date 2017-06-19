using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents
{
    // Here are some different cases to consider:

    // This is the normal case:
    // [Me Cast Nano] Executing Nano Program: Improved Healing.
    // [Me Cast Nano] Nano program executed successfully.

    // This will happen if you spam the button:
    // [Me Cast Nano] Executing Nano Program: Improved Healing.
    // [Me Cast Nano] Wait for current nano program execution to finish.
    // [Me Cast Nano] Unable to execute nano program. You can't execute this nano on the target.
    // [Me Cast Nano] Nano program executed successfully.

    // This will happen if you spam multiple buttons, sometimes:
    // [Me Cast Nano] Executing Nano Program: Lifegiving Elixir.
    // [Me Cast Nano] Executing Nano Program: Improved Healing.
    // [Me got health] You were healed for 668 points.
    // [Me Cast Nano] Nano program executed successfully.
    // [Me Cast Nano] Wait for current nano program execution to finish.

    // In the last case, LE is what succeeded, IH was never actually executing. I consider it a bug in AO's
    // logs that IH was shown there at all. But it means we could interpret the 'wait' as an end event of IH.
    // Waits aren't always end events though, as the middle case shows. There, the wait isn't associated with
    // any start event. The most important metric to get right is how many actual casts there were (no weird
    // fake casts like in the last case), and if they ended in a success, resist, counter, or abort. To achieve
    // that, we remember the latest potential start event, and mark it with one of the 4 real cast results
    // when the end event comes in. The details can get muddled when the last case happens. There, we'll log
    // it as IH succeeding, when in reality it was LE succeeding. But scenario is rare and the %s remain true.

    // Here's another one I recently found. Freak Shield is a perk, but creates a Me Cast Nano success...
    // [System] You successfully perform Freak Shield.
    // [Me Cast Nano] Nano program executed successfully.
    public class MeCastNano : FightEvent
    {
        public const string EventName = "Me Cast Nano";
        public override string Name => EventName;

        protected static MeCastNano _latestPotentialStartEvent;

        public static readonly Regex
            Executing =   CreateRegex("Executing Nano Program: (.+)."),
            Success =     CreateRegex("Nano program executed successfully."),
            Resisted =    CreateRegex("Target resisted."),
            Countered =   CreateRegex("Your target countered the nano program."),
            Aborted =     CreateRegex("Nano program aborted."),
            Unavailable = CreateRegex("Executing programs is currently unavailable."),
            Wait =        CreateRegex("Wait for current nano program execution to finish."),
            Already =     CreateRegex("Nano program failed. Already executing nanoprogram."),
            Unable =      CreateRegex("Unable to execute nano program. You can't execute this nano on the target."),
            Nano =        CreateRegex($"You need at least {AMOUNT} remaining nano energy to execute this program."),
            Better =      CreateRegex("NCU error: Better nano program already running."),
            Stand =       CreateRegex("Unable to execute nano program. You must stand still."),
            NotFound =    CreateRegex("Unable to execute nano program. Target not found."),
            LoS =         CreateRegex("Target is not in line of sight!"),
            Range1 =      CreateRegex("Target out of range for nano program."),
            Range2 =      CreateRegex("Target is outside range."),
            Structure =   CreateRegex("The molecular structure of this creature prohibits the use of this technology."),
            Falling =     CreateRegex("You can't execute nanoprograms while falling!"),
            Items =       CreateRegex("You can't execute nano programs on items."),
            PleaseWait =  CreateRegex("Please wait."),
            NCU =         CreateRegex("Target does not have enough nano controlling units \\(NCU\\) left.");

        public string NanoProgram { get; protected set; }
        public bool IsStartOfCast => EndEvent != null;
        public bool IsEndOfCast => StartEvent != null;
        public CastResult? CastResult { get; protected set; }
        public bool IsCastUnavailable { get; protected set; }

        // Both null or either one not null, but never both not null.
        // If this.StartEvent is not null, this.StartEvent.EndEvent == this.
        // If this.EndEvent is not null, this.EndEvent.StartEvent == this.
        public MeCastNano StartEvent { get; protected set; }
        public MeCastNano EndEvent { get; protected set; }

        public MeCastNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetSourceToOwner();

            bool resisted = false, countered = false, aborted = false;
            if (TryMatch(Executing, out Match match))
            {
                NanoProgram = match.Groups[1].Value;
                _latestPotentialStartEvent = this;
            }
            else if (TryMatch(Success, out match, out bool success)
                || TryMatch(Resisted, out match, out resisted)
                || TryMatch(Countered, out match, out countered)
                || TryMatch(Aborted, out match, out aborted))
            {
                CastResult = success ? AODamageMeter.CastResult.Success
                    : resisted ? AODamageMeter.CastResult.Countered // I guess resist and counter are the same?
                    : countered ? AODamageMeter.CastResult.Countered
                    : AODamageMeter.CastResult.Aborted;

                if (_latestPotentialStartEvent != null && _latestPotentialStartEvent.Fight == fight)
                {
                    _latestPotentialStartEvent.CastResult = CastResult;
                    NanoProgram = _latestPotentialStartEvent.NanoProgram;

                    StartEvent = _latestPotentialStartEvent;
                    _latestPotentialStartEvent.EndEvent = this;
                    _latestPotentialStartEvent = null;
                }
            }
            else if (TryMatch(Unavailable, out match)
                || TryMatch(Wait, out match)
                || TryMatch(Already, out match)
                || TryMatch(Unable, out match)
                || TryMatch(Nano, out match)
                || TryMatch(Better, out match)
                || TryMatch(Stand, out match)
                || TryMatch(NotFound, out match)
                || TryMatch(LoS, out match)
                || TryMatch(Range1, out match)
                || TryMatch(Range2, out match)
                || TryMatch(Structure, out match)
                || TryMatch(Falling, out match)
                || TryMatch(Items, out match)
                || TryMatch(PleaseWait, out match)
                || TryMatch(NCU, out match))
            {
                IsCastUnavailable = true;
            }
            else IsUnmatched = true;
        }
    }
}
