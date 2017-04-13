using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Heal
{
    public class MeGotNano : HealEvent
    {
        public const string EventKey = "16";
        public const string EventName = "Me got nano";

        public static readonly Regex
            Normal = CreateRegex($"You got nano from {SOURCE} for {AMOUNT} points.");

        protected MeGotNano(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<MeGotNano> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var healEvent = new MeGotNano(damageMeter, fight, timestamp, description);
            healEvent.SetTargetToOwner();
            healEvent.HealType = HealType.Nano;

            if (healEvent.TryMatch(Normal, out Match match))
            {
                await healEvent.SetSource(match, 1); // TODO: is it always a player character?
                healEvent.SetAmount(match, 2);
            }
            else throw new NotSupportedException($"{EventName}: {description}");

            return healEvent;
        }
    }
}
