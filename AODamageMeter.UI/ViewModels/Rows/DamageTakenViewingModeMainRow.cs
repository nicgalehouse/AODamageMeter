using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageTakenViewingModeMainRow : ViewingModeMainRowBase
    {
        public DamageTakenViewingModeMainRow(Fight fight)
            : base(ViewingMode.DamageTaken, "Damage Taken", 2, "/Icons/DamageTaken.png", Color.FromRgb(88, 166, 86), fight)
        { }

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight.DamageMeter)
                {
                    var stats = Fight.GetDamageTakenStats(
                        includeNPCs: Settings.Default.ShowTopLevelNPCRows,
                        includeZeroDamageTakens: Settings.Default.ShowTopLevelZeroDamageRows);

                    return
$@"{stats.FightCharacterCount} {(stats.FightCharacterCount == 1 ? "character": "characters")}

{stats.TotalDamage.ToString("N0")} total dmg

{stats.WeaponDamagePM.Format()} ({stats.WeaponPercentOfTotalDamage.FormatPercent()}) weapon dmg / min
{stats.NanoDamagePM.Format()} ({stats.NanoPercentOfTotalDamage.FormatPercent()}) nano dmg / min
{stats.IndirectDamagePM.Format()} ({stats.IndirectPercentOfTotalDamage.FormatPercent()}) indirect dmg / min
{stats.TotalDamagePM.Format()} total dmg / min

{stats.WeaponHitChance.FormatPercent()} weapon hit chance
  {stats.CritChance.FormatPercent()} crit chance
  {stats.GlanceChance.FormatPercent()} glance chance

{stats.WeaponHitAttemptsPM.Format()} weapon hit attempts / min
  {stats.WeaponHitsPM.Format()} weapon hits / min
  {stats.CritsPM.Format()} crits / min
  {stats.GlancesPM.Format()} glances / min
{stats.NanoHitsPM.Format()} nano hits / min
{stats.IndirectHitsPM.Format()} indirect hits / min
{stats.TotalHitsPM.Format()} total hits / min

{stats.AverageWeaponDamage.Format()} weapon dmg / hit
  {stats.AverageCritDamage.Format()} crit dmg / hit
  {stats.AverageGlanceDamage.Format()} glance dmg / hit
{stats.AverageNanoDamage.Format()} nano dmg / hit
{stats.AverageIndirectDamage.Format()} indirect dmg / hit"
+ (!stats.HasSpecials ? null : $@"

{stats.GetSpecialsTakenInfo()}");
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            RightText = Settings.Default.ShowTopLevelNPCRows
                ? $"{Fight.TotalDamageTaken.Format()} ({Fight.TotalDamageTakenPM.Format()})"
                : $"{Fight.TotalPlayerOrPetDamageTaken.Format()} ({Fight.TotalPlayerOrPetDamageTakenPM.Format()})";

            var topFightCharacters = Fight.FightCharacters
                .Where(c => (Settings.Default.ShowTopLevelNPCRows || !c.IsNPC)
                    && (Settings.Default.ShowTopLevelZeroDamageRows || c.TotalDamageTaken != 0))
                .OrderByDescending(c => c.TotalDamageTaken)
                .ThenBy(c => c.UncoloredName)
                .Take(6).ToArray();

            foreach (var fightCharacterDetailRow in _detailRowMap
                .Where(kvp => !topFightCharacters.Contains(kvp.Key)).ToArray())
            {
                _detailRowMap.Remove(fightCharacterDetailRow.Key);
                DetailRows.Remove(fightCharacterDetailRow.Value);
            }

            int detailRowDisplayIndex = 1;
            foreach (var fightCharacter in topFightCharacters)
            {
                if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                {
                    _detailRowMap.Add(fightCharacter, detailRow = new DamageTakenViewingModeDetailRow(fightCharacter));
                    DetailRows.Add(detailRow);
                }
                detailRow.Update(detailRowDisplayIndex++);
            }

            base.Update();
        }
    }
}
