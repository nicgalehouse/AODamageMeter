using System;
using System.Linq;

namespace AODamageMeter.UI.Helpers
{
    public static class FightCharacterExtensions
    {
        public static bool HasSpecialsDone(this FightCharacter fightCharacter)
            => DamageTypeHelpers.SpecialDamageTypes.Any(t => fightCharacter.HasDamageTypeDamageDone(t));

        public static string GetSpecialsDoneInfo(this FightCharacter fightCharacter)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(t => fightCharacter.HasDamageTypeDamageDone(t))
                .Select(t => $@"{fightCharacter.GetAverageDamageTypeDamageDone(t).Value.Format()} dmg"
                    + $", {fightCharacter.GetActiveSecondsPerDamageTypeDamageDone(t).Value.Format()} secs / {t.GetName()}"));
    }
}
