using System;
using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter
{
    public class Nanoline
    {
        private readonly Dictionary<string, Nano> _nanosByName;

        public Nanoline(string name, params Nano[] nanos)
        {
            Name = name;
            Nanos = nanos;
            _nanosByName = nanos.ToDictionary(n => n.Name, n => n, StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }
        public IReadOnlyList<Nano> Nanos { get; }

        public bool HasNano(string name)
            => _nanosByName.ContainsKey(name);

        public bool TryGetNano(string name, out Nano nano)
            => _nanosByName.TryGetValue(name, out nano);
    }
}
