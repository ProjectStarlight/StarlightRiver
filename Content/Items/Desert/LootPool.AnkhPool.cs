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

			int pyramidLoot = WorldGen.genRand.Next(new int[]
			{
				ItemID.FlyingCarpet,
				ItemID.SandstorminaBottle
			});

			AddItem(pyramidLoot, ChestRegionFlags.Ankh, 1, 1, false, 1);

			//secondary loot
			AddItem(ItemID.ScarabBomb, ChestRegionFlags.Ankh, 0.33f, WorldGen.genRand.Next(10, 20), false, -1);
			AddItem(ItemID.Rope, ChestRegionFlags.Ankh, 0.33f, WorldGen.genRand.Next(50, 100), false, -1);
			AddItem(ItemID.HealingPotion, ChestRegionFlags.Ankh, 0.5f, WorldGen.genRand.Next(4, 7), false, -1);

			int potions = WorldGen.genRand.Next(new int[]
			{
				ItemID.SpelunkerPotion,
				ItemID.FeatherfallPotion,
				ItemID.NightOwlPotion,
				ItemID.WaterWalkingPotion,
				ItemID.ArcheryPotion,
				ItemID.GravitationPotion
			});

			AddItem(potions, ChestRegionFlags.Ankh, 0.66f, WorldGen.genRand.Next(2, 4), false, -1);

			int otherPotions = WorldGen.genRand.Next(new int[]
			{
				ItemID.ThornsPotion,
				ItemID.WaterWalkingPotion,
				ItemID.InvisibilityPotion,
				ItemID.HunterPotion,
				ItemID.TrapsightPotion,
				ItemID.TeleportationPotion
			});

			AddItem(otherPotions, ChestRegionFlags.Ankh, 0.33f, WorldGen.genRand.Next(2, 4), false, -1);
			AddItem(ItemID.RecallPotion, ChestRegionFlags.Ankh, 0.5f, WorldGen.genRand.Next(3, 5), false, -1);

			int lightItems = WorldGen.genRand.Next(new int[]
			{
				ItemID.SpelunkerGlowstick,
				ItemID.DesertTorch,
			});

			AddItem(lightItems, ChestRegionFlags.Ankh, 0.5f, WorldGen.genRand.Next(15, 30), false, -1);
			AddItem(ItemID.GoldCoin, ChestRegionFlags.Ankh, 0.5f, WorldGen.genRand.Next(1, 3), false, -1);
		}
	}
}