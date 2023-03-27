using StarlightRiver.Content.Items.Misc;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class PartyGirlShopAdditions : GlobalNPC
	{
		public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
		{
			if (type == NPCID.PartyGirl)
			{
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<BalloonGun>());
				nextSlot++;
			}
		}
	}
}