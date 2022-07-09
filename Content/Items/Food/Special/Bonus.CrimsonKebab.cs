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
	internal class CrimsonKebab : BonusIngredient
	{
		public CrimsonKebab() : base("Replaces your blood with Ichor, which reduces armor of enemies that damage you\nWip") { }

		public override FoodRecipie Recipie() => new FoodRecipie(
			Type,
			ModContent.ItemType<CrimsonSteak>(),
			ModContent.ItemType<VertebrateNuggets>(),
			ModContent.ItemType<Eye>(),
			ModContent.ItemType<BlackPepper>()
			);

        public override void BuffEffects(Player Player, float multiplier)
        {

		}
	}
}
