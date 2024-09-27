using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Prefixes.Accessory.Cursed
{
	internal static class CursedPrefixPool
	{
		public static List<int> GetCursedPrefixes()
		{
			return new List<int>
			{
				ModContent.PrefixType<Carnal>(),
				ModContent.PrefixType<Crystalline>(),
				ModContent.PrefixType<Eldritch>(),
				ModContent.PrefixType<Ephemeral>(),
				ModContent.PrefixType<Occult>(),
				ModContent.PrefixType<Reckless>(),
				ModContent.PrefixType<Sapping>(),
				ModContent.PrefixType<Unfaithful>()
			};
		}
	}
}