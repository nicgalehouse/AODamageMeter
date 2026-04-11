using System;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class StatusBarViewModelBase : ViewModelBase
    {
        protected StatusBarViewModelBase(string label, string barColor, string iconPath)
        {
            Label = label;
            BarColor = (Color)ColorConverter.ConvertFromString(barColor);
            IconPath = iconPath ?? throw new ArgumentNullException(nameof(iconPath));
        }

        public string Label { get; }
        public string Key => Label;
        public Color BarColor { get; }
        public string IconPath { get; }

        public abstract double? TotalSeconds { get; }
        public abstract string RightText { get; }
        public abstract bool IsExpired { get; }
        public abstract void Refresh();
    }
}
