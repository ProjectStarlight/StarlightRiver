using StarlightRiver.Core;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
	internal class GiantMushroom : Ingredient
	{
		public GiantMushroom() : base("+40 max health", 1100, IngredientType.Main) { }

		public override void SafeSetDefaults()
		{
			Item.value = 200;
			Item.rare = ItemRarityID.Orange;
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.statLifeMax2 += (int)(40 * multiplier);
		}
	}
}