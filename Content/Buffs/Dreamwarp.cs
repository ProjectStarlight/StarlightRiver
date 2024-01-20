using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Buffs
{
	class Dreamwarp : SmartBuff
	{
		public Dreamwarp() : base("Dreamwarp", "Cuckoo for cocao puffs", true) { }

		public override string Texture => AssetDirectory.Buffs + "Dreamwarp";
	}
}