namespace AODamageMeter.UI.ViewModels
{
    public class FixedStatusBarViewModel : StatusBarViewModelBase
    {
        public FixedStatusBarViewModel(string label, string value, string barColor, string iconPath)
            : base(label, barColor, iconPath)
            => Value = value;

        public string Value { get; set; }
        public double Progress { get; set; } = 1.0;

        public override double? TotalSeconds => null;
        public override string RightText => Value;
        public override bool IsExpired => false;

        public override void Refresh() => RaisePropertyChanged(nameof(RightText));
    }
}
