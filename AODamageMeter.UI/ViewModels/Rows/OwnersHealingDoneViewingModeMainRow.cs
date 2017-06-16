using AODamageMeter.UI.Helpers;
using System.Linq;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingDoneViewingModeMainRow : ViewingModeMainRowBase
    {
        public OwnersHealingDoneViewingModeMainRow(Fight fight)
            : base(ViewingMode.OwnersHealingDone, $"{fight.DamageMeter.Owner}'s Healing Done", 3, "/Icons/OwnersHealingDone.png", Color.FromRgb(197, 135, 25), fight)
        { }

        private FightCharacter _fightOwner;
        public FightCharacter FightOwner => _fightOwner;

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight.DamageMeter)
                {
                    int fightCharacterCount = Fight
                        .GetFightCharacterCount(includeNullOwnersHealingDones: false);

                    return
$@"{fightCharacterCount} {(fightCharacterCount == 1 ? "character" : "characters")}

≥ {FightOwner?.PotentialHealingDonePlusPets.Format() ?? EmDash} potential healing
≥ {FightOwner?.RealizedHealingDonePlusPets.Format() ?? EmDash} realized healing
≥ {FightOwner?.OverhealingDonePlusPets.Format() ?? EmDash} overhealing
≥ {FightOwner?.NanoHealingDonePlusPets.Format() ?? EmDash} nano healing

≥ {FightOwner?.PotentialHealingDonePMPlusPets.Format() ?? EmDash} potential healing / min
≥ {FightOwner?.RealizedHealingDonePMPlusPets.Format() ?? EmDash} realized healing / min
≥ {FightOwner?.OverhealingDonePMPlusPets.Format() ?? EmDash} overhealing / min
≥ {FightOwner?.NanoHealingDonePMPlusPets.Format() ?? EmDash} nano healing / min

≥ {FightOwner?.PercentOfOverhealingDonePlusPets.FormatPercent() ?? EmDash} overhealing";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            if (FightOwner == null && !Fight.TryGetFightOwnerCharacter(out _fightOwner))
            {
                RightText = $"{EmDash} ({EmDash})";
                return;
            }

            RightText = $"{FightOwner.PotentialHealingDonePlusPets.Format()} ({FightOwner.PotentialHealingDonePMPlusPets.Format()})";

            var topHealingDoneInfos = FightOwner.HealingDoneInfos
                .OrderByDescending(i => i.PotentialHealingPlusPets)
                .ThenBy(i => i.Target.UncoloredName)
                .Take(6).ToArray();

            foreach (var fightCharacterDetailRow in _detailRowMap
                .Where(kvp => !topHealingDoneInfos.Select(i => i.Target).Contains(kvp.Key)).ToArray())
            {
                _detailRowMap.Remove(fightCharacterDetailRow.Key);
                DetailRows.Remove(fightCharacterDetailRow.Value);
            }

            int detailRowDisplayIndex = 1;
            foreach (var healingDoneInfo in topHealingDoneInfos)
            {
                if (!_detailRowMap.TryGetValue(healingDoneInfo.Target, out DetailRowBase detailRow))
                {
                    _detailRowMap.Add(healingDoneInfo.Target, detailRow = new OwnersHealingDoneViewingModeDetailRow(healingDoneInfo));
                    DetailRows.Add(detailRow);
                }
                detailRow.Update(detailRowDisplayIndex++);
            }

            base.Update();
        }
    }
}
