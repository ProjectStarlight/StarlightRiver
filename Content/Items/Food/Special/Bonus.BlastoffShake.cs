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
	internal class BlastoffShake : BonusIngredient
	{
		public BlastoffShake() : base("Violently delicious!\nWip") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<RocketFuel>(),
			ModContent.ItemType<Milk>(),
			ModContent.ItemType<ChocolateGlaze>(),
			ModContent.ItemType<Sugar>()
			);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{

		}
	}
}