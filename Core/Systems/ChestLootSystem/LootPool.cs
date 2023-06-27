using StarlightRiver.Helpers;
using System.Collections.Generic;

namespace StarlightRiver.Core.Systems.ChestLootSystem
{
	public abstract class LootPool
	{
		public readonly Dictionary<ChestRegionFlags, List<ChestLootInfo>> LootInfo = new();
		public readonly Dictionary<ChestRegionFlags, List<ChestLootInfo>> ExclusiveLootInfo = new();

		public virtual ChestRegionFlags Region => ChestRegionFlags.All;

		public abstract void AddLoot();

		/// <summary>
		/// legacy overload, uses slot 0 to replace main chest loot
		/// </summary>
		/// 
		protected void AddItem(int item, float chance = 0.125f, int stack = 1, bool exclusive = true, int slotIndex = 0)
		{
			AddItem(item, Region, chance, (stack, stack), exclusive, slotIndex);
		}

		protected void AddItem(int item, ChestRegionFlags chestRegions, float chance = 0.125f, int stack = 1, bool exclusive = true, int slotIndex = 0)
		{
			AddItem(item, chestRegions, chance, (stack, stack), exclusive, slotIndex);
		}

		protected void AddItem(int item, float chance, (int, int) stackRange, bool exclusive = true, int slotIndex = -1)
		{
			AddItem(item, Region, chance, stackRange, exclusive, slotIndex);
		}

		protected void AddItem(int item, ChestRegionFlags chestRegions, float chance, (int, int) stackRange, bool exclusive = true, int slotIndex = -1)
		{
			AddItem(new int[] { item }, chestRegions, chance, stackRange, exclusive, slotIndex);
		}

		//array versions (chooses one item from the array, while caaping the rest of the data the same
		protected void AddItem(int[] items, ChestRegionFlags chestRegions, float chance = 0.125f, int stack = 1, bool exclusive = true, int slotIndex = 0)
		{
			AddItem(items, chestRegions, chance, (stack, stack), exclusive, slotIndex);
		}

		protected void AddItem(int[] items, float chance, (int, int) stackRange, bool exclusive = true, int slotIndex = -1)
		{
			AddItem(items, Region, chance, stackRange, exclusive, slotIndex);
		}

		protected void AddItem(int[] items, ChestRegionFlags chestRegions, float chance, (int, int) stackRange, bool exclusive = true, int slotIndex = -1)
		{
			foreach (ChestRegionFlags flag in chestRegions.GetFlags())
			{
				Dictionary<ChestRegionFlags, List<ChestLootInfo>> dict = exclusive ? ExclusiveLootInfo : LootInfo;

				if (!dict.TryGetValue(flag, out List<ChestLootInfo> list))
					dict.Add(flag, new List<ChestLootInfo>());

				dict[flag].Add(new ChestLootInfo(items, stackRange, chestRegions, chance, slotIndex));
			}
		}
	}
}