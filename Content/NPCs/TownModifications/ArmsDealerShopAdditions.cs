using StarlightRiver.Content.Items.ArmsDealer;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	internal class ArmsDealerShopAdditions : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.ArmsDealer;
		}

		public override void ModifyShop(NPCShop shop)
		{
			if (shop.NpcType == NPCID.ArmsDealer)
			{
				shop.Add(new NPCShop.Entry(ModContent.ItemType<ArtilleryLicense>()));
				shop.Add(new NPCShop.Entry(ModContent.ItemType<DefenseSystem>())
					.AddShopOpenedCallback(
					(item, npc) =>
					{
						var modItem = item.ModItem as DefenseSystem;
						modItem.dealerName = npc.GivenName;
					}));
			}
		}
	}
}