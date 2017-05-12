using AODamageMeter.FightEvents.Attack;
using AODamageMeter.FightEvents.Heal;
using AODamageMeter.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace AODamageMeter.UI.ViewModels
{
    public class CharacterInfoViewModel : ViewModelBase
    {
        public CharacterInfoViewModel(string characterName = null, string logFilePath = null)
        {
            CharacterName = characterName;
            LogFilePath = logFilePath;
            AutoConfigureCommand = new RelayCommand(CanExecuteAutoConfigureCommand, ExecuteAutoConfigureCommand);
        }

        private string _characterName;
        public string CharacterName
        {
            get => _characterName;
            set
            {
                // When this changes null out the configure result to avoid confusion over stale data.
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
            set => Set(ref _logFilePath, value);
        }

        public bool IsEmpty
            => string.IsNullOrWhiteSpace(CharacterName)
            && string.IsNullOrWhiteSpace(LogFilePath);

        private string _autoConfigureResult;
        public string AutoConfigureResult
        {
            get => _autoConfigureResult;
            set => Set(ref _autoConfigureResult, value);
        }

        public ICommand AutoConfigureCommand { get; }
        private bool CanExecuteAutoConfigureCommand() => Character.FitsPlayerNamingRequirements(CharacterName);
        private void ExecuteAutoConfigureCommand()
        {
            var characterAndBioRetriever = Character.GetOrCreateCharacterAndBioRetriever(CharacterName);
            var character = characterAndBioRetriever.character;
            characterAndBioRetriever.bioRetriever.Wait(); // Not worth using await and binding IsEnableds.

            string autoConfiguredLogFilePath = null;
            if (character.ID == null)
            {
                AutoConfigureResult = $"Auto-configure failed. Could not find character ID of {CharacterName} on http://people.anarchy-online.com/.";
            }
            else
            {
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
                }
                else if (!Directory.Exists(chatWindowsPath))
                {
                    AutoConfigureResult = $"Auto-configure failed. Could not find chat windows folder under {prefsPath}.";
                }
                else
                {
                    // C:\Users\{user}\AppData\Local\Funcom\Anarchy Online\{installation ID}\Anarchy Online\Prefs\{account name}\Char#########\Chat\Windows\Window#
                    foreach (string path in Directory.EnumerateDirectories(chatWindowsPath, "Window*")
                        .Where(p => File.Exists($@"{p}\Config.xml")))
                    {
                        string configText = File.ReadAllText($@"{path}\Config.xml");
                        if (RequiredConfigGroupNames.All(n => configText.Contains(n))
                            && configText.Contains("name=\"is_logged\" value=\"true\""))
                        {
                            autoConfiguredLogFilePath = $@"{path}\Log.txt";
                            if (!File.Exists(autoConfiguredLogFilePath))
                            {
                                File.Create(autoConfiguredLogFilePath);
                            }
                            AutoConfigureResult = "Auto-configure succeeded. An existing log file was found.";
                            break;
                        }
                        else if (configText.Contains("Damage Meter Window"))
                        {
                            File.WriteAllText($@"{path}\Config.xml", GetAutoConfigureConfigXml(path.Split('\\').Last()));
                            autoConfiguredLogFilePath = $@"{path}\Log.txt";
                            if (!File.Exists(autoConfiguredLogFilePath))
                            {
                                File.Create(autoConfiguredLogFilePath);
                            }
                            AutoConfigureResult = "Auto-configure succeeded. An existing log file was found and reconfigured.";
                            break;
                        }
                    }

                    if (autoConfiguredLogFilePath == null)
                    {
                        int nextAvailableWindowNumber = 1;
                        while (Directory.Exists($@"{chatWindowsPath}\Window{nextAvailableWindowNumber}"))
                        {
                            ++nextAvailableWindowNumber;
                        }
                        string newWindowName = $@"Window{nextAvailableWindowNumber}";
                        Directory.CreateDirectory($@"{chatWindowsPath}\{newWindowName}");
                        File.WriteAllText($@"{chatWindowsPath}\{newWindowName}\Config.xml", GetAutoConfigureConfigXml(newWindowName));
                        autoConfiguredLogFilePath = $@"{chatWindowsPath}\{newWindowName}\Log.txt";
                        File.Create(autoConfiguredLogFilePath);
                        bool isAlreadyLoggedIn = Process.GetProcessesByName("AnarchyOnline")
                            .Where(p => p.MainWindowTitle?.StartsWith("Anarchy Online - ") ?? false)
                            .Any(p => p.MainWindowTitle.Contains(CharacterName));
                        AutoConfigureResult = isAlreadyLoggedIn ? "Auto-configure succeeded. A new log file was created, but you'll need to relog."
                            : "Auto-configure succeeded. A new log file was created.";
                    }
                }
            }

            if (autoConfiguredLogFilePath != null)
            {
                LogFilePath = autoConfiguredLogFilePath;
            }
        }

        // Doesn't include all the events that AODamageMeter logs, just the ones that seem important enough right now.
        private static IReadOnlyList<string> RequiredConfigGroupNames = new string[]
        {
            MeHitByEnvironment.EventName, MeHitByMonster.EventName, MeHitByNano.EventName, MeHitByPlayer.EventName, OtherHitByNano.EventName,
            OtherHitByOther.EventName, OtherMisses.EventName, YouHitOther.EventName, YouHitOtherWithNano.EventName, YourMisses.EventName,
            YourPetHitByMonster.EventName, YourPetHitByNano.EventName, YourPetHitByOther.EventName, MeGotHealth.EventName, MeGotNano.EventName,
            YouGaveHealth.EventName, YouGaveNano.EventName
        };

        // Includes all the events that AODamamgeMeter logs, not just the ones that seem important enough right now.
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
        <Int64 value=""1073741825"" />
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
        <String value='&quot;System&quot;' />
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
    <String name=""name"" value='&quot;Damage Meter Window&quot;' />
</Archive>
";
    }
}
