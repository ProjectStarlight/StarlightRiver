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
	internal class JungleSalad : BonusIngredient
	{
		public JungleSalad() : base("Grasses and plants have a low chance to drop coins and other pot loot when broken\nWip") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<MahoganyRoot>(),
			ModContent.ItemType<HoneySyrup>(),
			ModContent.ItemType<DicedMushrooms>(),
			ModContent.ItemType<Vinegar>()
			);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{

		}
	}
}