using AODamageMeter.UI.Helpers;
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
                RightText = $"{FightCharacter.DamageDone.Format()} ({FightCharacter.ActiveDPM.Format()}, {FightCharacter.PercentOfTotalDamageDone.FormatPercent()})";
            }
            else
            {
                PercentWidth = FightCharacter.PercentPlusPetsOfMaxDamageDonePlusPets;
                RightText = $"{FightCharacter.DamageDonePlusPets.Format()} ({FightCharacter.ActiveDPMPlusPets.Format()}, {FightCharacter.PercentPlusPetsOfTotalDamageDone.FormatPercent()})";
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
