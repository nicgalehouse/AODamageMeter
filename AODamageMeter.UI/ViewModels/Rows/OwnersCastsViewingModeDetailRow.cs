using AODamageMeter.UI.Helpers;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersCastsViewingModeDetailRow : DetailRowBase
    {
        public OwnersCastsViewingModeDetailRow(FightViewModel fightViewModel, CastInfo castInfo)
            : base(fightViewModel, showIcon: true)
        {
            CastInfo = castInfo;
            IconPath = "/Icons/Cast.png";
            Color = Color.FromRgb(54, 111, 238);
        }

        public CastInfo CastInfo { get; }

        public override string Title => $"{Owner.UncoloredName}'s Casts of {CastInfo.NanoProgram}";
        public override string UnnumberedLeftText => CastInfo.NanoProgram;
        public override string LeftTextToolTip => $"{DisplayIndex}. {CastInfo.NanoProgram}";

        public double? PercentOfMax { get; private set; }

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return CastInfo.GetOwnersCastsTooltip(Title, DisplayIndex);
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            PercentOfMax = CastInfo.PercentOfSourcesMaxCastSuccesses;
            PercentWidth = PercentOfMax ?? 0;
            RightText = $"{CastInfo.CastSuccesses:N0}/{CastInfo.CastAttempts:N0} ({CastInfo.CastSuccessChance.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
