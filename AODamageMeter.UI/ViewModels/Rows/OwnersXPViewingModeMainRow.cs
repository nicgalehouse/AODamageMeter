using AODamageMeter.UI.Helpers;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersXPViewingModeMainRow : ViewingModeMainRowBase
    {
        public OwnersXPViewingModeMainRow(Fight fight)
            : base(ViewingMode.OwnersXP, $"{fight.DamageMeter.Owner}'s XP", 6, "/Icons/XP.png", Color.FromRgb(67, 98, 110), fight)
        { }

        public Character Owner => Fight.DamageMeter.Owner;

        private FightCharacter _fightOwner;
        public FightCharacter FightOwner => _fightOwner;

        public override string LeftTextToolTip
            => $"{Owner.UncoloredName}"
+ (!Owner.HasPlayerInfo ? null : $@"
{Owner.Level}/{Owner.AlienLevel} {Owner.Faction} {Owner.Profession}
{Owner.Breed} {Owner.Gender}")
+ (!Owner.HasOrganizationInfo ? null : $@"
{Owner.Organization} ({Owner.OrganizationRank})");

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight.DamageMeter)
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
            if (FightOwner == null && !Fight.TryGetFightOwnerCharacter(out _fightOwner))
            {
                RightText = $"{EmDash} ({EmDash})";
                return;
            }

            RightText = FightOwner.AlienXPGained == 0
                ? $"{FightOwner.EffectiveXPGained.Format()} ({FightOwner.EffectiveXPGainedPM.Format()})"
                : $"{FightOwner.EffectiveXPGained.Format()}, {FightOwner.AlienXPGained.Format()} ({FightOwner.EffectiveXPGainedPM.Format()}, {FightOwner.AlienXPGainedPM.Format()})";

            base.Update();
        }
    }
}
