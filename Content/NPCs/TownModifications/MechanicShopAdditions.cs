using StarlightRiver.Content.Items.Misc;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class MechanicShopAdditions : GlobalNPC
	{
		public override void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type == NPCID.Mechanic)
			{
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<Sorcerwrench>());
				nextSlot++;
			}
		}
	}
}