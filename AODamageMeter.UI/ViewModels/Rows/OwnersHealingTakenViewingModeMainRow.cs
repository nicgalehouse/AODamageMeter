using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingTakenViewingModeMainRow : ViewingModeMainRowBase
    {
        private readonly Dictionary<FightCharacter, DetailRowBase> _detailRowMap = new Dictionary<FightCharacter, DetailRowBase>();

        public OwnersHealingTakenViewingModeMainRow(FightViewModel fightViewModel)
            : base(ViewingMode.OwnersHealingTaken, $"{fightViewModel.Owner}'s Healing Taken", 4, "/Icons/OwnersHealingTaken.png", Color.FromRgb(184, 100, 57), fightViewModel)
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

≥ {FightOwner?.PotentialHealingTaken.Format() ?? EmDash} potential healing
{FightOwner?.RealizedHealingTaken.Format() ?? EmDash} realized healing
≥ {FightOwner?.OverhealingTaken.Format() ?? EmDash} overhealing
≥ {FightOwner?.NanoHealingTaken.Format() ?? EmDash} nano healing

≥ {FightOwner?.PotentialHealingTakenPM.Format() ?? EmDash} potential healing / min
{FightOwner?.RealizedHealingTakenPM.Format() ?? EmDash} realized healing / min
≥ {FightOwner?.OverhealingTakenPM.Format() ?? EmDash} overhealing / min
≥ {FightOwner?.NanoHealingTakenPM.Format() ?? EmDash} nano healing / min

≥ {FightOwner?.PercentOfOverhealingTaken.FormatPercent() ?? EmDashPercent} overhealing";
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
                    .Take(Settings.Default.MaxNumberOfDetailRows).ToArray();

                foreach (var noLongerTopHealingTakenSource in _detailRowMap.Keys
                    .Except(topHealingTakenInfos.Select(i => i.Source)).ToArray())
                {
                    DetailRows.Remove(_detailRowMap[noLongerTopHealingTakenSource]);
                    _detailRowMap.Remove(noLongerTopHealingTakenSource);
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
            => CopyAndScriptProgressedRowsInfo(FightViewModel.GetUpdatedOwnersHealingTakenRows());
    }
}
