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
            IconPath = _fightCharacter.Profession.GetIconPath();
            ColorHexCode = _fightCharacter.Profession.GetColorHexCode();
            Width = _fightCharacter.PercentOfMaxDamageDone;
            RightText = $"{_fightCharacter.DamageDone.Format()} ({_fightCharacter.ActiveDPM.Format()}, {_fightCharacter.PercentOfTotalDamageDone.FormatPercent()})";
        }
    }
}
