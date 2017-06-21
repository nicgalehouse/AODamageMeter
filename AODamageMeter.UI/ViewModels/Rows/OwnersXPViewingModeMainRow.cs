using AODamageMeter.UI.Helpers;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersXPViewingModeMainRow : ViewingModeMainRowBase
    {
        public OwnersXPViewingModeMainRow(FightViewModel fightViewModel)
            : base(ViewingMode.OwnersXP, $"{fightViewModel.Owner}'s XP", 6, "/Icons/XP.png", Color.FromRgb(67, 98, 110), fightViewModel)
        { }

        public override string LeftTextToolTip => Owner.GetCharacterTooltip();

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{FightOwner?.NormalXPGained.Format() ?? EmDash} XP
{FightOwner?.ShadowXPGained.Format() ?? EmDash} SK
{FightOwner?.ResearchXPGained.Format() ?? EmDash} research XP
{FightOwner?.EffectiveXPGained.Format() ?? EmDash} effective XP
{FightOwner?.AlienXPGained.Format() ?? EmDash} alien XP

{FightOwner?.NormalXPGainedPM.Format() ?? EmDash} XP / min
{FightOwner?.ShadowXPGainedPM.Format() ?? EmDash} SK / min
{FightOwner?.ResearchXPGainedPM.Format() ?? EmDash} research XP / min
{FightOwner?.EffectiveXPGainedPM.Format() ?? EmDash} effective XP / min
{FightOwner?.AlienXPGainedPM.Format() ?? EmDash} alien XP / min";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            if (FightOwner == null)
            {
                RightText = $"{EmDash} ({EmDash})";
            }
            else
            {
                RightText = FightOwner.AlienXPGained == 0
                    ? $"{FightOwner.EffectiveXPGained.Format()} ({FightOwner.EffectiveXPGainedPM.Format()})"
                    : $"{FightOwner.EffectiveXPGained.Format()}, {FightOwner.AlienXPGained.Format()} ({FightOwner.EffectiveXPGainedPM.Format()}, {FightOwner.AlienXPGainedPM.Format()})";
            }

            base.Update();
        }
    }
}
