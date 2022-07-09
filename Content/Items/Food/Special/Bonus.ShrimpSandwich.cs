using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food.Special
{
	internal class ShrimpSandwich : BonusIngredient
	{
		public ShrimpSandwich() : base("Ability to breathe and swim underwater.\nUnderwater enemies have a chance to drop random fish.\n\"It's just that shrimple\"\nWip") { }

		public override FoodRecipie Recipie() => new FoodRecipie(
			Type,
			ModContent.ItemType<Toast>(),
			ModContent.ItemType<JumboShrimp>(),
			ModContent.ItemType<Lettuce>(),
			ModContent.ItemType<Dressing>()
			);

        public override void BuffEffects(Player Player, float multiplier)
        {
			
		}
	}
}
