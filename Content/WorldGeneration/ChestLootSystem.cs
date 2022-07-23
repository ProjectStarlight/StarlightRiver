using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Content.Items.Vitric;

namespace StarlightRiver.Content.WorldGeneration
{
    public static class ChestLootSystem
    {
        public static List<ChestLootInfo> LootInfo;
        public static Dictionary<ChestRegionFlags, int[]> RegionsToFraming;

        public static void Initialize()
        {
            LootInfo = new List<ChestLootInfo>();
            RegionsToFraming = new Dictionary<ChestRegionFlags, int[]>
            {
                [ChestRegionFlags.Surface] = new int[] { 0, 432 }, // Wooden and living wood.
                [ChestRegionFlags.Underground] = new int[] { 36 },
                [ChestRegionFlags.Ice] = new int[] { 396 },
                [ChestRegionFlags.Jungle] = new int[] { 288, 360 },
                [ChestRegionFlags.Temple] = new int[] { 576 },
                [ChestRegionFlags.Sky] = new int[] { 468 },
                [ChestRegionFlags.Underwater] = new int[] { 612 },
                [ChestRegionFlags.Spider] = new int[] { 540 },
                [ChestRegionFlags.Granite] = new int[] { 1800 },
                [ChestRegionFlags.Marble] = new int[] { 1836 },
                [ChestRegionFlags.Underworld] = new int[] { 144 },
                [ChestRegionFlags.Dungeon] = new int[] { 72 },
            };
        }

        public static void Load()
        {
            Initialize();

            AddLoot(ItemID.AcidDye, (1, 4), 0.01f, ChestRegionFlags.All);
            AddLoot(ItemID.Abeemination, (2, 3), 0.1f, ChestRegionFlags.Sky);
            AddLoot(ItemID.AlphabetStatueE, (50, 100), 1f, ChestRegionFlags.Ice, 20);
        }

        private static void AddLoot(int item, (int, int) stackRange, float chance, ChestRegionFlags chestRegions, int slotIndex = -1)
        {
            LootInfo.Add(new ChestLootInfo(item, stackRange, chance, chestRegions, slotIndex));
        }

        public static void Unload()
        {
            LootInfo = null;
            RegionsToFraming = null;
        }
    }

    public class ChestLootInfo
    {
        public readonly int item;//this may need to be a func<int> instead
        public readonly (int, int) stackRange;
        public readonly float chance;
        public readonly ChestRegionFlags chestRegions;
        public readonly int slotIndex;
        public ChestLootInfo(int item, (int, int) stackRange, float chance, ChestRegionFlags chestRegions, int slotIndex = -1) 
        { 
            this.item = item;
            this.stackRange = stackRange;
            this.chance = chance;
            this.chestRegions = chestRegions;
            this.slotIndex = slotIndex;
        }
    }
}