using AODamageMeter.FightEvents.Attack;
using AODamageMeter.FightEvents.Heal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace AODamageMeter.UI.ViewModels
{
    public sealed class CharacterInfoViewModel : ViewModelBase
    {
        private CharacterSelectionViewModel _characterSelectionViewModel;

        public CharacterInfoViewModel(CharacterSelectionViewModel characterSelectionViewModel,
            string characterName = null, string logFilePath = null)
        {
            _characterSelectionViewModel = characterSelectionViewModel;
            CharacterName = characterName;
            LogFilePath = logFilePath;
            AutoConfigureCommand = new RelayCommand(ExecuteAutoConfigureCommand);
        }

        private string _characterName;
        public string CharacterName
        {
            get => _characterName;
            set
            {
                if (Set(ref _characterName, value))
                {
                    AutoConfigureResult = null;
                }
            }
        }

        private string _logFilePath;
        public string LogFilePath
        {
            get => _logFilePath;
            set
            {
                if (Set(ref _logFilePath, value))
                {
                    RefreshLogFileSize();
                }
            }
        }

        private string _logFileSize;
        public string LogFileSize
        {
            get => _logFileSize;
            set => Set(ref _logFileSize, value);
        }

        public void RefreshLogFileSize()
        {
            if (File.Exists(_logFilePath))
            {
                try
                {
                    var fileInfo = new FileInfo(_logFilePath);
                    LogFileSize = $"{fileInfo.Length / ((double)1024):N0} KB";
                }
                catch
                {
                    LogFileSize = null;
                }
            }
            else
            {
                LogFileSize = null;
            }
        }

        public bool IsEmpty
            => string.IsNullOrWhiteSpace(CharacterName) && string.IsNullOrWhiteSpace(LogFilePath);

        private string _autoConfigureResult;
        public string AutoConfigureResult
        {
            get => _autoConfigureResult;
            set => Set(ref _autoConfigureResult, value);
        }

        public ICommand AutoConfigureCommand { get; }
        private void ExecuteAutoConfigureCommand()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CharacterName))
                {
                    var loggedInCharacterNames = Process.GetProcessesByName("AnarchyOnline")
                        .Where(p => p.MainWindowTitle?.StartsWith("Anarchy Online - ") ?? false)
                        .Select(p => p.MainWindowTitle.Substring("Anarchy Online - ".Length))
                        .ToArray();
                    var unconfiguredCharacterNames = loggedInCharacterNames
                        .Where(n => _characterSelectionViewModel.CharacterInfoViewModels.All(c => c.CharacterName != n))
                        .ToArray();
                    if (loggedInCharacterNames.Length == 0)
                    {
                        AutoConfigureResult = "Auto-configure failed. Unable to detect a running instance of AO from which to deduce a character name. Please enter a name manually.";
                        return;
                    }
                    else if (loggedInCharacterNames.Length > 1 && unconfiguredCharacterNames.Length != 1)
                    {
                        AutoConfigureResult = "Auto-configure failed. Can't deduce a sole character needing configuration from the running instances of AO. Please enter a name manually.";
                        return;
                    }
                    CharacterName = loggedInCharacterNames.Length == 1 ? loggedInCharacterNames.Single() : unconfiguredCharacterNames.Single();
                }

                if (!Character.FitsPlayerNamingRequirements(CharacterName))
                {
                    AutoConfigureResult = $"Auto-configure failed. {CharacterName} is not a valid character name.";
                    return;
                }

                var characterAndBioRetriever = Character.GetOrCreateCharacterAndBioRetriever(CharacterName);
                var character = characterAndBioRetriever.character;
                characterAndBioRetriever.bioRetriever.Wait(); // Not worth using await and binding IsEnableds.
                if (!character.HasPlayerInfo)
                {
                    if (long.TryParse(LogFilePath?.Split(':').Last(), out long characterID))
                    {
                        character.ID = characterID.ToString();
                    }
                    else
                    {
                        AutoConfigureResult = $"Auto-configure failed. Could not find character ID of {CharacterName} on http://people.anarchy-online.com/."
                            + " If you just created this character, as a workaround you can Shift+F9 in-game and copy & paste the displayed character ID into the log file input and try again.";
                        return;
                    }
                }

                // C:\Users\{user}\AppData\Local\Funcom\Anarchy Online
                string basePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Funcom\Anarchy Online";
                // C:\Users\{user}\AppData\Local\Funcom\Anarchy Online\{installation ID}\Anarchy Online\Prefs\{account name}\Char#########
                string prefsPath = Directory.EnumerateDirectories(basePath, $"Char{character.ID}", SearchOption.AllDirectories)
                    // Could be multiple matching pref folders from across installations; choose the one w/ the most recent activity.
                    .OrderByDescending(p => File.GetLastWriteTime($@"{p}\Prefs.xml"))
                    .FirstOrDefault();
                // C:\Users\{user}\AppData\Local\Funcom\Anarchy Online\{installation ID}\Anarchy Online\Prefs\{account name}\Char#########\Chat\Windows
                string chatWindowsPath = $@"{prefsPath}\Chat\Windows";
                if (prefsPath == null)
                {
                    AutoConfigureResult = $"Auto-configure failed. Could not find Char{character.ID}'s prefs folder under {basePath}.";
                    return;
                }
                if (!Directory.Exists(chatWindowsPath))
                {
                    AutoConfigureResult = $"Auto-configure failed. Could not find chat windows folder under {prefsPath}.";
                    return;
                }

                // C:\Users\{user}\AppData\Local\Funcom\Anarchy Online\{installation ID}\Anarchy Online\Prefs\{account name}\Char#########\Chat\Windows\Window#
                foreach (string path in Directory.EnumerateDirectories(chatWindowsPath, "Window*")
                    .Where(p => File.Exists($@"{p}\Config.xml")))
                {
                    string configText = File.ReadAllText($@"{path}\Config.xml");
                    if (RequiredConfigGroupNames.All(n => configText.Contains(n))
                        && configText.Contains("name=\"is_logged\" value=\"true\""))
                    {
                        LogFilePath = $@"{path}\Log.txt";
                        if (!File.Exists(LogFilePath))
                        {
                            File.Create(LogFilePath);
                            RefreshLogFileSize();
                        }
                        AutoConfigureResult = "Auto-configure succeeded. An existing log file was found.";
                        return;
                    }
                    else if (configText.Contains("Damage Meter Window")
                        || configText.Contains("Damage Meter Log"))
                    {
                        File.WriteAllText($@"{path}\Config.xml", GetAutoConfigureConfigXml(path.Split('\\').Last()));
                        LogFilePath = $@"{path}\Log.txt";
                        if (!File.Exists(LogFilePath))
                        {
                            File.Create(LogFilePath);
                            RefreshLogFileSize();
                        }
                        AutoConfigureResult = "Auto-configure succeeded. An existing log file was found and reconfigured.";
                        return;
                    }
                }

                int firstAvailableWindowNumber = 1;
                while (Directory.Exists($@"{chatWindowsPath}\Window{firstAvailableWindowNumber}"))
                {
                    ++firstAvailableWindowNumber;
                }
                string newWindowName = $@"Window{firstAvailableWindowNumber}";
                Directory.CreateDirectory($@"{chatWindowsPath}\{newWindowName}");
                File.WriteAllText($@"{chatWindowsPath}\{newWindowName}\Config.xml", GetAutoConfigureConfigXml(newWindowName));
                LogFilePath = $@"{chatWindowsPath}\{newWindowName}\Log.txt";
                File.Create(LogFilePath);
                RefreshLogFileSize();
                bool isAlreadyLoggedIn = Process.GetProcessesByName("AnarchyOnline")
                    .Any(p => p.MainWindowTitle?.Contains(CharacterName) ?? false);
                AutoConfigureResult = isAlreadyLoggedIn
                    ? "Auto-configure succeeded. A new log file was created, but you'll need to relog (a fast quit is fine)."
                    : "Auto-configure succeeded. A new log file was created.";
            }
            catch (Exception exception)
            {
                AutoConfigureResult = $"Auto-configure failed with an unexpected error: {exception.Message}";
            }
        }

        // Doesn't include all the events that AODamageMeter logs, just the ones that seem important enough.
        private static IReadOnlyList<string> RequiredConfigGroupNames = new string[]
        {
            MeHitByEnvironment.EventName, MeHitByMonster.EventName, MeHitByNano.EventName, MeHitByPlayer.EventName, OtherHitByNano.EventName,
            OtherHitByOther.EventName, OtherMisses.EventName, YouHitOther.EventName, YouHitOtherWithNano.EventName, YourMisses.EventName,
            YourPetHitByMonster.EventName, YourPetHitByNano.EventName, YourPetHitByOther.EventName, MeGotHealth.EventName, MeGotNano.EventName,
            YouGaveHealth.EventName, YouGaveNano.EventName
        };

        private static string GetAutoConfigureConfigXml(string windowName) =>
