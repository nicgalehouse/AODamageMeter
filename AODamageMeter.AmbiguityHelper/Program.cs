using System;
using System.IO;
using System.Linq;

namespace AODamageMeter.AmbiguityHelper
{
    public class Program
    {
        public static void Main()
        {
            // A name is ambiguous when we know an NPC has it, and we know a player *could* have it (no dashes, spaces, and so on). When
            // you find a name that isn't included yet, add it to AmbiguousNames.txt and use this program to generate well-formatted output.
            string[] ambiguousNames = File.ReadAllLines("AmbiguousNames.txt")
                .SelectMany(l => l.Split())
                .Select(n => n.Trim())
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => $"{char.ToUpper(n[0])}{n.Substring(1).ToLower()}")
                .Where(Character.FitsPlayerNamingRequirements)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .ToArray();

            foreach (string name in ambiguousNames)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine($"        protected static readonly HashSet<string> _ambiguousNames = new HashSet<string> {{ {string.Join(", ", ambiguousNames.Select(n => $"\"{n}\""))} }};");
        }
    }
}
