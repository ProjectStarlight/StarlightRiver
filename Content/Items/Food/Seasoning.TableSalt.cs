using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class TableSalt : Ingredient
	{
		public TableSalt() : base(Language.GetTextValue("CommonItemTooltip.FoodBuffEffectPercentBonus",5), 2400, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.createTile = ModContent.TileType<Tiles.Cooking.TableSalt>();
			Item.consumable = true;
			Item.value = 400;
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.05f;
		}
	}
}