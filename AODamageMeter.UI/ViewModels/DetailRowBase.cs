using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class DetailRowBase : RowBase
    {
        protected DetailRowBase(FightCharacter fightCharacter = null, bool showIcon = false)
            : base(fightCharacter)
            => ShowIcon = showIcon;

        public bool ShowIcon { get; }

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
