using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersCastsMainRow : MainRowBase
    {
        public OwnersCastsMainRow(FightViewModel fightViewModel, CastInfo castInfo)
            : base(fightViewModel)
        {
            CastInfo = castInfo;
            IconPath = "/Icons/Cast.png";
            Color = Color.FromRgb(54, 111, 238);
        }

        public CastInfo CastInfo { get; }

        public override string Title => $"{Owner.UncoloredName}'s Casts of {CastInfo.NanoProgram}";
        public override string UnnumberedLeftText => CastInfo.NanoProgram;
        public override string LeftTextToolTip => $"{DisplayIndex}. {CastInfo.NanoProgram}";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{CastInfo.CastSuccesses:N0} ({CastInfo.CastSuccessChance.FormatPercent()}) succeeded
{CastInfo.CastCountereds:N0} ({CastInfo.CastCounteredChance.FormatPercent()}) countered
{CastInfo.CastAborteds:N0} ({CastInfo.CastAbortedChance.FormatPercent()}) aborted
{CastInfo.CastAttempts:N0} attempted

{CastInfo.CastSuccessesPM.Format()} succeeded / min
{CastInfo.CastCounteredsPM.Format()} countered / min
{CastInfo.CastAbortedsPM.Format()} aborted / min
{CastInfo.CastAttemptsPM.Format()} attempted / min";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            PercentWidth = CastInfo.PercentOfSourcesMaxCastSuccesses ?? 0;
            double? percentDone = Settings.Default.ShowPercentOfTotal
                ? CastInfo.PercentOfSourcesCastSuccesses : CastInfo.PercentOfSourcesMaxCastSuccesses;
            RightText = $"{CastInfo.CastSuccesses:N0} ({CastInfo.CastSuccessesPM.Format()}, {CastInfo.CastSuccessChance.FormatPercent()}, {percentDone.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
