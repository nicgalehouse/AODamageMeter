using System;
using System.Linq;

namespace AODamageMeter.UI.Helpers
{
    public static class DamageMeterExtensions
    {
        public static string GetProfessionsInfo(this Fight fight)
            => string.Join(Environment.NewLine, Profession.All
                .Where(fight.HasProfession)
                .Select(p => new { profession = p, count = fight.GetProfessionCount(p), averageLevel = fight.GetAverageProfessionLevel(p).Value })
                .Select(a => $"{a.count} ({(a.count / (double)fight.PlayerCount).FormatPercent()}) {a.profession}{(a.count == 1 ? "" : "s")}, {a.averageLevel.Format()} avg level"));

        public static string GetSpecialsDoneInfo(this Fight fight)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(t => fight.HasDamageTypeDamageDone(t))
                .Select(t => $@"{fight.GetAverageDamageTypeDamageDone(t).Value.Format()} dmg"
                    + $", {fight.GetSecondsPerDamageTypeHitDone(t).Value.Format()} secs / {t.GetName()}"));

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
