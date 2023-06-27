using StarlightRiver.Core.Systems.ChestLootSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Desert
{
	public class AnkhPool : LootPool
	{
		public override void AddLoot()
		{
			//primary loot
			AddItem(ModContent.ItemType<DefiledAnkh>(), ChestRegionFlags.Ankh, 1, 1, true, 0);

			int[] pyramidLoot = new int[]
			{
				ItemID.FlyingCarpet,
				ItemID.SandstorminaBottle
			};

			AddItem(pyramidLoot, ChestRegionFlags.Ankh, 1, 1, false, 1);

			//secondary loot
			AddItem(ItemID.ScarabBomb, ChestRegionFlags.Ankh, 0.33f, (10, 20), false, -1);
			AddItem(ItemID.Rope, ChestRegionFlags.Ankh, 0.33f, (50, 100), false, -1);
			AddItem(ItemID.HealingPotion, ChestRegionFlags.Ankh, 0.5f, (4, 7), false, -1);

			int[] potions = new int[]
			{
				ItemID.SpelunkerPotion,
				ItemID.FeatherfallPotion,
				ItemID.NightOwlPotion,
				ItemID.WaterWalkingPotion,
				ItemID.ArcheryPotion,
				ItemID.GravitationPotion
			};

			AddItem(potions, ChestRegionFlags.Ankh, 0.66f, (2, 4), false, -1);

			int[] otherPotions = new int[]
			{
				ItemID.ThornsPotion,
				ItemID.WaterWalkingPotion,
				ItemID.InvisibilityPotion,
				ItemID.HunterPotion,
				ItemID.TrapsightPotion,
				ItemID.TeleportationPotion
			};

			AddItem(otherPotions, ChestRegionFlags.Ankh, 0.33f, (2, 4), false, -1);
			AddItem(ItemID.RecallPotion, ChestRegionFlags.Ankh, 0.5f, (3, 5), false, -1);

			int[] lightItems = new int[]
			{
				ItemID.SpelunkerGlowstick,
				ItemID.DesertTorch,
			};

			AddItem(lightItems, ChestRegionFlags.Ankh, 0.5f, (15, 30), false, -1);
			AddItem(ItemID.GoldCoin, ChestRegionFlags.Ankh, 0.5f, (1, 3), false, -1);
		}
	}
}