$@"<Archive code=""0"">
    <Array name=""selected_group_ids"">
        <Int64 value=""1107296260"" />
        <Int64 value=""1107296262"" />
        <Int64 value=""1107296263"" />
        <Int64 value=""1107296264"" />
        <Int64 value=""1107296265"" />
        <Int64 value=""1107296261"" />
        <Int64 value=""1107296259"" />
        <Int64 value=""1107296258"" />
        <Int64 value=""1107296284"" />
        <Int64 value=""1107296266"" />
        <Int64 value=""1107296257"" />
        <Int64 value=""1107296277"" />
        <Int64 value=""1107296279"" />
        <Int64 value=""1107296267"" />
        <Int64 value=""1107296273"" />
        <Int64 value=""1107296274"" />
        <Int64 value=""1107296275"" />
        <Int64 value=""1107296280"" />
        <Int64 value=""1107296268"" />
        <Int64 value=""1107296278"" />
        <Int64 value=""1107296276"" />
    </Array>
    <Array name=""selected_group_names"">
        <String value='&quot;Other hit by nano&quot;' />
        <String value='&quot;Me hit by monster&quot;' />
        <String value='&quot;Me hit by player&quot;' />
        <String value='&quot;You hit other&quot;' />
        <String value='&quot;Your pet hit by other&quot;' />
        <String value='&quot;You hit other with nano&quot;' />
        <String value='&quot;Your pet hit by nano&quot;' />
        <String value='&quot;Me hit by nano&quot;' />
        <String value='&quot;Research&quot;' />
        <String value='&quot;Other hit by other&quot;' />
        <String value='&quot;Me hit by environment&quot;' />
        <String value='&quot;Me got health&quot;' />
        <String value='&quot;You gave nano&quot;' />
        <String value='&quot;Me got XP&quot;' />
        <String value='&quot;Your pet hit by monster&quot;' />
        <String value='&quot;Your misses&quot;' />
        <String value='&quot;Other misses&quot;' />
        <String value='&quot;Me Cast Nano&quot;' />
        <String value='&quot;Me got SK&quot;' />
        <String value='&quot;Me got nano&quot;' />
        <String value='&quot;You gave health&quot;' />
    </Array>
    <Archive code=""0"" name=""log_window_config"">
        <Rect name=""WindowFrame"" value=""Rect(850.000000,430.000000,1383.000000,732.000000)"" />
        <Bool name=""WindowPinButtonState"" value=""false"" />
    </Archive>
    <Archive code=""0"" name=""chat_window_config"">
        <Bool name=""WindowPinButtonState"" value=""false"" />
        <Rect name=""WindowFrame"" value=""Rect(1304.000000,566.000000,1704.000000,766.000000)"" />
        <Bool name=""is_backmost"" value=""false"" />
        <Bool name= ""is_frontmost"" value=""false"" />
    </Archive>
    <Archive code=""0"" name=""chat_view_config"" />
    <Int32 name=""visual_mode"" value=""0"" />
    <String name=""output_group"" value='&quot;#0000000040000002#&quot;' />
    <Float name=""window_transparency_inactive"" value=""0.300000"" />
    <Float name=""window_transparency_active"" value=""0.800000"" />
    <Bool name=""show_timestamps"" value=""false"" />
    <Bool name=""hide_input_when_inactive"" value=""false"" />
    <Bool name=""deactivate_on_send"" value=""true"" />
    <Bool name=""is_textinput_enabled"" value=""false"" />
    <Bool name=""is_clickthrough"" value=""false"" />
    <Bool name=""is_logged"" value=""true"" />
    <Bool name=""is_message_fading_enabled"" value=""false"" />
    <Bool name=""is_autosubscribe_window"" value=""false"" />
    <Bool name=""is_window_open"" value=""false"" />
    <Int32 name=""tab_index"" value=""0"" />
    <String name=""window_name"" value='&quot;{windowName}&quot;' />
    <Bool name=""is_default_window"" value=""false"" />
    <Bool name=""is_startup_window"" value=""false"" />
    <String name=""name"" value='&quot;Damage Meter Log&quot;' />
</Archive>
";
    }
}
