using StarlightRiver.Content.Tiles.Mushroom;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class TruffleShopAdditions : GlobalNPC
	{
		public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
		{
			if (type == NPCID.Truffle)
			{
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<JellyShroomItem>());
				nextSlot++;
			}
		}
	}
}