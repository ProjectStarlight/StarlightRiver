using StarlightRiver.Content.CustomHooks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Utility
{
	class MeteorForcer : ModItem
	{
		public override string Texture => "StarlightRiver/Assets/Items/Utility/" + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Prayer for Meteorite");
			Tooltip.SetDefault("Guarantees a meteorite will be the next celestial object to fall");
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
			ModContent.GetInstance<AstralMeteor>().meteorForced = true;
			ModContent.GetInstance<AstralMeteor>().moonstoneForced = false;

			Main.NewText("A meteor is bound to fall next...", new Color(215, 120, 50));

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