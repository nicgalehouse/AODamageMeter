using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageDoneMainRowViewModel : RowViewModelBase
    {
        public DamageDoneMainRowViewModel(FightCharacter fightCharacter)
            : base(fightCharacter)
        { }

        public override void Update(int? displayIndex = null)
        {
            base.Update(displayIndex);

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

                foreach (var fightCharacter in new[] { FightCharacter }.Concat(FightCharacter.FightPets))
                {
                    if (!_detailRowViewModelsMap.TryGetValue(fightCharacter, out RowViewModelBase detailRowViewModel))
                    {
                        _detailRowViewModelsMap.Add(fightCharacter, detailRowViewModel = new DamageDoneDetailRowViewModel(fightCharacter));
                        DetailRowViewModels.Add(detailRowViewModel);
                    }
                    detailRowViewModel.Update();
                }
            }
        }
    }
}
