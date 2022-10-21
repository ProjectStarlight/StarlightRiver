using StarlightRiver.Content.Items.Vanity;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class TravellingMerchantShopAdditions : GlobalNPC
	{
		public override void SetupTravelShop(int[] shop, ref int nextSlot)
		{
			if (Main.rand.NextBool(15))
			{
				shop[nextSlot] = ModContent.ItemType<WardenHat>();
				nextSlot++;
				shop[nextSlot] = ModContent.ItemType<WardenRobe>();
				nextSlot++;
			}
		}
	}
}