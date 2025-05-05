using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Biomes
{
	internal class GestaltCellArena : ModBiome
	{
	}

	internal class GestaltCellArenaSystem : ModSystem
	{
		public static Rectangle arena;

		public static Rectangle ArenaWorld => new(arena.X * 16, arena.Y * 16, arena.Width * 16, arena.Height * 16);

		public override void SaveWorldData(TagCompound tag)
		{
			tag["arena"] = arena;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			arena = tag.Get<Rectangle>("arena");
		}
	}
}
