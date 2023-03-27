using StarlightRiver.Content.Items.Misc;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class MechanicShopAdditions : GlobalNPC
	{
		public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
		{
			if (type == NPCID.Mechanic)
			{
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<Sorcerwrench>());
				nextSlot++;
			}
		}
	}
}