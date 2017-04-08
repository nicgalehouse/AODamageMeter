using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents
{
    public class OtherHitByNano : FightEvent
    {
        public const string EventKey = "04";
        public const string EventName = "Other hit by nano";

        public static readonly Regex
            Sourced =   new Regex(@"^(.+?) was attacked with nanobots from (.+?) for (\d+) points of (.+?) damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Unsourced = new Regex(@"^(.+?) was attacked with nanobots for (\d+) points of (.+?) damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft);

        protected OtherHitByNano(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<OtherHitByNano> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var fightEvent = new OtherHitByNano(damageMeter, fight, timestamp, description);

            if (fightEvent.TryMatch(Sourced, out Match match))
            {
                await fightEvent.SetSourceAndTarget(match, 2, 1);
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 3);
                fightEvent.SetDamageType(match, 4);
            }
            else if (fightEvent.TryMatch(Unsourced, out match))
            {
                await fightEvent.SetTarget(match, 1);
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 2);
                fightEvent.SetDamageType(match, 3);
            }
            else throw new NotSupportedException($"{EventKey}, {EventName}: {description}");

            return fightEvent;
        }
    }
}
