using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageDoneDetailRowViewModel : RowViewModelBase
    {
        public DamageDoneDetailRowViewModel(FightCharacter fightCharacter)
            : base(fightCharacter)
        { }

        public override void Update(int? displayIndex = null)
        {
            Color = FightCharacter.IsPet ? FightCharacter.FightPetOwner.Profession.GetColor()
                : FightCharacter.Profession.GetColor();
            PercentWidth = FightCharacter.PercentOfMaxDamageDonePlusPets;
            RightText = $"{FightCharacter.DamageDone.Format()} ({FightCharacter.ActiveDPM.Format()}, {FightCharacter.PercentOfOwnOrOwnersDamageDonePlusPets.FormatPercent()}, {FightCharacter.PercentOfTotalDamageDone.FormatPercent()})";
        }
    }
}
