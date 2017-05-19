using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class DetailRowViewModelBase : RowViewModelBase
    {
        public DetailRowViewModelBase(FightCharacter fightCharacter)
            : base(fightCharacter)
        { }

        public override void Update(int displayIndex)
        {
            Color = FightCharacter.IsPet ? FightCharacter.FightPetOwner.Profession.GetColor() : FightCharacter.Profession.GetColor();

            base.Update(displayIndex);
        }
    }
}
