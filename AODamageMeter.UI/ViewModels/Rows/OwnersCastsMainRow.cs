using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersCastsMainRow : MainRowBase
    {
        public OwnersCastsMainRow(DamageMeterViewModel damageMeterViewModel, CastInfo castInfo)
            : base(damageMeterViewModel)
        {
            CastInfo = castInfo;
            IconPath = "/Icons/Cast.png";
            Color = Color.FromRgb(54, 111, 238);
        }

        public override string Title => $"{Source.UncoloredName}'s Casts of {CastInfo.NanoProgram}";

        public CastInfo CastInfo { get; }
        public FightCharacter Source => CastInfo.Source;

        public override string UnnumberedLeftText
            => CastInfo.NanoProgram;

        public override string LeftTextToolTip
            => $"{DisplayIndex}. {CastInfo.NanoProgram}";

        public override string RightTextToolTip
        {
            get
            {
                lock (CurrentDamageMeter)
                {
                    return
$@"{CastInfo.CastSuccesses.ToString("N0")} ({CastInfo.CastSuccessChance.FormatPercent()}) succeeded
{CastInfo.CastCountereds.ToString("N0")} ({CastInfo.CastCounteredChance.FormatPercent()}) countered
{CastInfo.CastAborteds.ToString("N0")} ({CastInfo.CastAbortedChance.FormatPercent()}) aborted
{CastInfo.CastAttempts.ToString("N0")} attempted

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
            RightText = $"{CastInfo.CastSuccesses.ToString("N0")} ({CastInfo.CastSuccessesPM.Format()}, {CastInfo.CastSuccessChance.FormatPercent()}, {percentDone.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
