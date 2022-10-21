using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
	internal class MahoganyRoot : Ingredient
	{
		public MahoganyRoot() : base("+12 defense", 400, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Green;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.statDefense += (int)(12 * multiplier);
		}
	}
}