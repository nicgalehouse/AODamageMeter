using System;
using System.Collections.Generic;
using System.IO;

namespace AODamageMeter
{
    public class DamageMeter : IDisposable
    {
        protected readonly StreamReader _logStreamReader;

        public DamageMeter(string characterName, string logFilePath, DamageMeterMode mode = DamageMeterMode.RealTime)
        {
            LogFilePath = logFilePath;
            _logStreamReader = new StreamReader(File.Open(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            Mode = mode;
            Owner = Character.GetOrCreateCharacter(characterName);
            Owner.CharacterType = CharacterType.Player;
        }

        public string LogFilePath { get; }
        public DamageMeterMode Mode { get; }
        public bool IsRealTimeMode => Mode == DamageMeterMode.RealTime;
        public bool IsParsedTimeMode => Mode == DamageMeterMode.ParsedTime;
        public Character Owner { get; protected set; }

        protected readonly List<Fight> _previousFights = new List<Fight>();
        public IReadOnlyList<Fight> PreviousFights => _previousFights;
        public Fight CurrentFight { get; protected set; }

        public void InitializeNewFight(bool skipToEndOfLog = true, bool saveCurrentFight = false)
        {
            if (CurrentFight != null)
            {
                CurrentFight.IsPaused = IsRealTimeMode;
                CurrentFight.EndTime = IsRealTimeMode ? DateTime.Now : CurrentFight.LatestEventTime;

                if (saveCurrentFight)
                {
                    _previousFights.Add(CurrentFight);
                }
            }

            if (skipToEndOfLog)
            {
                SkipToEndOfLog();
            }

            CurrentFight = new Fight(this) { IsPaused = IsPaused };
        }

        protected bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (IsParsedTimeMode && !value) return;
                if (IsParsedTimeMode) throw new NotSupportedException("Pausing for parsed-time meters isn't supported yet.");

                _isPaused = value;

                if (CurrentFight != null)
                {
                    CurrentFight.IsPaused = IsPaused;
                }
            }
        }

        public void Update()
        {
            string line;
            while ((line = _logStreamReader.ReadLine()) != null)
            {
                CurrentFight.AddFightEvent(line);
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
