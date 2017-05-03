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
            ColorHexCode = FightCharacter.IsPet ? FightCharacter.FightPetOwner.Profession.GetColorHexCode()
                : FightCharacter.Profession.GetColorHexCode();
            PercentWidth = FightCharacter.PercentOfMaxDamageDonePlusPets;
            RightText = $"{FightCharacter.DamageDone.Format()} ({FightCharacter.ActiveDPM.Format()}, {FightCharacter.PercentOfTotalDamageDone.FormatPercent()})";
        }
    }
}
