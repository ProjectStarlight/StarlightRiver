using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Tiles.Forest;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class DryadShopAdditions : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.Dryad;
		}

		public override void ModifyShop(NPCShop shop)
		{
			if (shop.NpcType == NPCID.Dryad)
			{
				shop.Add(new NPCShop.Entry(ModContent.ItemType<ThickTreeAcorn>()));
			}
		}
	}
}