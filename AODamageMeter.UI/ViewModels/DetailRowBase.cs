namespace AODamageMeter.UI.ViewModels
{
    public abstract class DetailRowBase : RowBase
    {
        protected DetailRowBase(DamageMeterViewModel damageMeterViewModel, bool showIcon = false)
            : base(damageMeterViewModel)
            => ShowIcon = showIcon;

        public bool ShowIcon { get; }
    }
}
