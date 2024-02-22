using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Abilities
{
	public class ShardSet
	{
		private readonly HashSet<int> collected = new();

		public bool Has(int id)
		{
			return collected.Contains(id);
		}

		public void Add(int id)
		{
			collected.Add(id);
		}

		public int Count => collected.Count;

		public List<int> ToList()
		{
			return collected.ToList();
		}
	}
}