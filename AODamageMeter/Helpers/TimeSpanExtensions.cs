using System;

namespace AODamageMeter.Helpers
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan WithoutMilliseconds(this TimeSpan timeSpan)
            => new TimeSpan(timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }
}
