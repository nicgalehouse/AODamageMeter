using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace AODamageMeter
{
    public static class PlayerBioCache
    {
        private static readonly string _cachePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly().Location),
            "PlayerBioCache.json");
        private static readonly Dictionary<string, PlayerBioData> _cache;
        private static readonly object _lock = new object();
        private static readonly TimeSpan _maxAgeWithPlayerInfo = TimeSpan.FromDays(2);
        private static readonly TimeSpan _maxAgeWithoutPlayerInfo = TimeSpan.FromHours(1);
        private static readonly TimeSpan _flushDelay = TimeSpan.FromMinutes(5);
        private static Timer _flushTimer;

        static PlayerBioCache()
        {
            try
            {
                if (File.Exists(_cachePath))
                {
                    string json = File.ReadAllText(_cachePath);
                    _cache = JsonConvert.DeserializeObject<Dictionary<string, PlayerBioData>>(json)
                        ?? new Dictionary<string, PlayerBioData>();
                }
            }
            catch
            {
                // If the cache file is corrupt, start fresh.
            }

            _cache = _cache ?? new Dictionary<string, PlayerBioData>();
        }

        public static bool IsStale(Character character)
        {
            lock (_lock)
            {
                if (!_cache.TryGetValue(character.Key, out PlayerBioData playerBioData)
                    || !playerBioData.LastUpdatedTimestamp.HasValue)
                    return true;

                return playerBioData.HasPlayerInfo ? playerBioData.Age >= _maxAgeWithPlayerInfo
                    : playerBioData.Age >= _maxAgeWithoutPlayerInfo;
            }
        }

        public static bool TryGet(Character character, out PlayerBioData playerBioData)
        {
            lock (_lock)
            {
                return _cache.TryGetValue(character.Key, out playerBioData);
            }
        }

        public static void AddOrUpdate(Character character, PlayerBioData playerBioData)
        {
            lock (_lock)
            {
                _cache[character.Key] = playerBioData;
                ScheduleFlush();
            }
        }

        private static void ScheduleFlush()
        {
            if (_flushTimer == null)
            {
                _flushTimer = new Timer(_ => Flush(), null, _flushDelay, Timeout.InfiniteTimeSpan);
            }
            else
            {
                _flushTimer.Change(_flushDelay, Timeout.InfiniteTimeSpan);
            }
        }

        public static void Flush()
        {
            lock (_lock)
            {
                try
                {
                    File.WriteAllText(_cachePath, JsonConvert.SerializeObject(_cache, Formatting.Indented));
                }
                catch
                {
                    // Don't crash the app if the file can't be written.
                }
            }
        }
    }
}
