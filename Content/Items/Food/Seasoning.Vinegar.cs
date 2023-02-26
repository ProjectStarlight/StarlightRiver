﻿using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food
{
	internal class Vinegar : Ingredient
	{
		public Vinegar() : base(Language.GetTextValue("CommonItemTooltip.FoodBuffEffectPercentReduce",40), 4800, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
			Item.value = 1000;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier -= 0.4f;
		}
	}
}