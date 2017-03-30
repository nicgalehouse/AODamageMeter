using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class DamageMeter : IDisposable
    {
        private string _logPath;
        private FileStream _logFileStream;
        private StreamReader _logStreamReader;
        private string _owningCharacterName;
        //private List<Fight> pastFights = new List<Fight>();

        public DamageMeter()
        { }

        public DamageMeter(string logPath)
        {
            _logPath = logPath;
            _logFileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _logStreamReader = new StreamReader(_logFileStream);
            _logStreamReader.ReadToEnd();

            SetOwningCharacterName();
        }

        public DamageMeter(string logPath, Action<DamageMeter> updater, TimeSpan updateInterval)
            : this(logPath)
        {
            Task.Run(async () =>
            {
                updater(this);

                await Task.Delay(updateInterval);
            });
        }

        private void SetOwningCharacterName()
        {
            string owningCharacterID = _logPath.Split('\\', '/')
                .LastOrDefault(d => d.StartsWith("Char"))
                ?.Remove(0, "Char".Length);

            if (owningCharacterID != null)
            {
                var potentialCharacterNames = Process.GetProcessesByName("AnarchyOnline")
                    .Where(p => p.MainWindowTitle.StartsWith("Anarchy Online - "))
                    .Select(p => p.MainWindowTitle.Remove(0, "Anarchy Online - ".Length));

                foreach (string characterName in potentialCharacterNames)
                {
                    string characterID = new CharacterBio(characterName).CharacterInfo?.CharacterID;
                    if (owningCharacterID == characterID)
                    {
                        _owningCharacterName = characterName;
                        break;
                    }
                }
            }

            _owningCharacterName = _owningCharacterName ?? "You";
        }

        public Fight CurrentFight { get; set; } = new Fight();

        public void Update()
        {
            string line;
            while ((line = _logStreamReader.ReadLine()) != null)
            {
                CurrentFight.AddEvent(new FightEvent(line, _owningCharacterName));
            }
            CurrentFight.UpdateCharactersTime();
        }

        public void Dispose()
        {
            _logFileStream.Dispose();
            _logStreamReader.Dispose();
        }
    }
}
