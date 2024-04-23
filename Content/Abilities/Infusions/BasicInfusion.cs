using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Tiles.Vitric.Temple;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Abilities.Infusions
{
	class BasicInfusion : InfusionItem
	{
		public override InfusionTier Tier => InfusionTier.Bronze;

		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DefaultFrame";

		public override bool Equippable => false;

		public override Type AbilityType => null;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blank Slate");
			Tooltip.SetDefault("Used to create infusions");
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 14;
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient<StaminaGel>(1);
			recipe.AddTile(ModContent.TileType<MainForge>());
			recipe.Register();
		}
	}
}