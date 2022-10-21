using StarlightRiver.Content.Items.Food;
using StarlightRiver.Content.Items.Utility;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class MerchantShopAdditions : GlobalNPC
	{
		public override void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type == NPCID.Merchant)
			{
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<ArmorBag>());
				nextSlot++;
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<ChefBag>());
				nextSlot++;

				shop.item[nextSlot].SetDefaults(ModContent.ItemType<TableSalt>());
				nextSlot++;
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<BlackPepper>());
				nextSlot++;
			}
		}
	}
}
