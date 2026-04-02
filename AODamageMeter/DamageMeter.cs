using System;
using System.Collections.Generic;
using System.IO;

namespace AODamageMeter
{
    public class DamageMeter : IDisposable
    {
        protected readonly StreamReader _logStreamReader;
        protected readonly List<LogEntry> _playbackLogEntries = new List<LogEntry>();
        protected readonly double _playbackSpeed;
        protected int _playbackStartIndex;
        protected int _playbackIndex;

        public DamageMeter(string characterName, Dimension dimension, string logFilePath,
            DamageMeterMode mode = DamageMeterMode.Live, double? playbackSpeed = null)
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

                _playbackSpeed = playbackSpeed ?? 1.0;
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

        public void Update()
        {
            if (IsPlaybackMode)
            {
                if (IsPaused || _playbackIndex >= _playbackLogEntries.Count) return;

                long baseUnixSeconds = _playbackLogEntries[_playbackStartIndex].UnixSeconds;
                long targetUnixSeconds = baseUnixSeconds + (long)((CurrentFight.Duration?.TotalSeconds ?? 0) * _playbackSpeed);

                while (_playbackIndex < _playbackLogEntries.Count
                    && _playbackLogEntries[_playbackIndex].UnixSeconds <= targetUnixSeconds)
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
