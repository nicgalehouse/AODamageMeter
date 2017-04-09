using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Heal
{
    public class MeGotHealth : HealEvent
    {
        public const string EventKey = "15";
        public const string EventName = "Me got health";

        public static readonly Regex
            Unsourced = CreateRegex(@"You were healed for (\d+) points."),
            Sourced =   CreateRegex(@"You got healed by (.+?) for (\d+) points of health.");

        protected MeGotHealth(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<MeGotHealth> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var healEvent = new MeGotHealth(damageMeter, fight, timestamp, description);
            healEvent.SetTargetToOwner();
            healEvent.HealType = HealType.Health;

            if (healEvent.TryMatch(Unsourced, out Match match))
            {
                // Usually this is from you healing yourself, which is a useful metric, so assume it's the case.
                healEvent.SetSourceToOwner();
                healEvent.SetAmount(match, 1);
            }
            else if (healEvent.TryMatch(Sourced, out match))
            {
                await healEvent.SetSource(match, 1);
                healEvent.SetAmount(match, 2);
            }
            else throw new NotSupportedException($"{EventName}: {description}");

            return healEvent;
        }
    }
}
