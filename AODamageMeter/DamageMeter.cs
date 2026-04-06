using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AODamageMeter
{
    public class DamageMeter : IDisposable
    {
        protected readonly StreamReader _logStreamReader;
        protected readonly Stopwatch _playbackStopwatch = new Stopwatch();
        protected readonly List<LogEntry> _playbackLogEntries = new List<LogEntry>();
        protected int _playbackLogIndex;
        protected long _playbackLogStartUnixSeconds;
        protected int _playbackSkipAheadSeconds;

        public DamageMeter(string characterName, Dimension dimension, string logFilePath,
            DamageMeterMode mode = DamageMeterMode.Live)
        {
            Dimension = dimension;
            LogFilePath = logFilePath;
            _logStreamReader = new StreamReader(File.Open(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            Mode = mode;
            Owner = Character.GetOrCreateCharacter(characterName, dimension);
            Owner.IsPlayer = true;

            if (IsPlaybackMode)
            {
                string logLine;
                while ((logLine = _logStreamReader.ReadLine()) != null)
                {
                    _playbackLogEntries.Add(new LogEntry(logLine));
                }
            }
        }

        public Dimension Dimension { get; }
        public string LogFilePath { get; }
        public DamageMeterMode Mode { get; }
        public bool IsLiveMode => Mode == DamageMeterMode.Live;
        public bool IsSummaryMode => Mode == DamageMeterMode.Summary;
        public bool IsPlaybackMode => Mode == DamageMeterMode.Playback;
        public Character Owner { get; }
        public IReadOnlyDictionary<string, string> PetRegistrations { get; set; }

        protected readonly List<Fight> _previousFights = new List<Fight>();
        public IReadOnlyList<Fight> PreviousFights => _previousFights;
        public Fight CurrentFight { get; protected set; }

        public TimeSpan? PlaybackDuration
            => !IsPlaybackMode ? (TimeSpan?)null
            : TimeSpan.FromSeconds(_playbackSkipAheadSeconds + _playbackStopwatch.Elapsed.TotalSeconds);

        public void InitializeNewFight(bool saveCurrentFight = false)
        {
            if (CurrentFight != null)
            {
                CurrentFight.IsPaused = true;
                CurrentFight.EndTime = !IsSummaryMode ? DateTime.Now : CurrentFight.LatestEventTime;

                if (saveCurrentFight)
                {
                    _previousFights.Add(CurrentFight);
                }
            }

            if (IsLiveMode)
            {
                SkipToEndOfLog();
            }
            else if (IsPlaybackMode)
            {
                _playbackLogIndex = 0;
                _playbackLogStartUnixSeconds = _playbackLogEntries.Count != 0
                    ? _playbackLogEntries[_playbackLogIndex].UnixSeconds : 0;
                _playbackSkipAheadSeconds = 0;
                if (IsPaused) _playbackStopwatch.Reset();
                else _playbackStopwatch.Restart();
            }

            CurrentFight = new Fight(this) { IsPaused = IsPaused };
        }

        protected bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (IsSummaryMode) return;

                _isPaused = value;

                if (CurrentFight != null)
                {
                    CurrentFight.IsPaused = IsPaused;
                }

                if (IsPlaybackMode)
                {
                    if (IsPaused) _playbackStopwatch.Stop();
                    else _playbackStopwatch.Start();
                }
            }
        }

        public void SkipAheadPlayback(int seconds)
            => _playbackSkipAheadSeconds += seconds;

        public void Update()
        {
            if (IsLiveMode || IsSummaryMode)
            {
                string logLine;
                while ((logLine = _logStreamReader.ReadLine()) != null)
                {
                    CurrentFight.AddFightEvent(logLine);
                }
            }
            else if (IsPlaybackMode)
            {
                if (IsPaused || _playbackLogIndex >= _playbackLogEntries.Count) return;

                long _playbackTargetUnixSeconds = _playbackLogStartUnixSeconds + (int)PlaybackDuration.Value.TotalSeconds;

                while (_playbackLogIndex < _playbackLogEntries.Count
                    && _playbackLogEntries[_playbackLogIndex].UnixSeconds <= _playbackTargetUnixSeconds)
                {
                    CurrentFight.AddFightEvent(_playbackLogEntries[_playbackLogIndex]);
                    _playbackLogIndex++;
                }

            }
            else throw new NotImplementedException();
        }

        public void SkipToStartOfLog()
            => _logStreamReader.BaseStream.Seek(0, SeekOrigin.Begin);

        public void SkipToEndOfLog()
            => _logStreamReader.BaseStream.Seek(0, SeekOrigin.End);

        public void Dispose()
            => _logStreamReader.Dispose();

        public override string ToString()
            => $"{Mode} damage meter owned by {Owner}, with {PreviousFights.Count + 1} fights.";
    }
}
