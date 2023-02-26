using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food.Special
{
	internal class SupportCookies : BonusIngredient
	{
		public SupportCookies() : base(Language.GetTextValue("Mods.StarlightRiver.Custom.Items.Food.SpecialEffect.SupportCookiesEffect")) { }

		public override FoodRecipie Recipie()
		{
			return new FoodRecipie(
			ModContent.ItemType<SupportCookies>(),
			ModContent.ItemType<Dough>(),
			ModContent.ItemType<HealthExtract>(),
			ModContent.ItemType<ManaExtract>(),
			ModContent.ItemType<TableSalt>()
			);
		}

		public override void Load()
		{
			StarlightItem.OnPickupEvent += BonusHeal;
		}

		private bool BonusHeal(Item Item, Player Player)
		{
			if (Active(Player))
			{
				if (Item.type == ItemID.Heart || Item.type == ItemID.Star)
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
