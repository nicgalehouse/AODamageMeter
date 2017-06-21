using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class FightMainRow : MainRowBase
    {
        public FightMainRow(FightViewModel fightViewModel, int displayIndex)
            : base(fightViewModel)
        {
            DisplayIndex = displayIndex;
            PercentWidth = 1;
        }

        public override string Title
            => Fight.HasEnded ? $"Fight ({Fight.StartTime:t} - {Fight.EndTime:t}"
            : Fight.HasStarted ? $"Fight ({Fight.StartTime:t} - {DateTime.Now:t})"
            : "Fight (unstarted)";

        public override string UnnumberedLeftText
            => Fight.HasEnded ? $"{Fight.StartTime:t}{EnDash}{Fight.EndTime:t}"
            : Fight.HasStarted ? $"{Fight.StartTime:t}{EnDash}{DateTime.Now:t}{(Fight.IsPaused ? " (paused)" : "")}"
            : $"Unstarted{(Fight.IsPaused ? " (paused)" : "")}";

        public override string LeftTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    var counts = Fight.GetFightCharacterCounts(includeNPCs: Settings.Default.ShowTopLevelNPCRows);

                    return counts.GetFightCharacterCountsTooltip();
                }
            }
        }

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{(Fight.Duration ?? TimeSpan.Zero).WithoutMilliseconds():g} duration

{Owner.UncoloredName}'s Damage Done
";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            RightText =  $"{(Fight.Duration ?? TimeSpan.Zero).WithoutMilliseconds():g}, {FightOwner?.TotalDamageDonePMPlusPets.Format() ?? EmDash}";

            IconPath = Owner.Profession.GetIconPath();
            Color = Owner.Profession.GetColor();
            base.Update(displayIndex);
        }
    }
}
