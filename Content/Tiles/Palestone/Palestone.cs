using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Palestone
{
	internal class PalestoneItem : QuickTileItem
	{
		public PalestoneItem() : base("Palestone Block", "", "Palestone", 0, AssetDirectory.PalestoneTile) { }
	}

	internal class PalestoneRecipes : ModSystem
	{
		public override void AddRecipes()
		{
			Recipe.Create(ItemID.Furnace)
				.AddIngredient(ItemType<PalestoneItem>(), 20)
				.AddRecipeGroup(RecipeGroupID.Wood, 4)
				.AddIngredient(ItemID.Torch, 3)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.WoodenArrow, 25)
				.AddRecipeGroup(RecipeGroupID.Wood)
				.AddIngredient(ItemType<PalestoneItem>())
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	internal class Palestone : ModTile
	{
		public override string Texture => AssetDirectory.PalestoneTile + Name;

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileStone[Type] = true;
			HitSound = Terraria.ID.SoundID.Tink;

			DustType = Terraria.ID.DustID.Stone;
			RegisterItemDrop(ItemType<PalestoneItem>());

			AddMapEntry(new Color(167, 180, 191));
		}
	}
}