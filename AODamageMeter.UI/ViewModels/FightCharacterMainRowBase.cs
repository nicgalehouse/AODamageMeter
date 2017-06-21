using AODamageMeter.UI.Helpers;
using System.Collections.Generic;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class FightCharacterMainRowBase : MainRowBase
    {
        protected readonly Dictionary<FightCharacter, DetailRowBase> _detailRowMap = new Dictionary<FightCharacter, DetailRowBase>();

        public FightCharacterMainRowBase(FightViewModel fightViewModel, FightCharacter fightCharacter)
            : base(fightViewModel)
            => FightCharacter = fightCharacter;

        public FightCharacter FightCharacter { get; }
        public string FightCharacterName => FightCharacter.UncoloredName;

        public sealed override string UnnumberedLeftText => FightCharacterName;
        public sealed override string LeftTextToolTip => FightCharacter.GetCharacterTooltip(DisplayIndex);

        public override void Update(int? displayIndex = null)
        {
            IconPath = FightCharacter.Profession.GetIconPath();
            Color = FightCharacter.Profession.GetColor();

            base.Update(displayIndex);
        }
    }
}
