using System;

namespace AODamageMeter.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime DateTimeLocalFromUnixSeconds(long seconds)
            => _unixEpoch.AddSeconds(seconds).ToLocalTime();
    }
}
