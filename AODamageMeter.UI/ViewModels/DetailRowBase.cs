namespace AODamageMeter.UI.ViewModels
{
    public abstract class DetailRowBase : RowBase
    {
        protected DetailRowBase(FightViewModel fightViewModel, bool showIcon = false)
            : base(fightViewModel)
            => ShowIcon = showIcon;

        public bool ShowIcon { get; }
    }
}
