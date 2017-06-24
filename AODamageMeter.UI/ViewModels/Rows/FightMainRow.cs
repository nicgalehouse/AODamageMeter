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
{FightOwner?.TotalDamageDonePMPlusPets.Format() ?? EmDash} total dmg / min

{(FightOwner?.HasIncompleteMissStatsPlusPets ?? false ? "≤ " : "")}{FightOwner?.WeaponHitDoneChancePlusPets.FormatPercent() ?? EmDashPercent} weapon hit chance
  {FightOwner?.CritDoneChancePlusPets.FormatPercent()} crit chance
  {FightOwner?.GlanceDoneChancePlusPets.FormatPercent()} glance chance

{(FightOwner?.HasIncompleteMissStatsPlusPets ?? false ? "≥ " : "")}{FightOwner?.WeaponHitAttemptsDonePMPlusPets.Format() ?? EmDash} weapon hit attempts / min
{FightOwner?.WeaponHitsDonePMPlusPets.Format() ?? EmDash} weapon hits / min
  {FightOwner?.CritsDonePMPlusPets.Format() ?? EmDash} crits / min
  {FightOwner?.GlancesDonePMPlusPets.Format() ?? EmDash} glances / min
{FightOwner?.NanoHitsDonePMPlusPets.Format() ?? EmDash} nano hits / min
{FightOwner?.IndirectHitsDonePMPlusPets.Format() ?? EmDash} indirect hits / min
{FightOwner?.TotalHitsDonePMPlusPets.Format() ?? EmDash} total hits / min

{FightOwner?.AverageWeaponDamageDonePlusPets.Format() ?? EmDash} weapon dmg / hit
  {FightOwner?.AverageCritDamageDonePlusPets.Format() ?? EmDash} crit dmg / hit
  {FightOwner?.AverageGlanceDamageDonePlusPets.Format() ?? EmDash} glance dmg / hit
{FightOwner?.AverageNanoDamageDonePlusPets.Format()} nano dmg / hit
{FightOwner?.AverageIndirectDamageDonePlusPets.Format() ?? EmDash} indirect dmg / hit"
+ (!(FightOwner?.HasSpecialsDone ?? false) ? null : $@"

{FightOwner?.GetSpecialsDoneInfo()}");
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
                .Where(c => (Settings.Default.IncludeTopLevelNPCRows || !c.IsNPC)
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
