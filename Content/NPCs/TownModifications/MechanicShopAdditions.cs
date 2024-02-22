using StarlightRiver.Content.Items.Misc;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class MechanicShopAdditions : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.Mechanic;
		}

		public override void ModifyShop(NPCShop shop)
		{
			if (shop.NpcType == NPCID.Mechanic)
			{
				shop.Add(new NPCShop.Entry(ModContent.ItemType<Sorcerwrench>()));
			}
		}
	}
}