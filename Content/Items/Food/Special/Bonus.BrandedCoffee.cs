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
	internal class BrandedCoffee : BonusIngredient
	{
		public BrandedCoffee() : base("Using abilities creates small starseekers that home in on foes based on your held weapon's damage and attack speed\n'The foam on top resembles a four-pointed star'\nWip") { }

		public override FoodRecipie Recipie() => new FoodRecipie(
			Type,
			ModContent.ItemType<CoffeeBeans>(),
			ModContent.ItemType<Milk>(),
			ModContent.ItemType<WhippedCream>(),
			ModContent.ItemType<SkySprinkles>()
			);

        public override void BuffEffects(Player Player, float multiplier)
        {
			
		}
	}
}
