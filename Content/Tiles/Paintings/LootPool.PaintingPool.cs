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
			AddItem(ModContent.ItemType<AuroraclePaintingItem>(), ChestRegionFlags.Ice | ChestRegionFlags.Permafrost, 0.15f, 1, false, -1);
			AddItem(ModContent.ItemType<EggCodexPaintingItem>(), ChestRegionFlags.Underground | ChestRegionFlags.Livingwood, 0.08f, 1, false, -1);
			AddItem(ModContent.ItemType<EndOfTimePaintingItem>(), ChestRegionFlags.Underground | ChestRegionFlags.TrappedUnderground, 0.03f, 1, false, -1);
			AddItem(ModContent.ItemType<RatKingPaintingItem>(), ChestRegionFlags.Underground | ChestRegionFlags.TrappedUnderground | ChestRegionFlags.Trashcan, 0.03f, 1, false, -1);
		}
	}
}