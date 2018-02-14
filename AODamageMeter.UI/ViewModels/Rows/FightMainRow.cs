using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class FightMainRow : MainRowBase
    {
        private readonly Dictionary<FightCharacter, DetailRowBase> _detailRowMap = new Dictionary<FightCharacter, DetailRowBase>();

        public FightMainRow(FightViewModel fightViewModel, int displayIndex)
            : base(fightViewModel)
            => DisplayIndex = displayIndex;


        public int? FightOwnersDisplayIndex { get; private set; }
        public double? FightOwnersPercentOfTotalDamageDone { get; private set; }
        public double? FightOwnersPercentOfMaxDamageDone { get; private set; }
        public double? DisplayedPercent => Settings.Default.ShowPercentOfTotal ? FightOwnersPercentOfTotalDamageDone : FightOwnersPercentOfMaxDamageDone;

        public override string Title
            => FightViewModel.FightTitle;

        public override string UnnumberedLeftText
            => Fight.HasEnded ? $"{Fight.StartTime:hh:mm}{EnDash}{Fight.EndTime:hh:mm}, {FightViewModel.FightDuration}"
            : Fight.HasStarted ? $"{Fight.StartTime:hh:mm}{EnDash}{DateTime.Now:hh:mm}, {FightViewModel.FightDuration}"
            : $"Unstarted";

        public override string LeftTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    var counts = Fight.GetFightCharacterCounts(includeNPCs: Settings.Default.IncludeTopLevelNPCRows);

                    return counts.GetFightCharacterCountsTooltip(Title);
                }
            }
        }

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{FightOwnersDisplayIndex?.ToString() ?? EmDash}. {Owner.UncoloredName}'s Damage Done

{FightOwner?.TotalDamageDonePlusPets.ToString("N0") ?? EmDash} total dmg
{FightOwnersPercentOfTotalDamageDone.FormatPercent()} of fight's total dmg
{FightOwnersPercentOfMaxDamageDone.FormatPercent()} of fight's max dmg

{FightOwner?.WeaponDamageDonePMPlusPets.Format() ?? EmDash} ({FightOwner?.WeaponPercentOfTotalDamageDonePlusPets.FormatPercent() ?? EmDashPercent}) weapon dmg / min
{FightOwner?.NanoDamageDonePMPlusPets.Format() ?? EmDash} ({FightOwner?.NanoPercentOfTotalDamageDonePlusPets.FormatPercent() ?? EmDashPercent}) nano dmg / min
{FightOwner?.IndirectDamageDonePMPlusPets.Format() ?? EmDash} ({FightOwner?.IndirectPercentOfTotalDamageDonePlusPets.FormatPercent() ?? EmDashPercent}) indirect dmg / min
{(!FightOwner?.HasCompleteAbsorbedDamageDoneStats ?? false ? "≥ " : "")}{FightOwner?.AbsorbedDamageDonePMPlusPets.Format() ?? EmDash} ({FightOwner?.AbsorbedPercentOfTotalDamageDonePlusPets.FormatPercent() ?? EmDashPercent}) absorbed dmg / min
{FightOwner?.TotalDamageDonePMPlusPets.Format() ?? EmDash} total dmg / min

{(!FightOwner?.HasCompleteMissStatsPlusPets ?? false ? "≤ " : "")}{FightOwner?.WeaponHitDoneChancePlusPets.FormatPercent() ?? EmDashPercent} weapon hit chance
  {FightOwner?.CritDoneChancePlusPets.FormatPercent() ?? EmDashPercent} crit chance
  {FightOwner?.GlanceDoneChancePlusPets.FormatPercent() ?? EmDashPercent} glance chance"
