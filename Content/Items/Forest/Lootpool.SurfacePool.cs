using StarlightRiver.Core.Systems.ChestLootSystem;

namespace StarlightRiver.Content.Items.Forest
{
	public class SurfacePool : LootPool
	{
		public override void AddLoot()
		{
			AddItem(ModContent.ItemType<AcornSprout>(), ChestRegionFlags.Surface, 0.5f, 1, true, 1);
			AddItem(ModContent.ItemType<DustyAmulet>(), ChestRegionFlags.Surface, 0.5f, 1, true, 1);
			AddItem(ModContent.ItemType<OldWhetstone>(), ChestRegionFlags.Surface, 0.5f, 1, true, 1);
			AddItem(ModContent.ItemType<Trowel>(), ChestRegionFlags.Surface, 0.5f, 1, true, 1);
		}
	}
}