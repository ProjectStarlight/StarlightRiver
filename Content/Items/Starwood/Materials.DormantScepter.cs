using StarlightRiver.Core.Systems.ChestLootSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Starwood
{
	public class DormantScepter : QuickMaterial
	{
		public DormantScepter() : base("Dormant Scepter", "The scepter lies dormant, waning for mystical energy", 1, Item.sellPrice(silver: 2), ItemRarityID.Gray, AssetDirectory.StarwoodItem + "StarwoodScepterDormant", true) { }
	}

	public class DormantScepterPool : LootPool
	{
		public override void AddLoot()
		{
			AddItem(ModContent.ItemType<DormantScepter>(), ChestRegionFlags.Surface, 0.5f, 1, true, 2);
		}
	}
}