using StarlightRiver.Content.Tiles.Mushroom;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class TruffleShopAdditions : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.Truffle;
		}

		public override void ModifyShop(NPCShop shop)
		{
			if (shop.NpcType == NPCID.Truffle)
			{
				shop.Add(new NPCShop.Entry(ModContent.ItemType<JellyShroomItem>()));
			}
		}
	}
}