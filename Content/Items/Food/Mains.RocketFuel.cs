﻿using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class RocketFuel : Ingredient
	{
		//wip
		public RocketFuel() : base("+500% effectiveness\nOnly 10% duration\nYou are afflicted with Warpfire for duration", 180, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;

			Item.value = Item.sellPrice(silver: 20);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetModPlayer<FoodBuffHandler>().Multiplier += 5f;
			//add warpfire debuff when it exists
		}
	}
}