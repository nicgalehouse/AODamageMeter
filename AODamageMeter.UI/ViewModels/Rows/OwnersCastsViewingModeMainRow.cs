using AODamageMeter.UI.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersCastsViewingModeMainRow : ViewingModeMainRowBase
    {
        public OwnersCastsViewingModeMainRow(Fight fight)
            : base(ViewingMode.OwnersCasts, $"{fight.DamageMeter.Owner}'s Casts", 5, "/Icons/OwnersCasts.png", Color.FromRgb(54, 111, 238), fight)
        { }

        public Character Owner => Fight.DamageMeter.Owner;

        private FightCharacter _fightOwner;
        public FightCharacter FightOwner => _fightOwner;

        private readonly new Dictionary<CastInfo, DetailRowBase> _detailRowMap = new Dictionary<CastInfo, DetailRowBase>();

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
$@"{FightOwner?.CastSuccesses.ToString("N0") ?? EmDash} ({FightOwner?.CastSuccessChance.FormatPercent() ?? EmDash}) succeeded
{FightOwner?.CastCountereds.ToString("N0") ?? EmDash} ({FightOwner?.CastCounteredChance.FormatPercent() ?? EmDash}) countered
{FightOwner?.CastAborteds.ToString("N0") ?? EmDash} ({FightOwner?.CastAbortedChance.FormatPercent() ?? EmDash}) aborted
{FightOwner?.CastAttempts.ToString("N0") ?? EmDash} attempted

{FightOwner?.CastSuccessesPM.Format() ?? EmDash} succeeded / min
{FightOwner?.CastCounteredsPM.Format() ?? EmDash} countered / min
{FightOwner?.CastAbortedsPM.Format() ?? EmDash} aborted / min
{FightOwner?.CastAttemptsPM.Format() ?? EmDash} attempted / min";
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

            RightText = $"{FightOwner.CastSuccesses.ToString("N0")} ({FightOwner.CastSuccessesPM.Format()}, {FightOwner.CastSuccessChance.FormatPercent()})";

            var topCastInfos = FightOwner.CastInfos
                .OrderByDescending(i => i.CastSuccesses)
                .ThenBy(i => i.NanoProgram)
                .Take(6).ToArray();

            foreach (var castInfoDetailRow in _detailRowMap
                .Where(kvp => !topCastInfos.Contains(kvp.Key)).ToArray())
            {
                _detailRowMap.Remove(castInfoDetailRow.Key);
                DetailRows.Remove(castInfoDetailRow.Value);
            }

            int detailRowDisplayIndex = 1;
            foreach (var castInfo in topCastInfos)
            {
                if (!_detailRowMap.TryGetValue(castInfo, out DetailRowBase detailRow))
                {
                    _detailRowMap.Add(castInfo, detailRow = new OwnersCastsViewingModeDetailRow(castInfo));
                    DetailRows.Add(detailRow);
                }
                detailRow.Update(detailRowDisplayIndex++);
            }

            base.Update();
        }
    }
}
