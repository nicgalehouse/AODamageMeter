using System;
using System.Diagnostics;

namespace AODamageMeter.UI.Helpers
{
    public sealed class SynchronizedStopwatch
    {
        private readonly object _lock = new object();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public TimeSpan Elapsed
        {
            get { lock (_lock) { return _stopwatch.Elapsed; } }
        }

        public void Start()
        {
            lock (_lock) { _stopwatch.Start(); }
        }

        public void Restart()
        {
            lock (_lock) { _stopwatch.Restart(); }
        }

        public void Reset()
        {
            lock (_lock) { _stopwatch.Reset(); }
        }
    }
}
