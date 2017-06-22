using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class FightCharacterDetailRowBase : DetailRowBase
    {
        public FightCharacterDetailRowBase(FightViewModel fightViewModel, FightCharacter fightCharacter, bool showIcon = false)
            : base(fightViewModel, showIcon)
            => FightCharacter = fightCharacter;

        public FightCharacter FightCharacter { get; }
        public string FightCharacterName => FightCharacter.UncoloredName;

        public sealed override string UnnumberedLeftText => FightCharacterName;
        public sealed override string LeftTextToolTip => FightCharacter.GetCharacterTooltip(DisplayIndex);

        public double? PercentOfTotal { get; protected set; }
        public double? PercentOfMax { get; protected set; }
        public double? DisplayedPercent => Settings.Default.ShowPercentOfTotal ? PercentOfTotal : PercentOfMax;

        public override void Update(int? displayIndex = null)
        {
            PercentWidth = PercentOfMax ?? 0;

            if (ShowIcon)
            {
                IconPath = FightCharacter.Profession.GetIconPath();
            }
            Color = FightCharacter.IsFightPet ? FightCharacter.FightPetOwner.Profession.GetColor() : FightCharacter.Profession.GetColor();

            base.Update(displayIndex);
        }
    }
}
