using StarlightRiver.Content.Items.Jungle;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.ChestLootSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Paintings
{
	public class PaintingPool : LootPool
	{
		public override void AddLoot()
		{
			AddItem(ModContent.ItemType<AuroraclePaintingItem>(), ChestRegionFlags.Underground | ChestRegionFlags.Trashcan | ChestRegionFlags.TrappedUnderground, 0.05f, 1, false, -1);
			AddItem(ModContent.ItemType<AuroraclePaintingItem>(), ChestRegionFlags.Ice | ChestRegionFlags.Permafrost, 0.1f, 1, false, -1);
			AddItem(ModContent.ItemType<AuroraclePaintingItem>(), ChestRegionFlags.Underground | ChestRegionFlags.Livingwood, 0.08f, 1, false, -1);
		}
	}
}