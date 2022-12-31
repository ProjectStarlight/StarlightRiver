using StarlightRiver.Content.Tiles.Mushroom;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class TruffleShopAdditions : GlobalNPC
	{
		public override void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type == NPCID.Truffle)
			{
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<JellyShroomItem>());
				nextSlot++;
			}
		}
	}
}
