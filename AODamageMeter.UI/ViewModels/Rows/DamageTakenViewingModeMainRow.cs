using AODamageMeter.UI.Helpers;
using System;
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
                    string specialsDoneInfo = Fight.HasSpecials ?
$@"

{Fight.GetSpecialsInfo()}" : null;

                    return
$@"{Fight.FightCharacterCount} {(Fight.FightCharacterCount == 1 ? "character": "characters")}

{Fight.WeaponHitChance.FormatPercent()} weapon hit chance
  {Fight.CritChance.FormatPercent()} crit chance
  {Fight.GlanceChance.FormatPercent()} glance chance

{Fight.WeaponHitAttemptsPM.Format()} weapon hit attempts / min
  {Fight.WeaponHitsPM.Format()} weapon hits / min
  {Fight.CritsPM.Format()} crits / min
  {Fight.GlancesPM.Format()} glances / min
{Fight.NanoHitsPM.Format()} nano hits / min
{Fight.IndirectHitsPM.Format()} indirect hits / min
{Fight.TotalHitsPM.Format()} total hits / min

{Fight.WeaponDamagePM.Format()} ({Fight.WeaponPercentOfTotalDamage.FormatPercent()}) weapon dmg / min
{Fight.NanoDamagePM.Format()} ({Fight.NanoPercentOfTotalDamage.FormatPercent()}) nano dmg / min
{Fight.IndirectDamagePM.Format()} ({Fight.IndirectPercentOfTotalDamage.FormatPercent()}) indirect dmg / min
{Fight.TotalDamagePM.Format()} total dmg / min

{Fight.AverageWeaponDamage.Format()} weapon dmg / hit
  {Fight.AverageCritDamage.Format()} crit dmg / hit
  {Fight.AverageGlanceDamage.Format()} glance dmg / hit
{Fight.AverageNanoDamage.Format()} nano dmg / hit
{Fight.AverageIndirectDamage.Format()} indirect dmg / hit{specialsDoneInfo}";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            RightText = $"{Fight.TotalDamage.Format()} ({Fight.TotalDamagePM.Format()})";

            var topFightCharacters = Fight.FightCharacters
                .OrderByDescending(c => c.TotalDamageTaken)
                .ThenBy(c => c.UncoloredName)
                .Take(6).ToArray();

            foreach (var fightCharacterDetailRow in _detailRowMap
                .Where(kvp => !topFightCharacters.Contains(kvp.Key)))
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
