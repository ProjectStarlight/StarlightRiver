using StarlightRiver.Content.CustomHooks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Utility
{
	class MoonstoneForcer : ModItem
	{
		public override string Texture => "StarlightRiver/Assets/Items/Utility/" + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Prayer for Moonstone");
			Tooltip.SetDefault("Guarantees a moonstone will be the next celestial object to fall");
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 30;
			Item.useStyle = Terraria.ID.ItemUseStyleID.HoldUp;
			Item.consumable = true;
			Item.UseSound = Terraria.ID.SoundID.Item2;
		}

		public override bool? UseItem(Player player)
		{
			ModContent.GetInstance<AstralMeteor>().meteorForced = false;
			ModContent.GetInstance<AstralMeteor>().moonstoneForced = true;

			Main.NewText("A moonstone is bound to fall next...", new Color(80, 150, 250));

			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.BambooBlock, 5);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}