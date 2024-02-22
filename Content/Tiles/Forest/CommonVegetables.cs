using StarlightRiver.Content.Items.Food;
using StarlightRiver.Content.Packets;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Forest
{
	internal class CommonVegetables : ModTile
	{
		public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
			var anchor = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 1, 0);
			int[] valid = new int[] { TileID.Grass };

			QuickBlock.QuickSetFurniture(this, 1, 2, DustID.Grass, SoundID.Dig, false, new Color(200, 255, 220), false, false, "", anchor, default, valid);

			HitSound = SoundID.Grass;
		}

		public override void RandomUpdate(int i, int j) //RandomUpdate is vanilla's less-than-ideal way of handling having the entire world loaded at once. a bunch of tiles update every tick at pure random. thanks redcode.
		{
			Tile tile = Framing.GetTileSafely(i, j); //you could probably add more safety checks if you want to be extra giga secure, but we assume RandomUpdate only calls valid tiles here
			var data = TileObjectData.GetTileData(tile.TileType, TileObjectData.GetTileStyle(tile)); //grabs the TileObjectData associated with our tile. So we dont have to use as many magic numbers
			int fullFrameWidth = data.Width * (data.CoordinateWidth + data.CoordinatePadding); //the width of a full frame of our multitile in pixels. We get this by multiplying the size of 1 full frame with padding by the width of our tile in tiles.

			if (tile.TileFrameY % 36 == 0) //this checks to make sure this is only the top-left tile. We only want one tile to do all the growing for us, and top-left is the standard. otherwise each tile in the multitile ticks on its own due to stupid poopoo redcode.
			{
				if ((Main.rand.NextBool(2) || Main.dayTime) && (tile.TileFrameX == 0 || tile.TileFrameX == 18)) //a random check here can slow growing as much as you want.
				{
					for (int x = 0; x < data.Width; x++) //this for loop iterates through every COLUMN of the multitile, starting on the top-left.
					{
						for (int y = 0; y < data.Height; y++) //this for loop iterates through every ROW of the multitile, starting on the top-left.
						{
							//These 2 for loops together iterate through every specific tile in the multitile, allowing you to move each one's frame
							Tile targetTile = Main.tile[i + x, j + y]; //find the tile we are targeting by adding the offsets we find via the for loops to the coordinates of the top-left tile.
							targetTile.TileFrameX += (short)fullFrameWidth; //adds the width of the frame to that specific tile's frame. this should push it forward by one full frame of your multitile sprite. cast to short because vanilla.
						}
					}

					NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 2);
				}
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			if (frameX > 18)
			{
				if (Main.rand.NextBool(4))
					Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i * 16, j * 16), ItemType<Lettuce>(), Main.rand.Next(4) + 1);

				if (Main.rand.NextBool(4))
					Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i * 16, j * 16), ItemType<Cabbage>(), Main.rand.Next(4) + 1);

				if (Main.rand.NextBool(4))
					Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i * 16, j * 16), ItemType<Carrot>(), Main.rand.Next(4) + 1);

				if (Main.rand.NextBool(4))
					Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i * 16, j * 16), ItemType<BrusselSprouts>(), Main.rand.Next(4) + 1);

				if (Main.rand.NextBool(4))
					Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i * 16, j * 16), ItemType<CommonVegetablesItem>(), Main.rand.Next(2) + 1);
			}

			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i * 16, j * 16), ItemType<CommonVegetablesItem>(), 1);
		}
	}

	public class CommonVegetablesItem : QuickTileItem
	{
		public CommonVegetablesItem() : base("Common Vegetable Seeds", "Plant to grow your own veggies", "CommonVegetables", 1, AssetDirectory.ForestTile) { }
	}
}
