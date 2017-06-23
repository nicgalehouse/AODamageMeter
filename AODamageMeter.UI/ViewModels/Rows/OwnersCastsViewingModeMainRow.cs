using AODamageMeter.UI.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersCastsViewingModeMainRow : ViewingModeMainRowBase
    {
        private readonly Dictionary<CastInfo, DetailRowBase> _detailRowMap = new Dictionary<CastInfo, DetailRowBase>();

        public OwnersCastsViewingModeMainRow(FightViewModel fightViewModel)
            : base(ViewingMode.OwnersCasts, $"{fightViewModel.Owner}'s Casts", 5, "/Icons/OwnersCasts.png", Color.FromRgb(54, 111, 238), fightViewModel)
        { }

        public override string LeftTextToolTip => Owner.GetCharacterTooltip();

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{Title}

{FightOwner?.CastSuccesses.ToString("N0") ?? EmDash} ({FightOwner?.CastSuccessChance.FormatPercent() ?? EmDashPercent}) succeeded
{FightOwner?.CastCountereds.ToString("N0") ?? EmDash} ({FightOwner?.CastCounteredChance.FormatPercent() ?? EmDashPercent}) countered
{FightOwner?.CastAborteds.ToString("N0") ?? EmDash} ({FightOwner?.CastAbortedChance.FormatPercent() ?? EmDashPercent}) aborted
{FightOwner?.CastAttempts.ToString("N0") ?? EmDash} attempted

{FightOwner?.CastSuccessesPM.Format() ?? EmDash} succeeded / min
{FightOwner?.CastCounteredsPM.Format() ?? EmDash} countered / min
{FightOwner?.CastAbortedsPM.Format() ?? EmDash} aborted / min
{FightOwner?.CastAttemptsPM.Format() ?? EmDash} attempted / min";
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
                RightText = $"{FightOwner.CastSuccesses:N0} ({FightOwner.CastSuccessesPM.Format()}, {FightOwner.CastSuccessChance.FormatPercent()})";

                var topCastInfos = FightOwner.CastInfos
                    .OrderByDescending(i => i.CastSuccesses)
                    .ThenBy(i => i.NanoProgram)
                    .Take(6).ToArray();

                foreach (var castInfoDetailRow in _detailRowMap
                    .Where(kvp => !topCastInfos.Contains(kvp.Key)).ToArray())
                {
                    _detailRowMap.Remove(castInfoDetailRow.Key);
                    DetailRows.Remove(castInfoDetailRow.Value);
                }

                int detailRowDisplayIndex = 1;
                foreach (var castInfo in topCastInfos)
                {
                    if (!_detailRowMap.TryGetValue(castInfo, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(castInfo, detailRow = new OwnersCastsViewingModeDetailRow(FightViewModel, castInfo));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            base.Update();
        }

        public override bool TryCopyAndScriptProgressedRowsInfo()
            => CopyAndScriptProgressedRowsInfo(FightViewModel.GetUpdatedOwnersCastsRows());
    }
}
