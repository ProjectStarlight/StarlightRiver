using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class Tooth : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/Tooth";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Bone, SoundID.NPCHit10, new Color(60, 40, 20));
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (Framing.GetTileSafely(i - 1, j).IsSquareSolidTile())
				tile.TileFrameY = 18;
			else if (Framing.GetTileSafely(i, j + 1).IsSquareSolidTile())
				tile.TileFrameY = 0;
			else if (Framing.GetTileSafely(i + 1, j).IsSquareSolidTile())
				tile.TileFrameY = 54;
			else if (Framing.GetTileSafely(i, j - 1).IsSquareSolidTile())
				tile.TileFrameY = 36;
			else
				WorldGen.KillTile(i, j);

			tile.TileFrameX = (short)(Main.rand.Next(6) * 18);

			return false;
		}
	}
}