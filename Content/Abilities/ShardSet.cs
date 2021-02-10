using System.Collections.Generic;
using System.Linq;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Abilities
{
    public class ShardSet
    {
        private readonly HashSet<int> collected = new HashSet<int>();

        public bool Has(int id) => collected.Contains(id);
        public void Add(int id) => collected.Add(id);

        public int Count => collected.Count;

        public List<int> ToList() => collected.ToList();
    }
}
