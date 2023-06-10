using StarlightRiver.Core.Systems.ChestLootSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	public class BarrelPool : LootPool
	{
		public override ChestRegionFlags Region => ChestRegionFlags.Barrel;

		public override void AddLoot()
		{
			AddItem(ItemID.Torch, 0.90f, (3, 10), false);
			AddItem(ItemID.Ale, 0.75f, (1, 2), false);
			AddItem(ItemID.Mug, 0.25f, (1, 3), false);

			AddItem(ItemID.Bottle, 0.20f, (3, 8), false);
			AddItem(ItemID.LesserHealingPotion, 0.35f, (1, 3), false);
			AddItem(ItemID.HealingPotion, 0.05f, 1, false);

			AddItem(ItemID.Gel, 0.35f, (5, 10), false);
			AddItem(ItemID.Wood, 0.50f, (5, 20), false);
			AddItem(ItemID.RecallPotion, 0.25f, (1, 3), false);

			AddItem(ItemID.ShinePotion, 0.20f, (1, 2), false);
			AddItem(ItemID.NightOwlPotion, 0.15f, (1, 3), false);
			AddItem(ItemID.IronskinPotion, 0.10f, (1, 2), false);

			AddItem(ItemID.MiningPotion, 0.20f, (1, 2), false);
			AddItem(ItemID.BuilderPotion, 0.08f, 1, false);
			AddItem(ItemID.SilverCoin, 0.35f, (2, 5), false);

			//AddItem(ItemID.WoodenArrow, ChestRegionFlags.Barrel, 0.25f, (10, 20), false);
			//AddItem(ItemID.Shuriken, ChestRegionFlags.Barrel, 0.20f, (10, 15), false);
			AddItem(ItemID.Bomb, 0.10f, 1, false);

			AddItem(ItemID.BowlofSoup, 0.08f, (1, 2), true);
			AddItem(ItemID.PumpkinPie, 0.03f, 1, true);
			AddItem(ItemID.ApplePie, 0.03f, 1, true);
			AddItem(ItemID.Steak, 0.05f, 1, true);
		}
	}
}