using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
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

        public double? PercentOfTotal { get; protected set; }
        public double? PercentOfMax { get; protected set; }
        public double? DisplayedPercent => Settings.Default.ShowPercentOfTotal ? PercentOfTotal : PercentOfMax;

        public override void Update(int? displayIndex = null)
        {
            PercentWidth = PercentOfMax ?? 0;

            IconPath = FightCharacter.Profession.GetIconPath();
            Color = FightCharacter.Profession.GetColor();

            base.Update(displayIndex);
        }
    }
}
