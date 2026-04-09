using System;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.BossModules
{
    public class StatusBarViewModel : ViewModelBase
    {
        private readonly DateTime _startTimeUtc;

        public StatusBarViewModel(string label, double totalSeconds, string barColor, string iconPath)
        {
            _startTimeUtc = DateTime.UtcNow;
            Label = label;
            TotalSeconds = totalSeconds;
            BarColor = (Color)ColorConverter.ConvertFromString(barColor);
            IconPath = iconPath ?? throw new ArgumentNullException(nameof(iconPath));
        }

        public string Label { get; }
        public string Key => Label;
        public double TotalSeconds { get; }
        public Color BarColor { get; }
        public string IconPath { get; }
        public TimeSpan AnimationDuration => TimeSpan.FromSeconds(TotalSeconds);
        public double RemainingSeconds => Math.Max(0, TotalSeconds - (DateTime.UtcNow - _startTimeUtc).TotalSeconds);
        public string TimeRemainingText => $"{(int)Math.Ceiling(RemainingSeconds)}s";
        public bool IsExpired => RemainingSeconds <= 0;

        public void Refresh() => RaisePropertyChanged(nameof(TimeRemainingText));
    }
}
