using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageDoneMainRowViewModel : MainRowViewModelBase
    {
        public DamageDoneMainRowViewModel(FightCharacter fightCharacter)
            : base(fightCharacter)
        { }

        public override string RightTextToolTip =>
$@"{DisplayIndex}. {FightCharacterName}

{FightCharacter.HitChancePlusPets.FormatPercent()} hit chance
{FightCharacter.CritChancePlusPets.FormatPercent()} crit chance
{FightCharacter.GlanceChancePlusPets.FormatPercent()} glance chance
{FightCharacter.MissChancePlusPets.FormatPercent()} miss chance

{FightCharacter.ActiveHPMPlusPets.Format()} hits / min
{FightCharacter.ActiveCPMPlusPets.Format()} crits / min
{FightCharacter.ActiveGPMPlusPets.Format()} glances / min
{FightCharacter.ActiveNHPMPlusPets.Format()} nano hits / min
{FightCharacter.ActiveIHPMPlusPets.Format()} indirect hits / min
{FightCharacter.ActiveTHPMPlusPets.Format()} total hits / min
{FightCharacter.ActiveMPMPlusPets.Format()} misses / min

{FightCharacter.ActiveHDPMPlusPets.Format()} ({FightCharacter.PercentOfDamageDonePlusPetsViaHits.FormatPercent()}) hit damage / min
{FightCharacter.ActiveNHDPMPlusPets.Format()} ({FightCharacter.PercentOfDamageDonePlusPetsViaNanoHits.FormatPercent()}) nano hit damage / min
{FightCharacter.ActiveIHDPMPlusPets.Format()} ({FightCharacter.PercentOfDamageDonePlusPetsViaIndirectHits.FormatPercent()}) indirect hit damage / min
{FightCharacter.ActiveDPMPlusPets.Format()} total damage / min";

        public override void Update(int displayIndex)
        {
            if (FightCharacter.FightPets.Count == 0)
            {
                PercentWidth = FightCharacter.PercentOfMaxDamageDonePlusPets;
                double percentDone = Settings.Default.ShowPercentOfTotalDamageDone ? FightCharacter.PercentOfTotalDamageDone : PercentWidth;
                RightText = $"{FightCharacter.DamageDone.Format()} ({FightCharacter.ActiveDPM.Format()}, {percentDone.FormatPercent()})";
            }
            else
            {
                PercentWidth = FightCharacter.PercentPlusPetsOfMaxDamageDonePlusPets;
                double percentDone = Settings.Default.ShowPercentOfTotalDamageDone ? FightCharacter.PercentPlusPetsOfTotalDamageDone : PercentWidth;
                RightText = $"{FightCharacter.DamageDonePlusPets.Format()} ({FightCharacter.ActiveDPMPlusPets.Format()}, {percentDone.FormatPercent()})";

                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { FightCharacter }.Concat(FightCharacter.FightPets)
                        .OrderByDescending(c => c.DamageDonePlusPets)
                        .ThenBy(c => c.Name))
                {
                    if (!_detailRowViewModelsMap.TryGetValue(fightCharacter, out RowViewModelBase detailRowViewModel))
                    {
                        _detailRowViewModelsMap.Add(fightCharacter, detailRowViewModel = new DamageDoneDetailRowViewModel(fightCharacter));
                        DetailRowViewModels.Add(detailRowViewModel);
                    }
                    detailRowViewModel.Update(detailRowDisplayIndex++);
                }
            }

            base.Update(displayIndex);
        }
    }
}
