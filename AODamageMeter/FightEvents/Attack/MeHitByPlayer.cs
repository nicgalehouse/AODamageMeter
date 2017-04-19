using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class MeHitByPlayer : AttackEvent
    {
        public const string EventName = "Me hit by player";

        public static readonly Regex
            Normal = CreateRegex($"(?:Player )?{SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true),
            Crit =   CreateRegex($"(?:Player )?{SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage. Critical hit!", rightToLeft: true),
            Glance = CreateRegex($"(?:Player )?{SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage. Glancing hit.", rightToLeft: true);

        public MeHitByPlayer(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static async Task<MeHitByPlayer> Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new MeHitByPlayer(fight, timestamp, description);
            attackEvent.SetTargetToOwner();
            attackEvent.AttackResult = AttackResult.Hit;

            bool crit = false, glance = false;
            if (attackEvent.TryMatch(Normal, out Match match, out bool normal)
                || attackEvent.TryMatch(Crit, out match, out crit)
                || attackEvent.TryMatch(Glance, out match, out glance))
            {
                await attackEvent.SetSource(match, 1);
                attackEvent.Source.Character.CharacterType = CharacterType.PlayerCharacter;
                attackEvent.SetAmount(match, 2);
                attackEvent.SetDamageType(match, 3);
                attackEvent.AttackModifier = crit ? AODamageMeter.AttackModifier.Crit
                    : glance ? AODamageMeter.AttackModifier.Glance
                    : (AttackModifier?)null;
            }
            else attackEvent.Unmatched = true;

            return attackEvent;
        }
    }
}
