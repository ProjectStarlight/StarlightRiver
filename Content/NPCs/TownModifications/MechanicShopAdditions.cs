using StarlightRiver.Content.Items.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
