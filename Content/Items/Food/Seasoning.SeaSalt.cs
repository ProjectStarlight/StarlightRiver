using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class SeaSalt : Ingredient
	{
		public SeaSalt() : base(Language.GetTextValue("CommonItemTooltip.FoodBuffEffectPercentBonus",10)+ "\n" + Language.GetTextValue("CommonItemTooltip.BreatheUnderWater"), 1200, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.createTile = TileType<Tiles.Cooking.PinkSeaSalt>();
			Item.consumable = true;
			Item.rare = ItemRarityID.Blue;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.1f;
			Player.gills = true;
		}
	}
}