using System;

namespace AODamageMeter.UI.ViewModels
{
    public class AnimatedStatusBarViewModel : StatusBarViewModelBase
    {
        private readonly DateTime _startTimeUtc;
        private readonly double _totalSeconds;

        public AnimatedStatusBarViewModel(string label, double totalSeconds, string barColor, string iconPath)
            : base(label, barColor, iconPath)
        {
            _startTimeUtc = DateTime.UtcNow;
            _totalSeconds = totalSeconds;
        }

        public override double? TotalSeconds => _totalSeconds;
        public TimeSpan AnimationDuration => TimeSpan.FromSeconds(_totalSeconds);
        public double RemainingSeconds => Math.Max(0, _totalSeconds - (DateTime.UtcNow - _startTimeUtc).TotalSeconds);
        public override string RightText => $"{(int)Math.Ceiling(RemainingSeconds)}s";
        public override bool IsExpired => RemainingSeconds <= 0;

        public override void Refresh() => RaisePropertyChanged(nameof(RightText));
    }
}
