using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Attack;
using AODamageMeter.FightEvents.Heal;
using AODamageMeter.FightEvents.Level;
using AODamageMeter.FightEvents.Nano;
using AODamageMeter.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public static async Task<FightEvent> Create(Fight fight, string line)
        {
            int lastIndexOfArrayPart = line.IndexOf(']');
            string[] arrayPart = line.Substring(1, lastIndexOfArrayPart - 1).Split(',')
                .Select(p => p.Trim('"'))
                .ToArray();
            string eventName = arrayPart[1];
            DateTime timestamp = DateTimeHelper.DateTimeLocalFromUnixSeconds(long.Parse(arrayPart[3]));
            string description = line.Substring(lastIndexOfArrayPart + 1);

            if (eventName == MeCastNano.EventName) return MeCastNano.Create(fight, timestamp, description);
            if (eventName == MeGotHealth.EventName) return await MeGotHealth.Create(fight, timestamp, description);
            if (eventName == MeGotNano.EventName) return await MeGotNano.Create(fight, timestamp, description);
            if (eventName == MeGotSK.EventName) return MeGotSK.Create(fight, timestamp, description);
            if (eventName == MeGotXP.EventName) return MeGotXP.Create(fight, timestamp, description);
            if (eventName == MeHitByEnvironment.EventName) return MeHitByEnvironment.Create(fight, timestamp, description);
            if (eventName == MeHitByMonster.EventName) return await MeHitByMonster.Create(fight, timestamp, description);
            if (eventName == MeHitByNano.EventName) return await MeHitByNano.Create(fight, timestamp, description);
            if (eventName == MeHitByPlayer.EventName) return await MeHitByPlayer.Create(fight, timestamp, description);
            if (eventName == OtherHitByNano.EventName) return await OtherHitByNano.Create(fight, timestamp, description);
            if (eventName == OtherHitByOther.EventName) return await OtherHitByOther.Create(fight, timestamp, description);
            if (eventName == OtherMisses.EventName) return await OtherMisses.Create(fight, timestamp, description);
            if (eventName == Research.EventName) return Research.Create(fight, timestamp, description);
            if (eventName == SystemEvent.EventName) return SystemEvent.Create(fight, timestamp, description);
            if (eventName == YouGaveHealth.EventName) return await YouGaveHealth.Create(fight, timestamp, description);
            if (eventName == YouGaveNano.EventName) return await YouGaveNano.Create(fight, timestamp, description);
            if (eventName == YouHitOther.EventName) return await YouHitOther.Create(fight, timestamp, description);
            if (eventName == YouHitOtherWithNano.EventName) return await YouHitOtherWithNano.Create(fight, timestamp, description);
            if (eventName == YourMisses.EventName) return await YourMisses.Create(fight, timestamp, description);
            if (eventName == YourPetHitByMonster.EventName) return await YourPetHitByMonster.Create(fight, timestamp, description);
            if (eventName == YourPetHitByNano.EventName) return await YourPetHitByNano.Create(fight, timestamp, description);
            if (eventName == YourPetHitByOther.EventName) return await YourPetHitByOther.Create(fight, timestamp, description);
            throw new NotSupportedException($"{eventName}: {description}");
        }

        public DamageMeter DamageMeter => Fight.DamageMeter;
        public Fight Fight { get; }
        public abstract string Name { get; }
        public DateTime Timestamp { get; }
        public string Description { get; }
        public bool Unmatched { get; protected set; }
        public FightCharacter Source { get; protected set; }
        public FightCharacter Target { get; protected set; }
        public int? Amount { get; protected set; }

        protected static Regex CreateRegex(string body, bool rightToLeft = false)
            => new Regex($"^{body}$", rightToLeft ? RegexOptions.Compiled | RegexOptions.RightToLeft : RegexOptions.Compiled);

        protected bool TryMatch(Regex regex, out Match match)
            => (match = regex.Match(Description)).Success;

        protected bool TryMatch(Regex regex, out Match match, out bool success)
        {
            match = regex.Match(Description);
            success = match.Success;
            return success;
        }

        /* The three pet events aren't super useful. "Your pet hit by monster" almost never happens, everything you'd expect
           in there actually happens in "Your pet hit by other". "Your pet hit by nano" also doesn't happen much. Neither of
           these can really be relied on to deduce pet names, because they happen so infrequently or would never happen in
           common DD-testing areas like the duel room. So that leaves "Your pet hit by other", which actually includes your
           pet hit by other and other hit by your pet. But it doesn't include your pet's shields/reflects doing damage--those
           get included in "Other hit by other". We could try to do better, but for now we're just gonna go with supporting a
           convention. We don't pay special attention to the pet events, but just apply this reasoning across all events:

           If we see a source/target with exactly the same name as the owner, it gets attributed to a pet character named
           "<Owner>'s pets". This works because the owner's name never appears in the log for their own events--there's just
           the contextual "You". If we see a source/target with a name prefixed by {an existent PC's name and a space}, it gets
           created as a pet character. In both cases, the corresponding PC has the pet character added to their pets. For
           example, we support an aggregate pet breakdown if the owner is "Elitengi" and he names his pets "Elitengi", and
           an individual pet breakdown if the PC is "Elitengi" and he names his pets "Elitengi Robo" and "Elitengi Doggo". The
           aggregate pet breakdown only works for the owner. For non-owner PCs, all we can do is add the damage to the PC. The
           individual pet breakdown works for all PCs, owner and non-owner alike. Natural false positives--NPCs which {a PC's
           name followed by a space} prefix--probably never happen. Ones caused by other characters naming their pets after a
           different character might happen. Maybe the biggest concern here is when someone renames their pets on one character
           to match their damage-dealing alt, and we'll never be able to solve that, so don't worry about this problem at all.
           We could see when someone has renamed their pets after the meter owner, and have special handling if we know the
           meter owner can't have pets, but nah. So for now, everyone can have pets regardless of their profession. */

        protected async Task SetSource(Match match, int index)
        {
            string name = match.Groups[index].Value;

            if (name == DamageMeter.Owner.Name)
            {
                Source = await Fight.GetOrCreateFightCharacter($"{name}'s pets");
                Source.Character.CharacterType = CharacterType.Pet;
                FightCharacter petOwner = Fight.GetOrCreateFightCharacter(DamageMeter.Owner);
                petOwner.RegisterPet(Source);
                return;
            }

            var nameParts = name.Split();
            if (nameParts.Length > 1
                && Character.TryGetCharacter(nameParts[0], out Character character)
                && character.CharacterType == CharacterType.PlayerCharacter)
            {
                Source = await Fight.GetOrCreateFightCharacter(name);
                Source.Character.CharacterType = CharacterType.Pet;
                FightCharacter petOwner = Fight.GetOrCreateFightCharacter(character);
                petOwner.RegisterPet(Source);
                return;
            }

            Source = await Fight.GetOrCreateFightCharacter(name);
        }

        protected async Task SetTarget(Match match, int index)
        {
            string name = match.Groups[index].Value;

            if (name == DamageMeter.Owner.Name)
            {
                Target = await Fight.GetOrCreateFightCharacter($"{name}'s pets");
                Target.Character.CharacterType = CharacterType.Pet;
                FightCharacter petOwner = Fight.GetOrCreateFightCharacter(DamageMeter.Owner);
                petOwner.RegisterPet(Target);
                return;
            }

            var nameParts = name.Split();
            if (nameParts.Length > 1
                && Character.TryGetCharacter(nameParts[0], out Character character)
                && character.CharacterType == CharacterType.PlayerCharacter)
            {
                Target = await Fight.GetOrCreateFightCharacter(name);
                Target.Character.CharacterType = CharacterType.Pet;
                FightCharacter petOwner = Fight.GetOrCreateFightCharacter(character);
                petOwner.RegisterPet(Target);
                return;
            }

            Target = await Fight.GetOrCreateFightCharacter(name);
        }

        protected Task SetSourceAndTarget(Match match, int sourceIndex, int targetIndex)
            => Task.WhenAll(SetSource(match, sourceIndex), SetTarget(match, targetIndex));

        protected void SetSourceToOwner()
            => Source = Fight.GetOrCreateFightCharacter(DamageMeter.Owner);

        protected void SetTargetToOwner()
            => Target = Fight.GetOrCreateFightCharacter(DamageMeter.Owner);

        protected void SetSourceAndTargetToOwner()
            => Source = Target = Fight.GetOrCreateFightCharacter(DamageMeter.Owner);

        protected void SetAmount(Match match, int index)
            => Amount = int.Parse(match.Groups[index].Value);
    }
}
