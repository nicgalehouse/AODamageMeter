using AODamageMeter.UI.ViewModels.BossModules;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace AODamageMeter.UI.Behaviors
{
    public static class StatusBarAnimation
    {
        public static readonly DependencyProperty StatusBarViewModelProperty =
            DependencyProperty.RegisterAttached(
                "StatusBarViewModel",
                typeof(StatusBarViewModel),
                typeof(StatusBarAnimation),
                new PropertyMetadata(null, OnStatusBarViewModelChanged));

        public static StatusBarViewModel GetStatusBarViewModel(ProgressBar progressBar)
            => (StatusBarViewModel)progressBar.GetValue(StatusBarViewModelProperty);

        public static void SetStatusBarViewModel(ProgressBar progressBar, StatusBarViewModel value)
            => progressBar.SetValue(StatusBarViewModelProperty, value);

        private static void OnStatusBarViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ProgressBar progressBar) || !(e.NewValue is StatusBarViewModel statusBarViewModel))
                return;

            double remainingSeconds = statusBarViewModel.RemainingSeconds;

            var animation = new DoubleAnimation
            {
                From = remainingSeconds,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(remainingSeconds)),
                FillBehavior = FillBehavior.HoldEnd,
            };

            progressBar.BeginAnimation(RangeBase.ValueProperty, animation);
        }
    }
}
