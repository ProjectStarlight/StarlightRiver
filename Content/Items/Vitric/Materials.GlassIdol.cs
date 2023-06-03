using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	class GlassIdol : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Used to worship a powerful guardian.");
		}

		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 20;
			Item.width = 32;
			Item.height = 32;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<SandstoneChunk>(5);
			recipe.AddIngredient<VitricOre>(5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	class GlassIdolEndless : ModItem
	{
		// TODO: CRAFTED AT FORGE (no clue if forge crafted is implemented yet)
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gilded Glass Idol");
			Tooltip.SetDefault("Used to worship a powerful guardian.\nInfinite uses");
		}

		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
			Item.width = 32;
			Item.height = 32;
		}
		// Placeholder recipe
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<GlassIdol>();
			recipe.AddIngredient<MagmaCore>();
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}