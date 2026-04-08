using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AODamageMeter.ChatFilterHelper
{
    public class Program
    {
        public static void Main()
        {
            string[] longInterleavableMessages = ReadMessages(Path.Combine("Messages", "LongInterleavableMessages.txt"));
            string[] shortInterleavableMessages = ReadMessages(Path.Combine("Messages", "ShortInterleavableMessages.txt"));
            string[] noninterleavableMessages = ReadMessages(Path.Combine("Messages", "NoninterleavableMessages.txt"));
            string[] allMessages = longInterleavableMessages.Concat(shortInterleavableMessages).Concat(noninterleavableMessages).ToArray();
            var filterPatterns = new List<string>();

            foreach (string[] interleavableMessages in new[] { longInterleavableMessages, shortInterleavableMessages })
            {
                var charClasses = BuildCharClasses(interleavableMessages);
                string filterPattern = BuildFilterPattern(charClasses);
                filterPatterns.Add(filterPattern);
            }

            foreach (string message in noninterleavableMessages)
            {
                filterPatterns.Add($"^{message}$");
            }

            var filterRegexes = new List<Regex>();
            var resultsOutput = new List<string>();
            foreach (string pattern in filterPatterns)
            {
                filterRegexes.Add(new Regex(pattern));
                resultsOutput.Add($"/filter add {pattern}");
                Console.WriteLine($"/filter add {pattern}");
            }
            Console.WriteLine();

            string unfilteredSampleLogDirectory = "UnfilteredSampleLogs";
            var unfilteredSampleLogFiles = Directory.GetFiles(unfilteredSampleLogDirectory, "*.txt");
            VerifyMessagesGetFiltered(filterRegexes, allMessages);
            TestForFalsePositives(filterRegexes, allMessages, unfilteredSampleLogFiles, resultsOutput);
            GenerateFilteredLogs(filterRegexes, unfilteredSampleLogFiles, resultsOutput);
            File.WriteAllLines(Path.Combine("..", "..", "FilterResults.txt"), resultsOutput);
        }

        public static string[] ReadMessages(string path)
        {
            if (!File.Exists(path)) return new string[0];

            return File.ReadAllLines(path)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#"))
                .ToArray();
        }

        public static List<char[]> BuildCharClasses(string[] interleavableMessages)
        {
            int patternLength = interleavableMessages.Min(m => m.Length);

            var charClasses = new List<char[]>();
            for (int position = 0; position < patternLength; position++)
            {
                var chars = new HashSet<char>();
                foreach (string message in interleavableMessages)
                {
                    chars.Add(message[position]);
                }
                charClasses.Add(SortCharClass(chars));
            }

            return charClasses;
        }

        public static char[] SortCharClass(HashSet<char> chars)
        {
            var upper = chars.Where(char.IsUpper).OrderBy(c => c);
            var lower = chars.Where(char.IsLower).OrderBy(c => c);
            var digits = chars.Where(char.IsDigit).OrderBy(c => c);
            var special = chars.Where(c => !char.IsLetterOrDigit(c) && c != ' ' && c != '-').OrderBy(c => c);
            var space = chars.Where(c => c == ' ');
            var hyphen = chars.Where(c => c == '-');
            return upper.Concat(lower).Concat(digits).Concat(special).Concat(space).Concat(hyphen).ToArray();
        }

        public static string BuildFilterPattern(List<char[]> charClasses)
        {
            var parts = new List<string>();
            foreach (char[] charClass in charClasses)
            {
                if (charClass.Length == 1)
                {
                    parts.Add(charClass[0].ToString());
                }
                else
                {
                    parts.Add($"[{new string(charClass)}]");
                }
            }

            return "^" + string.Join("", parts);
        }

        public static bool MatchesAnyFilter(string message, List<Regex> filterRegexes)
            => filterRegexes.Any(r => r.IsMatch(message));

        public static void VerifyMessagesGetFiltered(List<Regex> filterRegexes, string[] messages)
        {
            foreach (string message in messages)
            {
                if (!MatchesAnyFilter(message, filterRegexes))
                    throw new Exception($"This message doesn't get filtered: {message}");
            }
        }

        public static string ExtractMessage(string logLine)
        {
            int closeBracket = logLine.IndexOf(']');
            return closeBracket < 0 || closeBracket + 1 >= logLine.Length ? null
                : logLine.Substring(closeBracket + 1);
        }

        public static void TestForFalsePositives(List<Regex> allRegexes, string[] allFilterMessages, string[] logFiles, List<string> resultsOutput)
        {
            Console.WriteLine("=== False Positive Test ===");
            resultsOutput.Add("");
            resultsOutput.Add("=== False Positive Test ===");

            int totalTested = 0;
            int totalFalsePositives = 0;

            foreach (string logFile in logFiles)
            {
                string[] logLines = File.ReadAllLines(logFile);
                int tested = 0;
                int falsePositives = 0;

                foreach (string line in logLines)
                {
                    string message = ExtractMessage(line);
                    if (message == null) continue;

                    tested++;
                    if (!MatchesAnyFilter(message, allRegexes)) continue;

                    bool isIntendedMatch = allFilterMessages.Any(filterMessage => message.StartsWith(filterMessage));

                    if (!isIntendedMatch)
                    {
                        falsePositives++;
                        string msg = $"FALSE POSITIVE ({Path.GetFileNameWithoutExtension(logFile)}): {message}";
                        Console.WriteLine(msg);
                        resultsOutput.Add(msg);
                    }
                }

                totalTested += tested;
                totalFalsePositives += falsePositives;
            }

            string summary = $"{totalTested} messages tested across {logFiles.Length} files, {totalFalsePositives} false positive(s) found.";
            Console.WriteLine();
            Console.WriteLine(summary);
            Console.WriteLine();
            resultsOutput.Add(summary);
        }

        public static void GenerateFilteredLogs(List<Regex> allRegexes, string[] logFiles, List<string> resultsOutput)
        {
            string outputDir = "FilteredSampleLogs";
            Directory.CreateDirectory(outputDir);

            int totalRemoved = 0;
            int totalLines = 0;

            resultsOutput.Add("");
            resultsOutput.Add("=== Filtered Logs ===");

            foreach (string logFile in logFiles)
            {
                string[] logLines = File.ReadAllLines(logFile);
                int removed = 0;
                var kept = new List<string>();

                foreach (string line in logLines)
                {
                    string message = ExtractMessage(line);
                    if (message == null)
                    {
                        kept.Add(line);
                        continue;
                    }

                    totalLines++;
                    if (MatchesAnyFilter(message, allRegexes))
                    {
                        removed++;
                    }
                    else
                    {
                        kept.Add(line);
                    }
                }

                int fileTotal = removed + kept.Count;
                double removedPercent = fileTotal > 0 ? 100.0 * removed / fileTotal : 0;
                string outputFile = Path.Combine(outputDir, Path.GetFileName(logFile));
                File.WriteAllLines(outputFile, kept);
                string removedSummary = $"{Path.GetFileName(logFile)}: {removed} of {fileTotal} removed ({removedPercent:F1}%), {kept.Count} kept.";
                Console.WriteLine(removedSummary);
                resultsOutput.Add(removedSummary);
                totalRemoved += removed;
            }

            double totalRemovedPercent = totalLines > 0 ? 100.0 * totalRemoved / totalLines : 0;
            string totalRemovedSummary = $"Total: {totalRemoved} of {totalLines} removed ({totalRemovedPercent:F1}%). Filtered logs written to {outputDir}/.";
            Console.WriteLine();
            Console.WriteLine(totalRemovedSummary);
            resultsOutput.Add("");
            resultsOutput.Add(totalRemovedSummary);
        }
    }
}
