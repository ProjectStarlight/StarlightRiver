using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld
	{
		public static readonly List<int> InvalidShrineTiles = new()
		{
			TileID.Sand,
			TileID.Ebonstone,
			TileID.Crimstone,
			TileID.IceBlock,
			TileID.SnowBlock,
			TileID.Sandstone,
			TileID.JungleGrass,
			TileID.Mud,
			TileID.MushroomGrass,
			TileID.Marble,
			TileID.Granite,
		};

		public static void ShrineGen(GenerationProgress progress, GameConfiguration configuration)
		{
			ShrinePass("Structures/CombatShrine");
			ShrinePass("Structures/EvasionShrine");
		}

		public static void ShrinePass(string structure)
		{
			for (int x = 350; x < Main.maxTilesX - 350; x += WorldGen.genRand.Next(100, 350))
			{
				for (int retry = 0; retry < 40; retry++)
				{
					int y = WorldGen.genRand.Next((int)Main.worldSurface + 100, Main.UnderworldLayer - 100);

					Point16 dims = new();
					StructureHelper.Generator.GetDimensions(structure, StarlightRiver.Instance, ref dims);

					if (WorldGen.InWorld(x, y) && WorldGenHelper.IsRectangleSafe(new Rectangle(x, y, dims.X, dims.Y), ShrineCondition))
					{
						StructureHelper.Generator.GenerateStructure(structure, new Point16(x, y), StarlightRiver.Instance);
						break;
					}
				}
			}
		}

		public static bool ShrineCondition(Tile a, Point16 b)
		{
			//we dont want shrines on shrines...
			if (a.TileType == StarlightRiver.Instance.Find<ModTile>("TempleBrick").Type)
				return false;

			if (InvalidShrineTiles.Contains(a.TileType))
				return false;

			if (vitricBiome.Contains(b.ToPoint()))
				return false;

			return true;
		}
	}
}