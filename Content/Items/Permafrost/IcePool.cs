using StarlightRiver.Core.Systems.ChestLootSystem;

namespace StarlightRiver.Content.Items.Permafrost
{
	public class IcePool : LootPool
	{
		public override void AddLoot()
		{
			AddItem(ModContent.ItemType<SquidBossSpawn>(), ChestRegionFlags.Ice, 1f, 1, false, -1);
		}
	}
}