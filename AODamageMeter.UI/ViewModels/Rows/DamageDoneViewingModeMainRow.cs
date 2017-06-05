using AODamageMeter.UI.Helpers;
using System.Linq;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageDoneViewingModeMainRow : ViewingModeMainRowBase
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
                    string specialsDoneInfo = Fight.HasSpecialsDone ?
$@"

{Fight.GetSpecialsDoneInfo()}" : null;

                    return
$@"{Fight.FightCharacterCount} {(Fight.FightCharacterCount == 1 ? "character": "characters")}

{Fight.WeaponHitDoneChance.FormatPercent()} weapon hit chance
  {Fight.CritDoneChance.FormatPercent()} crit chance
  {Fight.GlanceDoneChance.FormatPercent()} glance chance

{Fight.WeaponHitAttemptsDonePM.Format()} weapon hit attempts / min
  {Fight.WeaponHitsDonePM.Format()} weapon hits / min
  {Fight.CritsDonePM.Format()} crits / min
  {Fight.GlancesDonePM.Format()} glances / min
{Fight.NanoHitsDonePM.Format()} nano hits / min
{Fight.IndirectHitsDonePM.Format()} indirect hits / min
{Fight.TotalHitsDonePM.Format()} total hits / min

{Fight.WeaponDamageDonePM.Format()} ({Fight.WeaponPercentOfTotalDamageDone.FormatPercent()}) weapon dmg / min
{Fight.NanoDamageDonePM.Format()} ({Fight.NanoPercentOfTotalDamageDone.FormatPercent()}) nano dmg / min
{Fight.IndirectDamageDonePM.Format()} ({Fight.IndirectPercentOfTotalDamageDone.FormatPercent()}) indirect dmg / min
{Fight.TotalDamageDonePM.Format()} total dmg / min

{Fight.AverageWeaponDamageDone.Format()} weapon dmg / hit
  {Fight.AverageCritDamageDone.Format()} crit dmg / hit
  {Fight.AverageGlanceDamageDone.Format()} glance dmg / hit
{Fight.AverageNanoDamageDone.Format()} nano dmg / hit
{Fight.AverageIndirectDamageDone.Format()} indirect dmg / hit{specialsDoneInfo}";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            RightText = $"{Fight.TotalDamageDone.Format()} ({Fight.TotalDamageDonePM.Format()})";

            var topFightCharacters = Fight.FightCharacters
                .Where(c => !c.IsFightPet)
                .OrderByDescending(c => c.TotalDamageDonePlusPets)
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
                    _detailRowMap.Add(fightCharacter, detailRow = new DamageDoneViewingModeDetailRow(fightCharacter));
                    DetailRows.Add(detailRow);
                }
                detailRow.Update(detailRowDisplayIndex++);
            }

            base.Update();
        }
    }
}
