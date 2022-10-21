using StarlightRiver.Content.Items.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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