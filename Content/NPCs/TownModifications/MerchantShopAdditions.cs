using StarlightRiver.Content.Items.Food;
using StarlightRiver.Content.Items.Utility;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class MerchantShopAdditions : GlobalNPC
	{
		public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
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