+ (!Fight.HasObservedBlockedHits ? null : $@"
  {(!FightOwner?.HasCompleteBlockedHitStatsPlusPets ?? false ? "≥ " : "")}{FightOwner?.BlockedHitDoneChancePlusPets.FormatPercent() ?? EmDashPercent} blocked hit chance")
+ $@"

{(!FightOwner?.HasCompleteMissStatsPlusPets ?? false ? "≥ " : "")}{FightOwner?.WeaponHitAttemptsDonePMPlusPets.Format() ?? EmDash} weapon hit attempts / min
{FightOwner?.WeaponHitsDonePMPlusPets.Format() ?? EmDash} weapon hits / min
  {FightOwner?.RegularsDonePMPlusPets.Format() ?? EmDash} regulars / min
    {FightOwner?.NormalsDonePMPlusPets.Format() ?? EmDash} normals / min
    {FightOwner?.CritsDonePMPlusPets.Format() ?? EmDash} crits / min
    {FightOwner?.GlancesDonePMPlusPets.Format() ?? EmDash} glances / min"
 + (!Fight.HasObservedBlockedHits ? null : $@"
    {(!FightOwner?.HasCompleteBlockedHitStatsPlusPets ?? false ? "≥ " : "")}{FightOwner?.BlockedHitsDonePMPlusPets.Format() ?? EmDash} blocked hits / min")
+ $@"
  {FightOwner?.SpecialsDonePMPlusPets.Format() ?? EmDash} specials / min
{FightOwner?.NanoHitsDonePMPlusPets.Format() ?? EmDash} nano hits / min
{FightOwner?.IndirectHitsDonePMPlusPets.Format() ?? EmDash} indirect hits / min
{(!FightOwner?.HasCompleteAbsorbedDamageDoneStats ?? false ? "≥ " : "")}{FightOwner?.AbsorbedHitsDonePMPlusPets.Format() ?? EmDash} absorbed hits / min
{FightOwner?.TotalHitsDonePMPlusPets.Format() ?? EmDash} total hits / min

{FightOwner?.AverageWeaponDamageDonePlusPets.Format() ?? EmDash} weapon dmg / hit
  {FightOwner?.AverageRegularDamageDonePlusPets.Format() ?? EmDash} regular dmg / hit
    {FightOwner?.AverageNormalDamageDonePlusPets.Format() ?? EmDash} normal dmg / hit
    {FightOwner?.AverageCritDamageDonePlusPets.Format() ?? EmDash} crit dmg / hit
    {FightOwner?.AverageGlanceDamageDonePlusPets.Format() ?? EmDash} glance dmg / hit
  {FightOwner?.AverageSpecialDamageDonePlusPets.Format() ?? EmDash} special dmg / hit
{FightOwner?.AverageNanoDamageDonePlusPets.Format() ?? EmDash} nano dmg / hit
{FightOwner?.AverageIndirectDamageDonePlusPets.Format() ?? EmDash} indirect dmg / hit
{FightOwner?.AverageAbsorbedDamageDonePlusPets.Format() ?? EmDash} absorbed dmg / hit"
+ (!FightOwner?.HasSpecialsDonePlusPets ?? true ? null : $@"

{FightOwner.GetSpecialsDonePlusPetsInfo()}")
+ ((FightOwner?.TotalDamageDonePlusPets ?? 0) == 0 ? null : $@"

{FightOwner.GetDamageTypesDonePlusPetsInfo()}")
+ ((FightOwner?.HealthDrained ?? 0) == 0 ? null : $@"

{FightOwner.HealthDrainedPM.Format()} health drained / min
{FightOwner.NanoDrainedPM.Format()} nano drained / min");
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            if (FightOwner == null)
            {
                RightText = $"{EmDash} ({EmDash}, {EmDash})";
            }
            else
            {
                FightOwnersPercentOfTotalDamageDone = Settings.Default.IncludeTopLevelNPCRows
                    ? FightOwner.PercentPlusPetsOfFightsTotalDamageDone
                    : FightOwner.PercentPlusPetsOfFightsTotalPlayerDamageDonePlusPets;
                FightOwnersPercentOfMaxDamageDone = Settings.Default.IncludeTopLevelNPCRows
                    ? FightOwner.PercentPlusPetsOfFightsMaxDamageDonePlusPets
                    : FightOwner.PercentPlusPetsOfFightsMaxPlayerDamageDonePlusPets;
                PercentWidth = FightOwnersPercentOfMaxDamageDone ?? 0;
                RightText = $"{FightOwner.TotalDamageDonePlusPets.Format()} ({FightOwner.TotalDamageDonePMPlusPets.Format()}, {DisplayedPercent.FormatPercent()})";
            }

            var fightCharacters = Fight.FightCharacters
                .Where(c => (Settings.Default.IncludeTopLevelNPCRows || c.IsPlayerOrFightPet)
                    && (Settings.Default.IncludeTopLevelZeroDamageRows || c.TotalDamageDonePlusPets != 0))
                .Where(c => !c.IsFightPet)
                .OrderByDescending(c => c.TotalDamageDonePlusPets)
                .ThenBy(c => c.UncoloredName);
            if (FightOwner != null)
            {
                int index = 0;
                foreach (var fightCharacter in fightCharacters)
                {
                    ++index;
                    if (fightCharacter == FightOwner)
                    {
                        FightOwnersDisplayIndex = index;
                        break;
                    }
                }
            }

            var topFightCharacters = fightCharacters
                .Take(Settings.Default.MaxNumberOfDetailRows).ToArray();

            foreach (var noLongerTopFightCharacter in _detailRowMap.Keys
                .Except(topFightCharacters).ToArray())
            {
                DetailRows.Remove(_detailRowMap[noLongerTopFightCharacter]);
                _detailRowMap.Remove(noLongerTopFightCharacter);
            }

            int detailRowDisplayIndex = 1;
            foreach (var fightCharacter in topFightCharacters)
            {
                if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                {
                    _detailRowMap.Add(fightCharacter, detailRow = new DamageDoneViewingModeDetailRow(FightViewModel, fightCharacter));
                    DetailRows.Add(detailRow);
                }
                detailRow.Update(detailRowDisplayIndex++);
            }

            IconPath = Owner.Profession.GetIconPath();
            Color = Owner.Profession.GetColor();

            base.Update(displayIndex);
        }

        public override bool TryCopyAndScriptProgressedRowsInfo()
            => CopyAndScriptProgressedRowsInfo(FightViewModel.GetUpdatedDamageDoneRows());
    }
}
