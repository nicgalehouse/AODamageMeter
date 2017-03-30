using AODamageMeter.Helpers;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DpsRowViewModel : RowViewModelBase
    {
        public DpsRowViewModel(FightCharacter character)
        {
            LeftText = character.Name;
            Icon = Professions.GetIcon(character.Profession);
            Color = Professions.GetProfessionColor(character.Profession);
            Width = character.PercentOfDamageDone;
            RightText = NumberFormatter.ThousandsSeparator(character.PercentOfMaxDamage);
        }

        public override void Update(FightCharacter character)
        {
            Width = character.PercentOfMaxDamage;
            RightText = NumberFormatter.ThousandsSeparator(character.DamageDone) + "(" + character.DPSrelativeToPlayerStart + ")";
        }
    }
}
