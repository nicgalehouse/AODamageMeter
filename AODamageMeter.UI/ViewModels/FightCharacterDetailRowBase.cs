using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class FightCharacterDetailRowBase : DetailRowBase
    {
        public FightCharacterDetailRowBase(DamageMeterViewModel damageMeterViewModel, FightCharacter fightCharacter, bool showIcon = false)
            : base(damageMeterViewModel, showIcon)
            => FightCharacter = fightCharacter;

        public FightCharacter FightCharacter { get; }
        public string FightCharacterName => FightCharacter.UncoloredName;
        public sealed override string LeftText => FightCharacterName;
        public sealed override string LeftTextToolTip => FightCharacter.Character.GetCharacterTooltip(DisplayIndex);

        public override void Update(int? displayIndex = null)
        {
            if (ShowIcon)
            {
                IconPath = FightCharacter.Profession.GetIconPath();
            }

            Color = FightCharacter.IsFightPet ? FightCharacter.FightPetOwner.Profession.GetColor() : FightCharacter.Profession.GetColor();

            base.Update(displayIndex);
        }
    }
}
