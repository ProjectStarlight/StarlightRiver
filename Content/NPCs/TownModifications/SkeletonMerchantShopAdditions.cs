using StarlightRiver.Content.Items.Misc;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class SkeletonMerchantShopAdditions : GlobalNPC
	{
		public override void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type == NPCID.SkeletonMerchant && Main.moonPhase > 2 && Main.moonPhase < 5)
			{
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<BizarrePotion>());
				nextSlot++;
			}
		}
	}
}