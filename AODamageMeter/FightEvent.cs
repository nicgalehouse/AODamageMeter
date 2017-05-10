using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Attack;
using AODamageMeter.FightEvents.Heal;
using AODamageMeter.FightEvents.Level;
using AODamageMeter.FightEvents.Nano;
using AODamageMeter.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AODamageMeter
{
    public abstract class FightEvent
    {
        protected const string SOURCE = "(.+?)", TARGET = "(.+?)", AMOUNT = @"(\d+)";

        protected FightEvent(Fight fight, DateTime timestamp, string description)
        {
            Fight = fight;
            Timestamp = timestamp;
            Description = description;
        }

        public static FightEvent Create(Fight fight, string line)
        {
            // Lines look like this for example, the array part always having four elements:
            // ["#000000004200000a#","Other hit by other","",1492309026]Elitengi hit Leet for 6 points of cold damage.
            int lastIndexOfArrayPart = line.IndexOf(']');
            string[] arrayPart = line.Substring(1, lastIndexOfArrayPart - 1).Split(',')
                .Select(p => p.Trim('"'))
                .ToArray();
            string eventName = arrayPart[1];
            DateTime timestamp = fight.DamageMeter.IsRealTimeMode ? DateTime.Now
                : DateTimeHelper.DateTimeLocalFromUnixSeconds(long.Parse(arrayPart[3]));
            string description = line.Substring(startIndex: lastIndexOfArrayPart + 1);

            switch (eventName)
            {
                case MeCastNano.EventName: return new MeCastNano(fight, timestamp, description);
                case MeGotHealth.EventName: return new MeGotHealth(fight, timestamp, description);
                case MeGotNano.EventName: return new MeGotNano(fight, timestamp, description);
                case MeGotSK.EventName: return new MeGotSK(fight, timestamp, description);
                case MeGotXP.EventName: return new MeGotXP(fight, timestamp, description);
                case MeHitByEnvironment.EventName: return new MeHitByEnvironment(fight, timestamp, description);
                case MeHitByMonster.EventName: return new MeHitByMonster(fight, timestamp, description);
                case MeHitByNano.EventName: return new MeHitByNano(fight, timestamp, description);
                case MeHitByPlayer.EventName: return new MeHitByPlayer(fight, timestamp, description);
                case OtherHitByNano.EventName: return new OtherHitByNano(fight, timestamp, description);
                case OtherHitByOther.EventName: return new OtherHitByOther(fight, timestamp, description);
                case OtherMisses.EventName: return new OtherMisses(fight, timestamp, description);
                case Research.EventName: return new Research(fight, timestamp, description);
                case SystemEvent.EventName: return new SystemEvent(fight, timestamp, description);
                case YouGaveHealth.EventName: return new YouGaveHealth(fight, timestamp, description);
                case YouGaveNano.EventName: return new YouGaveNano(fight, timestamp, description);
                case YouHitOther.EventName: return new YouHitOther(fight, timestamp, description);
                case YouHitOtherWithNano.EventName: return new YouHitOtherWithNano(fight, timestamp, description);
                case YourMisses.EventName: return new YourMisses(fight, timestamp, description);
                case YourPetHitByMonster.EventName: return new YourPetHitByMonster(fight, timestamp, description);
                case YourPetHitByNano.EventName: return new YourPetHitByNano(fight, timestamp, description);
                case YourPetHitByOther.EventName: return new YourPetHitByOther(fight, timestamp, description);
                default: return new UnrecognizedEvent(fight, timestamp, description);
            }
        }

        public DamageMeter DamageMeter => Fight.DamageMeter;
        public Fight Fight { get; }
        public abstract string Name { get; }
        public DateTime Timestamp { get; }
        public string Description { get; }
        public bool IsUnmatched { get; protected set; }
        public FightCharacter Source { get; protected set; }
        public FightCharacter Target { get; protected set; }
        public int? Amount { get; protected set; }

        protected static Regex CreateRegex(string body, bool rightToLeft = false)
            => new Regex($"^{body}$", rightToLeft ? RegexOptions.Compiled | RegexOptions.RightToLeft : RegexOptions.Compiled);

        protected bool TryMatch(Regex regex, out Match match)
            => (match = regex.Match(Description)).Success;

        protected bool TryMatch(Regex regex, out Match match, out bool success)
            => success = (match = regex.Match(Description)).Success;

        /* The three pet events aren't super useful. "Your pet hit by monster" almost never happens, everything you'd expect
           in there actually happens in "Your pet hit by other". "Your pet hit by nano" also doesn't happen much. Neither of
           these can really be relied on to deduce pet names, because they happen so infrequently or would never happen in
           common DD-testing areas like the duel room. So that leaves "Your pet hit by other", which actually includes your
           pet hit by other and other hit by your pet. But it doesn't include your pet's shields/reflects doing damage--those
           get included in "Other hit by other". We could try to do better, but for now we're just gonna go with supporting a
           convention. We don't pay special attention to the pet events, but just apply this reasoning across all events:

           If we see a source/target with exactly the same name as the owner, it gets attributed to a pet character named
           "<Owner>'s pets". This works because the owner's name never appears in the log for their own events--there's
           just the contextual "You". If we see a source/target with a name prefixed by {an existent character's name and
           an apostrophe}, it gets registered as a pet character. In both cases, the corresponding character has the pet
           character added to their pets. For example, we support an aggregate pet breakdown if the owner is "Elitengi"
           and he names his pets "Elitengi", and an individual pet breakdown if the PC is "Elitengi" and he names his
           pets "Elitengi's Robo" and "Elitengi's Doggo". The aggregate pet breakdown only works for the owner. For
           non-owner characters, all we can do is add the damage to the character. The individual pet breakdown works for
           all characters, owner and non-owner alike. We don't really expect any false positives here because the apostrophe
           is an uncommonly used character across NPC and pet names right now. Biggest false positive is when a PC names
           their pet the same as some other PC, and we can't do anything about that. We don't bother checking if the pet
           owner is a PC, because that would complicate debugging/testing static log files, when the knowledge of a character
           being a PC comes in eventually from people.anarchy-online.com, well after all the parsing has happened. */

        protected void SetSource(Match match, int index)
        {
            string name = Character.RemoveMarkupCharacters(match.Groups[index].Value);
            if (name == DamageMeter.Owner.Name)
            {
                Source = Fight.GetOrCreateFightCharacter($"{name}'s pets", Timestamp);
                if (Source.FightPetOwner == null) // If Source isn't registered as a fight pet yet, register it.
                {
                    Fight.GetOrCreateFightCharacter(DamageMeter.Owner, Timestamp)
                        .RegisterFightPet(Source);
                }
            }
            else
            {
                Source = Fight.GetOrCreateFightCharacter(name, Timestamp);
                if (Source.FightPetOwner == null // If Source isn't registered as a fight pet yet, try to register it.
                    && name.Contains('\'')
                    && Character.TryGetCharacter(name.Split('\'')[0], out Character petOwner))
                {
                    Fight.GetOrCreateFightCharacter(petOwner, Timestamp)
                        .RegisterFightPet(Source);
                }
            }
        }

        protected void SetTarget(Match match, int index)
        {
            string name = Character.RemoveMarkupCharacters(match.Groups[index].Value);
            if (name == DamageMeter.Owner.Name)
            {
                Target = Fight.GetOrCreateFightCharacter($"{name}'s pets", Timestamp);
                if (Target.FightPetOwner == null) // If Target isn't registered as a fight pet yet, register it.
                {
                    Fight.GetOrCreateFightCharacter(DamageMeter.Owner, Timestamp)
                        .RegisterFightPet(Target);
                }
            }
            else
            {
                Target = Fight.GetOrCreateFightCharacter(name, Timestamp);
                if (Target.FightPetOwner == null // If Target isn't registered as a fight pet yet, try to register it.
                    && name.Contains('\'')
                    && Character.TryGetCharacter(name.Split('\'')[0], out Character petOwner))
                {
                    Fight.GetOrCreateFightCharacter(petOwner, Timestamp)
                        .RegisterFightPet(Target);
                }
            }
        }

        protected void SetSourceAndTarget(Match match, int sourceIndex, int targetIndex)
        {
            SetSource(match, sourceIndex);
            SetTarget(match, targetIndex);
        }

        protected void SetSourceToOwner()
            => Source = Fight.GetOrCreateFightCharacter(DamageMeter.Owner, Timestamp);

        protected void SetTargetToOwner()
            => Target = Fight.GetOrCreateFightCharacter(DamageMeter.Owner, Timestamp);

        protected void SetSourceAndTargetToOwner()
            => Source = Target = Fight.GetOrCreateFightCharacter(DamageMeter.Owner, Timestamp);

        protected void SetAmount(Match match, int index)
            => Amount = int.Parse(match.Groups[index].Value);

        public override string ToString()
            => $"{Name}: {Description}";
    }
}
