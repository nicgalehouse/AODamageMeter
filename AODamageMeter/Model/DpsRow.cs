using System;
using AODamageMeter.Helpers;

namespace AODamageMeter.Model
{
    public class DpsRow : Row
    {
        public DpsRow(Character character)
        {
            LeftText = character.Name;
            Icon = Professions.GetIcon(character.Profession);
            Color = Professions.GetProfessionColor(character.Profession);
            Width = character.PercentOfDamageDone;
            RightText = NumberFormatter.ThousandsSeparator(character.PercentOfMaxDamage);
        }

        public override void Update(Character character)
        {
            Width = character.PercentOfMaxDamage;
            RightText = NumberFormatter.ThousandsSeparator(character.DamageDone) + "(" + character.DPSrelativeToPlayerStart + ")";
        }
    }
}
