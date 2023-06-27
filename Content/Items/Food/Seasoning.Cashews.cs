﻿using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class Cashews : Ingredient
	{
		public Cashews() : base("+5% melee damage\n+1 defense", 60, IngredientType.Seasoning) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;

			Item.value = Item.sellPrice(silver: 3);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.statDefense += (int)(1 * multiplier);
			Player.GetDamage(DamageClass.Melee) += 0.05f * multiplier;
		}
	}
}