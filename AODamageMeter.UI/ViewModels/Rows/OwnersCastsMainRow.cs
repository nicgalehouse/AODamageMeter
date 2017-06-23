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

        public double? PercentOfTotal { get; private set; }
        public double? PercentOfMax { get; private set; }
        public double? DisplayedPercent => Settings.Default.ShowPercentOfTotal ? PercentOfTotal : PercentOfMax;

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return CastInfo.GetOwnersCastsTooltip(Title, DisplayIndex, PercentOfTotal, PercentOfMax);
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            PercentOfTotal = CastInfo.PercentOfSourcesCastSuccesses;
            PercentOfMax = CastInfo.PercentOfSourcesMaxCastSuccesses;
            PercentWidth = PercentOfMax ?? 0;
            RightText = $"{CastInfo.CastSuccesses:N0} ({CastInfo.CastSuccessesPM.Format()}, {CastInfo.CastSuccessChance.FormatPercent()}, {DisplayedPercent.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
