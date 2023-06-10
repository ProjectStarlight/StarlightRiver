using StarlightRiver.Content.Items.Moonstone;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Herbology
{
	public class GreenhouseGlass : ModTile
	{
		public override string Texture => AssetDirectory.HerbologyTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSet(0, 13, SoundID.Shatter, new Color(156, 172, 177), ModContent.ItemType<GreenhouseGlassItem>(), false, false, "Greenhouse Glass");
			Main.tileBlockLight[Type] = false;
			//Main.tileLighted[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
		}

		//public override bool RightClick(int i, int j)
		//{
		//	Main.NewText("Manual Update");
		//	ModContent.GetModTile(Main.tile[i, j].TileType)?.RandomUpdate(i, j);
		//	return base.RightClick(i, j);
		//}

		public override void RandomUpdate(int i, int j)
		{
			for (int k = 0; k < 10; k++)//k = max range up, this checks the area above it
			{
				if (Main.tile[i, j - 1 - k].HasTile && Main.tileBlockLight[Main.tile[i, j - 1 - k].TileType])
				{
					break;//breaks if a light blocking block is found
				}
				else if (k == 9)//starts downward scan on last block checked
				{
					for (int m = 0; m < 10; m++)//k = max range down, if the area above it clear this looks for the first plant below it
					{
						if (Main.tileSolid[Main.tile[i, j + 1 + m].TileType] && Main.tile[i, j + 1 + m].HasTile && !Main.tileSolidTop[Main.tile[i, j + 1 + m].TileType])
						{
							break;//breaks if Solid is true, Active is true, and solidTop is false
						}
						else if (
							Main.tile[i, j + 1 + m].HasTile &&
							Main.tileFrameImportant[Main.tile[i, j + 1 + m].TileType] &&
							!Main.tileSolid[Main.tile[i, j + 1 + m].TileType])//chooses if frameimportant, non-solid, and active
						{
							ModContent.GetModTile(Main.tile[i, j + 1 + m].TileType)?.RandomUpdate(i, j + 1 + m);//runs randomUpdate on selected block
																												//TODO: this doesn't work on vanilla plants since they dont use randomUpdate, figure out a way to fix this or make a case for vanilla plants
							break;
						}
					}
				}
			}
		}
	}

	public class GreenhouseGlassItem : QuickTileItem
	{
		public GreenhouseGlassItem() : base("Greenhouse Glass", "Speeds up the growth of any plant below it\nNeeds a 10 blocks of clear area or transparent blocks above it", "GreenhouseGlass", 1, AssetDirectory.HerbologyTile) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe(4);
			recipe.AddIngredient(ItemID.Glass, 4);
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<GreenhouseWallItem>(), 4);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}

	public class GreenhouseWall : ModWall
	{
		public override string Texture => AssetDirectory.HerbologyTile + Name;

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = true;
			RegisterItemDrop(ModContent.ItemType<GreenhouseWallItem>());
		}
	}

	public class GreenhouseWallItem : QuickWallItem
	{
		public GreenhouseWallItem() : base("Greenhouse Glass Wall", "Fancy!", ModContent.WallType<GreenhouseWall>(), 0, AssetDirectory.HerbologyTile) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe(4);//1 tile to 4 wall
			recipe.AddIngredient(ModContent.ItemType<GreenhouseGlassItem>(), 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}