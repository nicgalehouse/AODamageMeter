using System;
using System.Linq;

namespace AODamageMeter.UI.Helpers
{
    public static class FightCharacterExtensions
    {
        public static bool HasSpecialsDone(this FightCharacter fightCharacter)
            => DamageTypeHelpers.SpecialDamageTypes.Any(t => fightCharacter.HasDamageTypeDamageDone(t));

        /* Getting some reuse here because pets don't do specials, so same code across main and detail rows. */

        public static string GetSpecialsDoneInfo(this FightCharacter fightCharacter)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(t => fightCharacter.HasDamageTypeDamageDone(t))
                .Select(t => $@"{fightCharacter.GetAverageDamageTypeDamageDone(t).Value.Format()} dmg"
                    + $", {fightCharacter.GetSecondsPerDamageTypeHitDone(t).Value.Format()} secs / {t.GetName()}"));

        public static string GetSpecialsDoneInfo(this DamageInfo damageInfo)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(t => damageInfo.HasDamageTypeDamage(t))
                .Select(t => $@"{damageInfo.GetAverageDamageTypeDamage(t).Value.Format()} dmg / {t.GetName()}"));
    }
}
