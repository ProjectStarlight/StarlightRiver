using StarlightRiver.Content.Items.Misc;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class SkeletonMerchantShopAdditions : GlobalNPC
	{
		public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
		{
			if (type == NPCID.SkeletonMerchant && Main.moonPhase > 2 && Main.moonPhase < 5)
			{
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<BizarrePotion>());
				nextSlot++;
			}
		}
	}
}