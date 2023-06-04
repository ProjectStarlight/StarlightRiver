using StarlightRiver.Content.Items.Jungle;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.ChestLootSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Paintings
{
	public class BarrelPool : LootPool
	{
		public override void AddLoot()
		{
			AddItem(ItemID.Torch, ChestRegionFlags.Barrel, 0.90f, (3, 10), false);
			AddItem(ItemID.Ale, ChestRegionFlags.Barrel, 0.75f, (1, 2), false);
			AddItem(ItemID.Mug, ChestRegionFlags.Barrel, 0.25f, (1, 3), false);

			AddItem(ItemID.Bottle, ChestRegionFlags.Barrel, 0.20f, (3, 8), false);
			AddItem(ItemID.LesserHealingPotion, ChestRegionFlags.Barrel, 0.35f, (1, 3), false);
			AddItem(ItemID.HealingPotion, ChestRegionFlags.Barrel, 0.05f, 1, false);

			AddItem(ItemID.Gel, ChestRegionFlags.Barrel, 0.35f, (5, 10), false);
			AddItem(ItemID.Wood, ChestRegionFlags.Barrel, 0.50f, (5, 20), false);
			AddItem(ItemID.RecallPotion, ChestRegionFlags.Barrel, 0.25f, (1, 3), false);

			AddItem(ItemID.ShinePotion, ChestRegionFlags.Barrel, 0.20f, (1, 2), false);
			AddItem(ItemID.NightOwlPotion, ChestRegionFlags.Barrel, 0.15f, (1, 3), false);
			AddItem(ItemID.IronskinPotion, ChestRegionFlags.Barrel, 0.10f, (1, 2), false);

			AddItem(ItemID.MiningPotion, ChestRegionFlags.Barrel, 0.20f, (1, 2), false);
			AddItem(ItemID.BuilderPotion, ChestRegionFlags.Barrel, 0.08f, 1, false);
			AddItem(ItemID.SilverCoin, ChestRegionFlags.Barrel, 0.35f, (2, 5), false);

			//AddItem(ItemID.WoodenArrow, ChestRegionFlags.Barrel, 0.25f, (10, 20), false);
			//AddItem(ItemID.Shuriken, ChestRegionFlags.Barrel, 0.20f, (10, 15), false);
			AddItem(ItemID.Bomb, ChestRegionFlags.Barrel, 0.10f, 1, false);

			AddItem(ItemID.BowlofSoup, ChestRegionFlags.Barrel, 0.08f, (1, 2), true);
			AddItem(ItemID.PumpkinPie, ChestRegionFlags.Barrel, 0.03f, 1, true);
			AddItem(ItemID.ApplePie, ChestRegionFlags.Barrel, 0.03f, 1, true);
			AddItem(ItemID.Steak, ChestRegionFlags.Barrel, 0.05f, 1, true);
		}
	}
}