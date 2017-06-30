using System;
using System.IO;
using System.Linq;

namespace AODamageMeter.AmbiguityHelper
{
    public class Program
    {
        public static void Main()
        {
            // A player name is ambiguous when we know an NPC has it, and we know a player *could* have it (no dashes, spaces, and so on). When
            // you find a name that isn't included yet, add it to AmbiguousPlayerNames.txt and use this program to generate well-formatted output.
            string[] ambiguousPlayerNames = File.ReadAllLines("AmbiguousPlayerNames.txt") // Can be on same line.
                .SelectMany(l => l.Split()) 
                .Select(n => n.Trim())
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => $"{char.ToUpperInvariant(n[0])}{n.Substring(1).ToLowerInvariant()}")
                .Where(Character.FitsPlayerNamingRequirements)
                .Distinct()
                .OrderBy(n => n)
                .ToArray();

            foreach (string name in ambiguousPlayerNames)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine($"        protected static readonly HashSet<string> _ambiguousPlayerNames = new HashSet<string> {{ {string.Join(", ", ambiguousPlayerNames.Select(n => $"\"{n}\""))} }};");
            Console.WriteLine();

            // A pet name is ambiguous when we know an NPC has it, and we know a conventional pet *could* have it (has a player name followed by "'s ").
            // When you find a name that isn't included yet, add it to AmbiguousPetNames.txt and use this program to generate well-formatted output.
            string[] ambiguousPetNames = File.ReadAllLines("AmbiguousPetNames.txt") // Must be on different lines.
                .Select(n => n.Trim()) 
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => string.Join(" ", n
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => $"{char.ToUpperInvariant(p[0])}{p.Substring(1).ToLowerInvariant()}")))
                .Where(Character.FitsPetNamingConventions)
                .Distinct()
                .OrderBy(n => n)
                .ToArray();

            foreach (string name in ambiguousPetNames)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine($"        protected static readonly HashSet<string> _ambiguousPetNames = new HashSet<string> {{ {string.Join(", ", ambiguousPetNames.Select(n => $"\"{n}\""))} }};");
        }
    }
}
