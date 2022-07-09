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
	internal class MushroomSteak : BonusIngredient
	{
		public MushroomSteak() : base("Health pickups turn into mushrooms which grow over time, starting at 10 and ending at 80 after 30 seconds\nWip") { }

		public override FoodRecipie Recipie() => new FoodRecipie(
			Type,
			ModContent.ItemType<GiantMushroom>(),
			ModContent.ItemType<BrusselSprouts>(),
			ModContent.ItemType<IvySalad>(),
			ModContent.ItemType<TableSalt>()
			);

        public override void BuffEffects(Player Player, float multiplier)
        {
			
		}
	}
}
