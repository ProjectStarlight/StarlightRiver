using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class ThickTreeSapling : ModTile
	{
		public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleHorizontal = true;

			var bottomAnchor = new AnchorData(Terraria.Enums.AnchorType.SolidTile, 2, 0);
			this.QuickSetFurniture(2, 3, DustID.WoodFurniture, SoundID.Dig, new Color(200, 255, 230), 18, false, false, "Large sapling", bottomAnchor);
		}

		public override void RandomUpdate(int i, int j)
		{
			Tile tile = Main.tile[i, j];

			if (!(tile.TileFrameX % 36 == 0 && tile.TileFrameY == 0))
				return;

			//for (int p = -2; p <= 2; p++)//debug dust on update, stays until tile is fully functional
			//{
			//	Tile tile2 = Main.tile[i, j];
			//	int offsetX2 = tile2.TileFrameX / 18 % 2;
			//	int offsetY2 = tile2.TileFrameY / 18;
			//	Dust.NewDustPerfect(
			//		new Vector2(i - offsetX2 + 1, j - offsetY2) * 16 + new Vector2(p * 6, 6),
			//		DustID.PinkFairy,
			//		new Vector2(p * 0.05f, Main.rand.NextFloat(-0.7f, -0.25f)),
			//		0,
			//		Color.White, 1f);
			//}

			//vanilla tree seems to be about >5%?
			//at 3% after over an hour and a half 6/10 grew
			if (Main.rand.NextFloat(100) > 4f)//still needs testing but should be fast enough
				return;

			if (!StarlightWorld.IsGround(i - 1, j + 3, 4))
				return;

			int height = Main.rand.Next(20, 35);
			const int AbsMinHeight = 12;//lower than the min height normally

			for (int h = 0; h < height; h++)//finds clear area
			{
				if (!IsAir(i - 1, j + 2 - h, 4))
				{
					if(h > AbsMinHeight)//if above min height, just use current height
					{
						height = h - 4;//needs to account for the height of the base
						break;
					}
					else//if below min height, cancel
					{
						return;
					}
				}
			}

			//removes sapling
			for (int g = 0; g < 2; g++)
			{
				for (int h = 0; h < 3; h++)
					Main.tile[i + g, j + h].ClearTile();
			}

			StarlightWorld.PlaceTree(i, j + 3, height);
		}

		private bool IsAir(int x, int y, int w)//method from worldgen, but needs to skip sapling and platform
		{
			for (int k = 0; k < w; k++)
			{
				Tile tile = Framing.GetTileSafely(x + k, y);
				if (tile.HasTile && tile.TileType != Type && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])//this version allows the tree to break stuff the player can stand on but pass though (platforms, tool stations)
					return false;
			}

			return true;
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = 1;
		}

		//public override bool RightClick(int i, int j)//debug, needed for later
		//{
		//	RandomUpdate(i, j);
		//	return true;
		//}
	}

	[SLRDebug]
	class ThickTreeSaplingPlacer: QuickTileItem
	{
		public ThickTreeSaplingPlacer() : base("Large Sapling Placer", "", "ThickTreeSapling", 1, AssetDirectory.ForestTile, false) { }
	}
}