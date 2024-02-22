using StarlightRiver.Content.Tiles.Dungeon;
using System.Linq;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{

		private readonly int[] dungeonBricks = new int[] { TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick };

		private void TwistSwordGen(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Mounting weaponry";

			int tries = 0;

			for (int i = 0; i < 4; i++)
			{
				if (!PlaceTwistSword(WorldGen.genRand.Next(20, Main.maxTilesX - 20), WorldGen.genRand.Next((int)Main.rockLayer, Main.maxTilesY - 20)))
				{
					tries++;
					i--;
					if (tries > 9990)
						break;
				}
			}
		}

		private bool PlaceTwistSword(int i, int j)
		{
			Tile root = Main.tile[i, j];
			if (!root.HasTile || !dungeonBricks.Contains(root.TileType))
				return false;

			for (int k = 1; k < 6; k++)
			{
				Tile checkTile = Main.tile[i, j + k];
				if (checkTile.HasTile || !Main.wallDungeon[checkTile.WallType])
					return false;
			}

			Tile tile = Main.tile[i, j + 1];
			tile.ClearEverything();
			tile.HasTile = true;
			tile.TileType = (ushort)ModContent.TileType<TwistSwordTile>();
			return true;
		}
	}
}