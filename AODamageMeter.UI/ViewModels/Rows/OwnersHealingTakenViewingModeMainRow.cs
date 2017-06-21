using AODamageMeter.UI.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingTakenViewingModeMainRow : ViewingModeMainRowBase
    {
        private readonly Dictionary<FightCharacter, DetailRowBase> _detailRowMap = new Dictionary<FightCharacter, DetailRowBase>();

        public OwnersHealingTakenViewingModeMainRow(FightViewModel fightViewModel)
            : base(ViewingMode.OwnersHealingTaken, $"{fightViewModel.Owner}'s Healing Taken", 4, "/Icons/OwnersHealingTaken.png", Color.FromRgb(184, 100, 57), fightViewModel)
        { }

        public override string LeftTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    var counts = Fight
                        .GetFightCharacterCounts(includeNullOwnersHealingTakens: false);

                    return counts.GetFightCharacterCountsTooltip();
                }
            }
        }

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    int fightCharacterCount = Fight
                        .GetFightCharacterCount(includeNullOwnersHealingTakens: false);

                    return
$@"{fightCharacterCount} {(fightCharacterCount == 1 ? "character" : "characters")}

≥ {FightOwner?.PotentialHealingTaken.Format() ?? EmDash} potential healing
{FightOwner?.RealizedHealingTaken.Format() ?? EmDash} realized healing
≥ {FightOwner?.OverhealingTaken.Format() ?? EmDash} overhealing
≥ {FightOwner?.NanoHealingTaken.Format() ?? EmDash} nano healing

≥ {FightOwner?.PotentialHealingTakenPM.Format() ?? EmDash} potential healing / min
{FightOwner?.RealizedHealingTakenPM.Format() ?? EmDash} realized healing / min
≥ {FightOwner?.OverhealingTakenPM.Format() ?? EmDash} overhealing / min
≥ {FightOwner?.NanoHealingTakenPM.Format() ?? EmDash} nano healing / min

≥ {FightOwner?.PercentOfOverhealingTaken.FormatPercent() ?? EmDash} overhealing";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            if (FightOwner == null)
            {
                RightText = $"{EmDash} ({EmDash})";
            }
            else
            {
                RightText = $"{FightOwner.PotentialHealingTaken.Format()} ({FightOwner.PotentialHealingTakenPM.Format()})";

                var topHealingTakenInfos = FightOwner.HealingTakenInfos
                    .Where(i => !i.Source.IsFightPet)
                    .OrderByDescending(i => i.PotentialHealingPlusPets)
                    .ThenBy(i => i.Source.UncoloredName)
                    .Take(6).ToArray();

                foreach (var fightCharacterDetailRow in _detailRowMap
                    .Where(kvp => !topHealingTakenInfos.Select(i => i.Source).Contains(kvp.Key)).ToArray())
                {
                    _detailRowMap.Remove(fightCharacterDetailRow.Key);
                    DetailRows.Remove(fightCharacterDetailRow.Value);
                }

                int detailRowDisplayIndex = 1;
                foreach (var healingTakenInfo in topHealingTakenInfos)
                {
                    if (!_detailRowMap.TryGetValue(healingTakenInfo.Source, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(healingTakenInfo.Source, detailRow = new OwnersHealingTakenViewingModeDetailRow(FightViewModel, healingTakenInfo));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            base.Update();
        }

        public override bool TryCopyAndScriptProgressedRowsInfo()
        {
            var body = new StringBuilder();
            foreach (var ownersHealingTakenRow in FightViewModel.GetUpdatedOwnersHealingTakenRows()
                .OrderBy(r => r.DisplayIndex))
            {
                body.AppendLine(ownersHealingTakenRow.RowScriptText);
            }

            CopyAndScript(body.ToString());

            return true;
        }
    }
}
