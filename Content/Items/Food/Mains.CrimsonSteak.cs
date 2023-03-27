using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class CrimsonSteak : Ingredient
	{
		public CrimsonSteak() : base("+5% all damage", 400, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.GetDamage(DamageClass.Generic) += 0.05f * multiplier;
		}
	}
}