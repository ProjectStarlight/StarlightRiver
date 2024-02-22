using StarlightRiver.Content.Items.Food;
using StarlightRiver.Content.Items.Utility;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class MerchantShopAdditions : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.Merchant;
		}

		public override void ModifyShop(NPCShop shop)
		{
			if (shop.NpcType == NPCID.Merchant)
			{
				shop.Add(new NPCShop.Entry(ItemID.Flare, Items.Breacher.FlareBreacher.getMerchantFlareCondition()));
				shop.Add(new NPCShop.Entry(ItemID.BlueFlare, Items.Breacher.FlareBreacher.getMerchantFlareCondition()));
				shop.Add(new NPCShop.Entry(ModContent.ItemType<ArmorBag>()));
				shop.Add(new NPCShop.Entry(ModContent.ItemType<ChefBag>()));
				shop.Add(new NPCShop.Entry(ModContent.ItemType<TableSalt>()));
				shop.Add(new NPCShop.Entry(ModContent.ItemType<BlackPepper>()));
				shop.Add(new NPCShop.Entry(ModContent.ItemType<Flour>()));
				shop.Add(new NPCShop.Entry(ModContent.ItemType<Sugar>()));
			}
		}
	}
}