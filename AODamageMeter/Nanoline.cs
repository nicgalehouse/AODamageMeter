using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter
{
    public class Nanoline
    {
        private readonly Dictionary<string, Buff> _buffsByName;

        public Nanoline(string name, params Buff[] buffs)
        {
            Name = name;
            Buffs = buffs;
            _buffsByName = buffs.ToDictionary(b => b.Name);
        }

        public string Name { get; }
        public IReadOnlyList<Buff> Buffs { get; }

        public bool TryGetBuff(string name, out Buff buff)
            => _buffsByName.TryGetValue(name, out buff);
    }
}
