using System;
using System.Linq;

namespace AODamageMeter.UI.Helpers
{
    public static class DamageMeterExtensions
    {
        public static string GetProfessionsInfo(this FightCharacterCounts counts)
            => string.Join(Environment.NewLine, Profession.All
                .Where(counts.HasProfession)
                .Select(p => new
                {
                    profession = p,
                    count = counts.GetProfessionCount(p),
                    averageLevel = counts.GetAverageProfessionLevel(p),
                    averageAlienLevel = counts.GetAverageProfessionAlienLevel(p)
                })
                .Select(a => $"{a.count} {a.profession}{(a.count == 1 ? "" : "s")}, {a.averageLevel.Format()}/{a.averageAlienLevel.Format()}"));

        public static string GetSpecialsDoneInfo(this FightDamageDoneStats stats)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(stats.HasDamageTypeDamage)
                .Select(t => $@"{stats.GetAverageDamageTypeDamage(t).Value.Format()} dmg"
                    + $", {stats.GetSecondsPerDamageTypeHit(t).Value.Format()} secs / {t.GetName()}"));

        public static string GetSpecialsTakenInfo(this FightDamageTakenStats stats)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(stats.HasDamageTypeDamage)
                .Select(t => $@"{stats.GetAverageDamageTypeDamage(t).Value.Format()} dmg"
                    + $", {stats.GetSecondsPerDamageTypeHit(t).Value.Format()} secs / {t.GetName()}"));

        public static string GetDamageTypesDoneInfo(this FightDamageDoneStats stats)
            => string.Join(Environment.NewLine, DamageTypeHelpers.AllDamageTypes
                .Where(stats.HasDamageTypeDamage)
                .OrderByDescending(stats.GetDamageTypeDamage)
                .Select(t => $@"{stats.GetPercentDamageTypeDamage(t).FormatPercent()} {t.GetName()} dmg, {stats.GetPercentDamageTypeHits(t).FormatPercent()} of hits"));

        public static string GetDamageTypesTakenInfo(this FightDamageTakenStats stats)
            => string.Join(Environment.NewLine, DamageTypeHelpers.AllDamageTypes
                .Where(stats.HasDamageTypeDamage)
                .OrderByDescending(stats.GetDamageTypeDamage)
                .Select(t => $@"{stats.GetPercentDamageTypeDamage(t).FormatPercent()} {t.GetName()} dmg, {stats.GetPercentDamageTypeHits(t).FormatPercent()} of hits"));

        public static string GetSpecialsDoneInfo(this FightCharacter fightCharacter)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(fightCharacter.HasDamageTypeDamageDone)
                .Select(t => $@"{fightCharacter.GetAverageDamageTypeDamageDone(t).Value.Format()} dmg"
                    + $", {fightCharacter.GetSecondsPerDamageTypeHitDone(t).Value.Format()} secs / {t.GetName()}"));

        public static string GetSpecialsDonePlusPetsInfo(this FightCharacter fightCharacter)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(fightCharacter.HasDamageTypeDamageDonePlusPets)
                .Select(t => $@"{fightCharacter.GetAverageDamageTypeDamageDonePlusPets(t).Value.Format()} dmg"
                    + $", {fightCharacter.GetSecondsPerDamageTypeHitDonePlusPets(t).Value.Format()} secs / {t.GetName()}"));

        public static string GetSpecialsTakenInfo(this FightCharacter fightCharacter)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(fightCharacter.HasDamageTypeDamageTaken)
                .Select(t => $@"{fightCharacter.GetAverageDamageTypeDamageTaken(t).Value.Format()} dmg"
                    + $", {fightCharacter.GetSecondsPerDamageTypeHitTaken(t).Value.Format()} secs / {t.GetName()}"));

        public static string GetDamageTypesDoneInfo(this FightCharacter fightCharacter)
            => string.Join(Environment.NewLine, DamageTypeHelpers.AllDamageTypes
                .Where(fightCharacter.HasDamageTypeDamageDone)
                .OrderByDescending(fightCharacter.GetDamageTypeDamageDone)
                .Select(t => $@"{fightCharacter.GetPercentDamageTypeDamageDone(t).FormatPercent()} {t.GetName()} dmg, {fightCharacter.GetPercentDamageTypeHitsDone(t).FormatPercent()} of hits"));

        public static string GetDamageTypesDonePlusPetsInfo(this FightCharacter fightCharacter)
            => string.Join(Environment.NewLine, DamageTypeHelpers.AllDamageTypes
                .Where(fightCharacter.HasDamageTypeDamageDonePlusPets)
                .OrderByDescending(fightCharacter.GetDamageTypeDamageDonePlusPets)
                .Select(t => $@"{fightCharacter.GetPercentDamageTypeDamageDonePlusPets(t).FormatPercent()} {t.GetName()} dmg, {fightCharacter.GetPercentDamageTypeHitsDonePlusPets(t).FormatPercent()} of hits"));

        public static string GetDamageTypesTakenInfo(this FightCharacter fightCharacter)
            => string.Join(Environment.NewLine, DamageTypeHelpers.AllDamageTypes
                .Where(fightCharacter.HasDamageTypeDamageTaken)
                .OrderByDescending(fightCharacter.GetDamageTypeDamageTaken)
                .Select(t => $@"{fightCharacter.GetPercentDamageTypeDamageTaken(t).FormatPercent()} {t.GetName()} dmg, {fightCharacter.GetPercentDamageTypeHitsTaken(t).FormatPercent()} of hits"));

        public static string GetSpecialsInfo(this DamageInfo damageInfo)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(damageInfo.HasDamageTypeDamage)
                .Select(t => $@"{damageInfo.GetAverageDamageTypeDamage(t).Value.Format()} dmg / {t.GetName()}"));

        public static string GetSpecialsPlusPetsInfo(this DamageInfo damageInfo)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(damageInfo.HasDamageTypeDamagePlusPets)
                .Select(t => $@"{damageInfo.GetAverageDamageTypeDamagePlusPets(t).Value.Format()} dmg / {t.GetName()}"));

        public static string GetDamageTypesInfo(this DamageInfo damageInfo)
            => string.Join(Environment.NewLine, DamageTypeHelpers.AllDamageTypes
                .Where(damageInfo.HasDamageTypeDamage)
                .OrderByDescending(damageInfo.GetDamageTypeDamage)
                .Select(t => $@"{damageInfo.GetPercentDamageTypeDamage(t).FormatPercent()} {t.GetName()} dmg, {damageInfo.GetPercentDamageTypeHits(t).FormatPercent()} of hits"));

        public static string GetDamageTypesPlusPetsInfo(this DamageInfo damageInfo)
            => string.Join(Environment.NewLine, DamageTypeHelpers.AllDamageTypes
                .Where(damageInfo.HasDamageTypeDamagePlusPets)
                .OrderByDescending(damageInfo.GetDamageTypeDamagePlusPets)
                .Select(t => $@"{damageInfo.GetPercentDamageTypeDamagePlusPets(t).FormatPercent()} {t.GetName()} dmg, {damageInfo.GetPercentDamageTypeHitsPlusPets(t).FormatPercent()} of hits"));

        public static string GetScriptFolderPath(this DamageMeter damageMeter)
            => $"{damageMeter.LogFilePath.Substring(0, damageMeter.LogFilePath.IndexOf("\\Prefs\\"))}\\scripts";

        public static string GetScriptFilePath(this DamageMeter damageMeter)
            => $"{damageMeter.LogFilePath.Substring(0, damageMeter.LogFilePath.IndexOf("\\Prefs\\"))}\\scripts\\aodm";
    }
}
