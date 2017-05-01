using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageDoneRowViewModel : RowViewModelBase
    {
        public DamageDoneRowViewModel(FightCharacter fightCharacter)
            : base(fightCharacter)
        { }

        public override void Update()
        {
            base.Update();

            PercentWidth = FightCharacter.PercentOfMaxDamageDone;
            RightText = $"{FightCharacter.DamageDone.Format()} ({FightCharacter.ActiveDPM.Format()}, {FightCharacter.PercentOfTotalDamageDone.FormatPercent()})";
        }
    }
}
