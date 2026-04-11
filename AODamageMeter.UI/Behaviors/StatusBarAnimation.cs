using AODamageMeter.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace AODamageMeter.UI.Behaviors
{
    public static class StatusBarAnimation
    {
        public static readonly DependencyProperty StatusBarViewModelProperty =
            DependencyProperty.RegisterAttached(
                "StatusBarViewModel",
                typeof(StatusBarViewModelBase),
                typeof(StatusBarAnimation),
                new PropertyMetadata(null, OnStatusBarViewModelChanged));

        public static StatusBarViewModelBase GetStatusBarViewModel(ProgressBar progressBar)
            => (StatusBarViewModelBase)progressBar.GetValue(StatusBarViewModelProperty);

        public static void SetStatusBarViewModel(ProgressBar progressBar, StatusBarViewModelBase value)
            => progressBar.SetValue(StatusBarViewModelProperty, value);

        private static void OnStatusBarViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ProgressBar progressBar) || !(e.NewValue is StatusBarViewModelBase statusBarViewModel))
                return;

            if (statusBarViewModel is FixedStatusBarViewModel fixedStatusBarViewModel)
            {
                progressBar.Maximum = 1.0;
                progressBar.BeginAnimation(RangeBase.ValueProperty, null);
                progressBar.SetBinding(RangeBase.ValueProperty, new Binding(nameof(FixedStatusBarViewModel.Progress))
                {
                    Source = fixedStatusBarViewModel,
                    Mode = BindingMode.OneWay
                });
            }
            else if (statusBarViewModel is AnimatedStatusBarViewModel animatedStatusBarViewModel)
            {
                progressBar.Maximum = animatedStatusBarViewModel.TotalSeconds.Value;
                double remainingSeconds = animatedStatusBarViewModel.RemainingSeconds;

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
}
