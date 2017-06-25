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
$@"{Title}

{FightOwner?.NormalXPGained.FormatSigned() ?? EmDash} XP
{FightOwner?.ShadowXPGained.FormatSigned() ?? EmDash} SK
{FightOwner?.ResearchXPGained.FormatSigned() ?? EmDash} research XP
{FightOwner?.EffectiveXPGained.FormatSigned() ?? EmDash} effective XP
{FightOwner?.AlienXPGained.FormatSigned() ?? EmDash} alien XP

{FightOwner?.NormalXPGainedPM.FormatSigned() ?? EmDash} XP / min
{FightOwner?.ShadowXPGainedPM.FormatSigned() ?? EmDash} SK / min
{FightOwner?.ResearchXPGainedPM.FormatSigned() ?? EmDash} research XP / min
{FightOwner?.EffectiveXPGainedPM.FormatSigned() ?? EmDash} effective XP / min
{FightOwner?.AlienXPGainedPM.FormatSigned() ?? EmDash} alien XP / min";
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
                    ? $"{FightOwner.EffectiveXPGained.FormatSigned()} ({FightOwner.EffectiveXPGainedPM.FormatSigned()})"
                    : $"{FightOwner.EffectiveXPGained.FormatSigned()}, {FightOwner.AlienXPGained.FormatSigned()} ({FightOwner.EffectiveXPGainedPM.FormatSigned()}, {FightOwner.AlienXPGainedPM.FormatSigned()})";
            }

            base.Update();
        }
    }
}
