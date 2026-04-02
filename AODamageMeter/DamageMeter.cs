using System;
using System.Collections.Generic;
using System.IO;

namespace AODamageMeter
{
    public class DamageMeter : IDisposable
    {
        protected readonly StreamReader _logStreamReader;
        protected readonly List<LogEntry> _playbackLogEntries = new List<LogEntry>();
        protected int _playbackStartIndex;
        protected int _playbackIndex;
        protected long _playbackBaseUnixSeconds;
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
            : TimeSpan.FromSeconds(_playbackSkipAheadSeconds + (CurrentFight.Duration?.TotalSeconds ?? 0));

        public void InitializeNewFight(bool saveCurrentFight = false)
        {
            if (CurrentFight != null)
            {
                CurrentFight.IsPaused = !IsSummaryMode;
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
                _playbackStartIndex = _playbackIndex;
                _playbackBaseUnixSeconds = _playbackLogEntries[_playbackStartIndex].UnixSeconds;
                _playbackSkipAheadSeconds = 0;
            }

            CurrentFight = new Fight(this) { IsPaused = IsPaused };
        }

        protected bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (IsSummaryMode && !value) return;
                if (IsSummaryMode) throw new NotSupportedException("Pausing is not supported in summary mode.");

                _isPaused = value;

                if (CurrentFight != null)
                {
                    CurrentFight.IsPaused = IsPaused;
                }
            }
        }

        public void SkipAheadPlayback(int seconds)
            => _playbackSkipAheadSeconds += seconds;

        public void Update()
        {
            if (IsPlaybackMode)
            {
                if (IsPaused || _playbackIndex >= _playbackLogEntries.Count) return;

                long _playbackTargetUnixSeconds = _playbackBaseUnixSeconds + (int)PlaybackDuration.Value.TotalSeconds;

                while (_playbackIndex < _playbackLogEntries.Count
                    && _playbackLogEntries[_playbackIndex].UnixSeconds <= _playbackTargetUnixSeconds)
                {
                    CurrentFight.AddFightEvent(_playbackLogEntries[_playbackIndex]);
                    _playbackIndex++;
                }

                if (_playbackIndex >= _playbackLogEntries.Count)
                {
                    IsPaused = true;
                }
            }
            else
            {
                string logLine;
                while ((logLine = _logStreamReader.ReadLine()) != null)
                {
                    CurrentFight.AddFightEvent(logLine);
                }
            }
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
