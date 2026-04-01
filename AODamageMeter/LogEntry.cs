using System;
using System.Linq;

namespace AODamageMeter
{
    public readonly struct LogEntry
    {
        private static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Log lines look like this, the array part always having four elements:
        // ["#000000004200000a#","Other hit by other","",1492309026]Elitengi hit Leet for 6 points of cold damage.
        public LogEntry(string logLine)
        {
            int lastIndexOfArrayPart = logLine.IndexOf(']');
            string[] arrayPart = logLine.Substring(1, lastIndexOfArrayPart - 1).Split(',')
                .Select(p => p.Trim('"'))
                .ToArray();
            EventName = arrayPart[1];
            UnixSeconds = long.Parse(arrayPart[3]);
            Description = logLine.Substring(lastIndexOfArrayPart + 1);
        }

        public string EventName { get; }
        public long UnixSeconds { get; }
        public string Description { get; }

        public DateTime Timestamp => _unixEpoch.AddSeconds(UnixSeconds).ToLocalTime();
    }
}
