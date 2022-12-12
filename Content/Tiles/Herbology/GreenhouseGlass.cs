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

		public override void RandomUpdate(int i, int j)
		{

		}
	}

	public class GreenhouseGlassItem : QuickTileItem
	{
		public GreenhouseGlassItem() : base("Greenhouse Glass", "Speeds up the growth of any plant below it\nNeeds a clear area above it", "GreenhouseGlass", 1, AssetDirectory.HerbologyTile) { }

		public override void AddRecipes()
		{
			//Recipe recipe = CreateRecipe();
			//recipe.AddIngredient(ItemID.Glass, 10);
			//recipe.AddIngredient(ModContent.ItemType<AluminumBarItem>(), 1);
			//recipe.AddTile(TileID.WorkBenches);

			//recipe = CreateRecipe();
			//recipe.AddIngredient(ModContent.ItemType<GreenhouseWallItem>(), 4);
			//recipe.AddTile(TileID.WorkBenches);
		}
	}

	public class GreenhouseWall : ModWall
	{
		public override string Texture => AssetDirectory.HerbologyTile + Name;

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = true;
			ItemDrop = ModContent.ItemType<GreenhouseWallItem>();
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