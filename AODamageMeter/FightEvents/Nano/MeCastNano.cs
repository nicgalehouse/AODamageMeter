using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Nano
{
    public class MeCastNano : NanoEvent
    {
        public const string EventName = "Me Cast Nano";

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
            Range =       CreateRegex("Target out of range for nano program."),
            Structure =   CreateRegex("The molecular structure of this creature prohibits the use of this technology."),
            Falling =     CreateRegex("You can't execute nanoprograms while falling!"),
            Items =       CreateRegex("You can't execute nano programs on items."),
            PleaseWait =  CreateRegex("Please wait.");

        protected MeCastNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static MeCastNano Create(Fight fight, DateTime timestamp, string description)
        {
            var nanoEvent = new MeCastNano(fight, timestamp, description);
            nanoEvent.SetSourceToOwner();

            bool resisted = false, countered = false, aborted = false;
            if (nanoEvent.TryMatch(Executing, out Match match))
            {
                nanoEvent.NanoProgram = match.Groups[1].Value;
                _latestPotentialStartEvent = nanoEvent;
            }
            else if (nanoEvent.TryMatch(Success, out match, out bool success)
                || nanoEvent.TryMatch(Resisted, out match, out resisted)
                || nanoEvent.TryMatch(Countered, out match, out countered)
                || nanoEvent.TryMatch(Aborted, out match, out aborted))
            {
                nanoEvent.CastResult = success ? AODamageMeter.CastResult.Success
                    : resisted ? AODamageMeter.CastResult.Resisted
                    : countered ? AODamageMeter.CastResult.Countered
                    : AODamageMeter.CastResult.Aborted;

                // I don't think this can ever be null in the textbook case. Almost all events we log are independent
                // of one another, but here's a case where they're not. There's another case over in MeGotHealth, but
                // there the start event being null is a standard case. Here, it may only be null because of the filters
                // the user has configured in their chat windows, i.e, excluding lines with "Executing Nano Program:".
                if (_latestPotentialStartEvent != null)
                {
                    _latestPotentialStartEvent.CastResult = nanoEvent.CastResult;
                    nanoEvent.NanoProgram = _latestPotentialStartEvent.NanoProgram;

                    nanoEvent.StartEvent = _latestPotentialStartEvent;
                    _latestPotentialStartEvent.EndEvent = nanoEvent;
                    _latestPotentialStartEvent = null;
                }
            }
            else if (nanoEvent.TryMatch(Unavailable, out match)
                || nanoEvent.TryMatch(Wait, out match)
                || nanoEvent.TryMatch(Already, out match)
                || nanoEvent.TryMatch(Unable, out match)
                || nanoEvent.TryMatch(Nano, out match)
                || nanoEvent.TryMatch(Better, out match)
                || nanoEvent.TryMatch(Stand, out match)
                || nanoEvent.TryMatch(NotFound, out match)
                || nanoEvent.TryMatch(LoS, out match)
                || nanoEvent.TryMatch(Range, out match)
                || nanoEvent.TryMatch(Structure, out match)
                || nanoEvent.TryMatch(Falling, out match)
                || nanoEvent.TryMatch(Items, out match)
                || nanoEvent.TryMatch(PleaseWait, out match))
            {
                nanoEvent.IsCastUnavailable = true;
            }
            else nanoEvent.IsUnmatched = true;

            return nanoEvent;
        }
    }
}
