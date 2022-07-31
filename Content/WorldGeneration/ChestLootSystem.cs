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
using StarlightRiver.Content.Items.BaseTypes;

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

            AddLoot(ItemID.AcidDye, (1, 4), 0.01f, ChestRegionFlags.Ice);
            AddLoot(ItemID.Abeemination, (2, 3), 0.75f, ChestRegionFlags.Sky);
            AddLoot(ItemID.AlphabetStatueE, (50, 100), 1f, ChestRegionFlags.All, 20);
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

        public static void PopulateAllChests()
        {
            for (int i = 0; i < Main.maxChests; i++)
            {
                if (i >= Main.chest.Length) //failsafe
                    return;

                Chest chest = Main.chest[i];

                // Within this block this chest is valid to put an Item in.
                if (chest != null && Framing.GetTileSafely(chest.x, chest.y) is Tile tile && tile.HasTile)
                {
                    //if (WorldGen.genRand.NextFloat() < displayCaseChance && IsDisplayCaseReplaceable(tile.TileFrameX))
                    //{
                    //    PlaceDisplayCaseOn(chest);

                    //    // Continues because we don't want the code after this to touch a chest that is no longer accessible.
                    //    continue;
                    //}

                    // Selects a random Item to be placed in a chest.
                    ChestLootInfo itemInfo = WorldGen.genRand.Next(LootInfo);

                    // Type check is to prevent dungeon wooden chests being treated as surface ones.
                    if (WorldGen.genRand.NextFloat() < itemInfo.chance && chest.item[0].type != ItemID.GoldenKey && TileMatchesRegionFlags(itemInfo.chestRegions, tile))
                    {
                        int stack = WorldGen.genRand.Next(itemInfo.stackRange.Item1, itemInfo.stackRange.Item2 + 1);
                        int slot = itemInfo.slotIndex;

                        //finds first open slot
                        if(slot == -1)
                            for (int g = 0; g < chest.item.Length; g++)
                                if (chest.item[g].IsAir) {
                                    slot = g;
                                    break;
                                }

                        //slot is checked again in case no open slot was found, and stack is checked in case the minimum was zero
                        if(slot != -1 && stack > 0)
                        {
                            chest.item[slot] = SetupItem(itemInfo.itemType, stack, false);//isRelic option is unused
                        }
                    }
                }
            }
        }

        private static Item SetupItem(int type, int stack, bool isRelic)
        {
            Item Item = new Item(type, stack);

            Item.GetGlobalItem<RelicItem>().isRelic = isRelic;
            Item.Prefix(ItemLoader.ChoosePrefix(Item, Main.rand));

            return Item;
        }
        private static bool TileMatchesRegionFlags(ChestRegionFlags flags, Tile tile)
        {
            if (flags.HasFlag(ChestRegionFlags.All))
            {
                return true;
            }

            ChestRegionFlags[] values = (ChestRegionFlags[])Enum.GetValues(typeof(ChestRegionFlags));

            foreach (ChestRegionFlags flag in values)
            {
                if (flag == ChestRegionFlags.All)
                {
                    continue;
                }

                if (flags.HasFlag(flag))
                {
                    int[] frames = RegionsToFraming[flag];

                    foreach (int frame in frames)
                    {
                        if (tile.TileFrameX == frame)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public class ChestLootInfo
    {
        public readonly int itemType;//this may need to be a func<int> instead
        public readonly (int, int) stackRange;
        public readonly float chance;
        public readonly ChestRegionFlags chestRegions;
        public readonly int slotIndex;
        public ChestLootInfo(int itemType, (int, int) stackRange, float chance, ChestRegionFlags chestRegions, int slotIndex = -1) 
        { 
            this.itemType = itemType;
            this.stackRange = stackRange;
            this.chance = chance;
            this.chestRegions = chestRegions;
            this.slotIndex = slotIndex;
        }
    }
}