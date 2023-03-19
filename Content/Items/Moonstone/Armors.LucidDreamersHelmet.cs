using Terraria.ID;
using Terraria;

namespace StarlightRiver.Content.Items.Moonstone
{
	[AutoloadEquip(EquipType.Head)]
	public class LucidDreamersHelmet : ModItem
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lucid Dreamer's Helmet");
			Tooltip.SetDefault("Blue spirit arms will fight for you\n'You have become a superior being'");
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;

			Item.value = Item.sellPrice(silver: 3);

			Item.defense = 4;
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 5);
			recipe.AddIngredient(ItemID.DivingHelmet);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
