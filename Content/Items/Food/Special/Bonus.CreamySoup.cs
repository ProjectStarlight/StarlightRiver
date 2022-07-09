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
	internal class CreamySoup : BonusIngredient
	{
		public CreamySoup() : base("Critical strikes reveal the secret of the sauce, horrifying enemies and causing them to deal 50% less damage the next time they hit you.\nWip") { }

		public override FoodRecipie Recipie() => new FoodRecipie(
			Type,
			ModContent.ItemType<Entrails>(),
			ModContent.ItemType<VertebrateNuggets>(),	
			ModContent.ItemType<Milk>(),
			ModContent.ItemType<TableSalt>()
			);

        public override void BuffEffects(Player Player, float multiplier)
        {
			
		}
	}
}
