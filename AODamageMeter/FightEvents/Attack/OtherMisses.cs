using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class OtherMisses : AttackEvent
    {
        public const string EventName = "Other misses";

        public static readonly Regex
            Normal =  CreateRegex($"{SOURCE} tried to hit you, but missed!", rightToLeft: true),
            Special = CreateRegex($"{SOURCE} tries to attack you with {DAMAGETYPE}, but misses!", rightToLeft: true);

        public OtherMisses(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static OtherMisses Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new OtherMisses(fight, timestamp, description);
            attackEvent.SetTargetToOwner();
            attackEvent.AttackResult = AttackResult.Missed;

            if (attackEvent.TryMatch(Normal, out Match match))
            {
                attackEvent.SetSource(match, 1);
            }
            else if (attackEvent.TryMatch(Special, out match))
            {
                attackEvent.SetSource(match, 1);
                attackEvent.SetDamageType(match, 2);
            }
            else attackEvent.IsUnmatched = true;

            return attackEvent;
        }
    }
}
