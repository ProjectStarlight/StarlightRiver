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
	internal class SupportCookies : BonusIngredient
	{
		public SupportCookies() : base("Life and mana pickups restore bonus life and mana") { }

		public override FoodRecipie Recipie() => new FoodRecipie(
			ModContent.ItemType<SupportCookies>(),
			ModContent.ItemType<Dough>(),
			ModContent.ItemType<HealthExtract>(),
			ModContent.ItemType<ManaExtract>(),
			ModContent.ItemType<TableSalt>()
			);

		public override void Load()
		{
			StarlightItem.OnPickupEvent += BonusHeal;
		}

		private bool BonusHeal(Item Item, Player Player)
		{
			if(Active(Player))
			{
				if(Item.type == ItemID.Heart || Item.type == ItemID.Star)
				{
					Player.HealEffect(5);
					Player.ManaEffect(5);
					Player.statLife += 5;
					Player.statMana += 5;
				}
			}

			return true;
		}
	}
}
