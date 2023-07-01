﻿using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Toast : Ingredient
	{
		public Toast() : base("+5% all damage\n+5% defense", 400, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 10);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetDamage(DamageClass.Generic) += 0.05f * multiplier;
			Player.statDefense += (int)(Player.statDefense * (0.05f * multiplier));
		}
	}
}