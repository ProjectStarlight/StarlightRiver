using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Desert
{
	internal class DesertMonolith : ModTile
	{
		public override string Texture => AssetDirectory.DesertTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.StyleHorizontal = true;
			QuickBlock.QuickSetFurniture(this, 4, 4, DustID.Sand, SoundID.Tink, new Color(200, 160, 50));
		}

		public override void PlaceInWorld(int i, int j, Item item)
		{
			int oldJ = j;
			FrameMonolith(Main.rand.Next(2), 2, i - 2, j - 2);

			j += 4;

			while (true)
			{
				Tile tile = Framing.GetTileSafely(i, j + 2);

				if (tile.TileType != Type)
				{
					FrameMonolith(Main.rand.Next(2), 0, i - 2, j - 2);
					break;
				}
				else
				{
					FrameMonolith(Main.rand.Next(5), 1, i - 2, j - 2);
				}

				j += 4;
			}

			j = oldJ;

			Tile tile2 = Framing.GetTileSafely(i, j + +4 * 6 + 2);

			if (tile2.TileType == Type)
			{
				for (int k = 0; k < 3; k++)
				{
					FrameMonolith(2 - k, 3, i - 2, j - 2 + 4 * k);
				}
			}
		}

		public void FrameMonolith(int x, int y, int i, int j)
		{
			for (int x1 = 0; x1 < 4; x1++)
			{
				for (int y1 = 0; y1 < 4; y1++)
				{
					Tile tile = Framing.GetTileSafely(i + x1, j + y1);
					tile.TileFrameX = (short)(4 * 18 * x + x1 * 18);
					tile.TileFrameY = (short)(4 * 18 * y + y1 * 18);
				}
			}
		}
	}

	internal class DesertMonolithItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.DesertTile + Name;

		public DesertMonolithItem() : base("Desert Monolith", "Places a section of desert monolith", "DesertMonolith") { }
	}
}
