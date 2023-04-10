using StarlightRiver.Content.Items.Misc;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class PartyGirlShopAdditions : GlobalNPC
	{
		public override void ModifyShop(NPCShop shop)
		{
			if (shop.NpcType == NPCID.PartyGirl)
			{
				shop.Add(new NPCShop.Entry(ModContent.ItemType<BalloonGun>()));
			}
		}
	}
}