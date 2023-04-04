using Terraria.DataStructures;
using Terraria.Enums;
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
			TileObjectData.newTile.DrawYOffset = 2;

			var anchor = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, 4, 0);
			QuickBlock.QuickSetFurniture(this, 4, 4, DustID.Sand, SoundID.Tink, new Color(200, 160, 50), bottomAnchor: anchor, anchorTiles: new int[] { Type });
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
					FrameMonolith(Main.rand.Next(3), 0, i - 2, j - 2);
					break;
				}
				else
				{
					int thisFrame = Main.rand.Next(4); //We do all this to prevent >1 repeat of the same frame

					short frameDown1 = Framing.GetTileSafely(i - 2, j + 2).TileFrameX;
					short frameDown2 = Framing.GetTileSafely(i - 2, j + 6).TileFrameX;

					while (frameDown1 == frameDown2 && frameDown1 == (short)(thisFrame * 18))
						thisFrame = Main.rand.Next(4);

					FrameMonolith(thisFrame, 1, i - 2, j - 2);
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

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Tile tile = Framing.GetTileSafely(i, j + 4);

			if (tile.TileType == Type)
				FrameMonolith(Main.rand.Next(2), 2, i, j + 4);
		}

		public void FrameMonolith(int x, int y, int i, int j)
		{
			for (int x1 = 0; x1 < 4; x1++)
			{
				for (int y1 = 0; y1 < 4; y1++)
				{
					Tile tile = Framing.GetTileSafely(i + x1, j + y1);

					if (tile.TileType != Type)
						return;

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

	internal class DesertMonolithFlipped : DesertMonolith
	{
		//huh, not much to do here, huh?
	}

	internal class DesertMonolithFlippedItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.DesertTile + Name;

		public DesertMonolithFlippedItem() : base("Flipped Desert Monolith", "Places a section of flipped desert monolith", "DesertMonolithFlipped") { }
	}
}
