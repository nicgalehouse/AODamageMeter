using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageDoneDetailRowViewModel : DetailRowViewModelBase
    {
        public DamageDoneDetailRowViewModel(FightCharacter fightCharacter)
            : base(fightCharacter)
        { }

        public override string RightTextToolTip =>
$@"{DisplayIndex}. {FightCharacterName}

{FightCharacter.HitChance.FormatPercent()} hit chance
{FightCharacter.CritChance.FormatPercent()} crit chance
{FightCharacter.GlanceChance.FormatPercent()} glance chance
{FightCharacter.MissChance.FormatPercent()} miss chance

{FightCharacter.ActiveHPM.Format()} hits/min
{FightCharacter.ActiveCPM.Format()} crits/min
{FightCharacter.ActiveGPM.Format()} glances/min
{FightCharacter.ActiveNHPM.Format()} nano hits/min
{FightCharacter.ActiveIHPM.Format()} indirect hits/min
{FightCharacter.ActiveTHPM.Format()} total hits/min
{FightCharacter.ActiveMPM.Format()} misses/min

{FightCharacter.ActiveHDPM.Format()} ({FightCharacter.PercentOfDamageDoneViaHits.FormatPercent()}) hit dmg/min
{FightCharacter.ActiveNHDPM.Format()} ({FightCharacter.PercentOfDamageDoneViaNanoHits.FormatPercent()}) nano dmg/min
{FightCharacter.ActiveIHDPM.Format()} ({FightCharacter.PercentOfDamageDoneViaIndirectHits.FormatPercent()}) indirect dmg/min
{FightCharacter.ActiveDPM.Format()} total dmg/min";

        public override void Update(int displayIndex)
        {
            PercentWidth = FightCharacter.PercentOfMaxDamageDonePlusPets;
            double percentDone = Settings.Default.ShowPercentOfTotalDamageDone ? FightCharacter.PercentOfTotalDamageDone : PercentWidth;
            RightText = $"{FightCharacter.DamageDone.Format()} ({FightCharacter.ActiveDPM.Format()}, {FightCharacter.PercentOfOwnOrOwnersDamageDonePlusPets.FormatPercent()}, {percentDone.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
