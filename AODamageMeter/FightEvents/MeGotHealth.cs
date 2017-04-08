using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents
{
    public class MeGotHealth : FightEvent
    {
        public const string EventKey = "15";
        public const string EventName = "Me got health";

        public static readonly Regex
            Unsourced = new Regex(@"^You were healed for (\d+) points.$", RegexOptions.Compiled),
            Sourced =   new Regex(@"^You got healed by (.+?) for (\d+) points of health.$", RegexOptions.Compiled);

        protected MeGotHealth(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<MeGotHealth> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var fightEvent = new MeGotHealth(damageMeter, fight, timestamp, description);

            if (fightEvent.TryMatch(Unsourced, out Match match))
            {
                // Usually this is from you healing yourself, which is a useful metric, so assume it's the case.
                fightEvent.SetSourceAndTargetAsOwner();
                fightEvent.ActionType = ActionType.Heal;
                fightEvent.SetAmount(match, 1);
            }
            else if (fightEvent.TryMatch(Sourced, out match))
            {
                await fightEvent.SetSource(match, 1);
                fightEvent.SetTargetAsOwner();
                fightEvent.ActionType = ActionType.Heal;
                fightEvent.SetAmount(match, 2);
            }
            else throw new NotSupportedException($"{EventName}: {description}");

            return fightEvent;
        }
    }
}
