using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		private void DungeonGen(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Caging starlight...";

			// Place the shard cage
			Vector2 pos = new Vector2(Main.dungeonX, Main.dungeonY);

			int direction = Main.dungeonX > Main.maxTilesX / 2 ? 1 : -1;
			pos.Y -= 16;
			pos.X += direction * 38 - 2;

			StructureHelper.API.Generator.GenerateStructure("Structures/DungeonShardCage", pos.ToPoint16(), Mod);

			// Generate the chain
			pos.X += 2;
			pos.Y -= 1;

			while (true)
			{
				Tile tile = Framing.GetTileSafely(pos.ToPoint16());

				if (tile.HasTile && Main.tileSolid[tile.TileType])
					break;

				WorldGen.PlaceTile((int)pos.X, (int)pos.Y, TileID.Chain);
				pos.Y -= 1;
			}
		}
	}
}