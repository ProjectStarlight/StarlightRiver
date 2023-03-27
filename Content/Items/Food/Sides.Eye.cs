using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
	internal class Eye : Ingredient
	{
		public Eye() : base("+10% critical strike chance", 540, IngredientType.Side) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetCritChance(DamageClass.Generic) += 0.1f * multiplier;
		}
	}
}