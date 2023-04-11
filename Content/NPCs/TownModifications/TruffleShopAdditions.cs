using StarlightRiver.Content.Tiles.Mushroom;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class TruffleShopAdditions : GlobalNPC
	{
		public override void ModifyShop(NPCShop shop)
		{
			if (shop.NpcType == NPCID.Truffle)
			{
				shop.Add(new NPCShop.Entry(ModContent.ItemType<JellyShroomItem>()));
			}
		}
	}
}