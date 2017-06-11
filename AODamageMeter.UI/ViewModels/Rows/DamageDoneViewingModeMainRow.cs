using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageDoneViewingModeMainRow : ViewingModeMainRowBase
    {
        public DamageDoneViewingModeMainRow(Fight fight)
            : base(ViewingMode.DamageDone, "Damage Done", 1, "/Icons/DamageDone.png", Color.FromRgb(91, 84, 183), fight)
        { }

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight.DamageMeter)
                {
                    var stats = Fight.GetDamageDoneStats(
                        includeNPCs: Settings.Default.ShowTopLevelNPCRows,
                        includeZeroDamageDones: Settings.Default.ShowTopLevelZeroDamageRows);

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

{stats.GetSpecialsDoneInfo()}");
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            RightText = Settings.Default.ShowTopLevelNPCRows
                ? $"{Fight.TotalDamageDone.Format()} ({Fight.TotalDamageDonePM.Format()})"
                : $"{Fight.TotalPlayerDamageDonePlusPets.Format()} ({Fight.TotalPlayerDamageDonePMPlusPets.Format()})";

            var topFightCharacters = Fight.FightCharacters
                .Where(c => (Settings.Default.ShowTopLevelNPCRows || !c.IsNPC)
                    && (Settings.Default.ShowTopLevelZeroDamageRows || c.TotalDamageDonePlusPets != 0))
                .Where(c => !c.IsFightPet)
                .OrderByDescending(c => c.TotalDamageDonePlusPets)
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
                    _detailRowMap.Add(fightCharacter, detailRow = new DamageDoneViewingModeDetailRow(fightCharacter));
                    DetailRows.Add(detailRow);
                }
                detailRow.Update(detailRowDisplayIndex++);
            }

            base.Update();
        }
    }
}
