using System.Collections.Generic;
using System.Linq;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Food
{
	internal class FoodBuffHandler : ModPlayer
	{
		public List<Item> Consumed { get; set; } = new List<Item>(); //all of the ingredients in the food the Player ate
		public float Multiplier { get; set; } = 1; //the multipler that should be applied to those ingredients
		public float oldMult = 1;

		public override void ResetEffects()
		{
			oldMult = Multiplier;

			if (!Player.HasBuff(BuffType<Buffs.FoodBuff>()) && Consumed.Count > 0)
				Consumed.Clear(); //clears the Player's "belly" if they're not under the effects of food anymore, also resets the multiplier just in case.

			Multiplier = 1;

			foreach (Item Item in Consumed.Where(n => n.ModItem is Ingredient))
				(Item.ModItem as Ingredient).ResetBuffEffects(Player, Multiplier);
		}
	}
}