using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents
{
    public class YouHitOtherWithNano : FightEvent
    {
        public const string EventKey = "05";
        public const string EventName = "You hit other with nano";

        public static readonly Regex
            Normal = new Regex(@"^You hit (.+?) with nanobots for (\d+) points of (.+?) damage.$", RegexOptions.Compiled);

        protected YouHitOtherWithNano(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<YouHitOtherWithNano> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var fightEvent = new YouHitOtherWithNano(damageMeter, fight, timestamp, description);

            if (fightEvent.TryMatch(Normal, out Match match))
            {
                fightEvent.SetSourceAsOwner();
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
