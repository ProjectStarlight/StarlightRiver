using StarlightRiver.Content.Items.Misc;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class SkeletonMerchantShopAdditions : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.SkeletonMerchant;
		}

		public override void ModifyShop(NPCShop shop)
		{
			if (shop.NpcType == NPCID.SkeletonMerchant)
			{
				shop.Add(new NPCShop.Entry(ItemID.SpelunkerFlare, Items.Breacher.FlareBreacher.getMerchantFlareCondition()));
				shop.Add(new NPCShop.Entry(ModContent.ItemType<BizarrePotion>(), Condition.MoonPhaseFirstQuarter));
			}
		}
	}
}