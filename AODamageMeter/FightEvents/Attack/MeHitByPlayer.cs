using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class MeHitByPlayer : AttackEvent
    {
        public const string EventKey = "07";
        public const string EventName = "Me hit by player";

        public static readonly Regex
            Normal     = CreateRegex($"Player {SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage."),
            Unprefixed = CreateRegex($"{SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true);

        public MeHitByPlayer(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<MeHitByPlayer> Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new MeHitByPlayer(fight, timestamp, description);
            attackEvent.SetTargetToOwner();

            if (attackEvent.TryMatch(Normal, out Match match)
                || attackEvent.TryMatch(Unprefixed, out match))
            {
                await attackEvent.SetSource(match, 1, CharacterType.PlayerCharacter);
                attackEvent.SetAmount(match, 2);
                attackEvent.SetDamageType(match, 3);
            }
            else throw new NotSupportedException($"{EventName}: {description}");

            return attackEvent;
        }
    }
}
