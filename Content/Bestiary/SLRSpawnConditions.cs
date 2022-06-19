using Microsoft.Xna.Framework;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Bestiary
{
    public static class SLRSpawnConditions 
    {
		public static ModBiomeSpawnCondition VitricDesert = new ModBiomeSpawnCondition("Vitric Desert", AssetDirectory.Biomes + "VitricDesertIcon", AssetDirectory.MapBackgrounds + "GlassMap", Color.White);

		public static ModBiomeSpawnCondition AuroraSquid = new ModBiomeSpawnCondition("Aurora Temples", AssetDirectory.Biomes + "AuroraIcon", AssetDirectory.Biomes + "AuroraBG", Color.White);

		public static void Unload()
		{
			VitricDesert = null;
		}
	}
}
