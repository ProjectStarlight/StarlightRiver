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
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.WorldGeneration
{
    public static class ChestLootSystem
    {
        private static Dictionary<ChestRegionFlags, List<ChestLootInfo>> RegionLootInfo;
        private static Dictionary<int, ChestRegionFlags> FramingToRegions;

        public static void Initialize()
        {
            FramingToRegions = new Dictionary<int, ChestRegionFlags>
            {
                [0] = ChestRegionFlags.Surface,
                [36] = ChestRegionFlags.Underground,
                [396] = ChestRegionFlags.Ice,
                [288] = ChestRegionFlags.Jungle,
                [360] = ChestRegionFlags.JungleShrine,
                [576] = ChestRegionFlags.Temple,
                [468] = ChestRegionFlags.Sky,
                [612] = ChestRegionFlags.Underwater,
                [540] = ChestRegionFlags.Spider,
                [1800] = ChestRegionFlags.Granite,
                [1836] = ChestRegionFlags.Marble,
                [144] = ChestRegionFlags.Underworld,
                [72] = ChestRegionFlags.Dungeon,
                [432] = ChestRegionFlags.Livingwood,
                [2360] = ChestRegionFlags.Desert,
                [2432] = ChestRegionFlags.Biome
            };

            RegionLootInfo = new Dictionary<ChestRegionFlags, List<ChestLootInfo>>();

            foreach (ChestRegionFlags val in Enum.GetValues<ChestRegionFlags>())
                RegionLootInfo.Add(val, new List<ChestLootInfo>());
        }

        public static void Load()
        {
            Initialize();

            AddLoot(ModContent.ItemType<BarbedKnife>(), 1, ChestRegionFlags.Surface);
            AddLoot(ModContent.ItemType<Cheapskates>(), 1, ChestRegionFlags.Ice);
            AddLoot(ModContent.ItemType<Slitherring>(), 1, ChestRegionFlags.Jungle);
            AddLoot(ModContent.ItemType<SojournersScarf>(), 1, ChestRegionFlags.Underground | ChestRegionFlags.Surface | ChestRegionFlags.Granite | ChestRegionFlags.Marble);


            AddLoot(ItemID.Abeemination, (2, 3), ChestRegionFlags.Sky, 0.75f);
            AddLoot(ItemID.AlphabetStatueE, (50, 100), ChestRegionFlags.All, 1f, 20);
        }

        private static void AddLoot(int item, (int, int) stackRange, ChestRegionFlags chestRegions, float chance, int slotIndex = -1)
        {
            foreach(ChestRegionFlags flag in chestRegions.GetFlags())
                RegionLootInfo[flag].Add(new ChestLootInfo(item, stackRange, chance, chestRegions, slotIndex));
        }
        /// <summary>
        /// legacy overload, tries to replace main chest loot
        /// </summary>
        private static void AddLoot(int item, int stack, ChestRegionFlags chestRegions, float chance = 0.125f, int slotIndex = 0)
        {
            foreach (ChestRegionFlags flag in chestRegions.GetFlags())
                RegionLootInfo[flag].Add(new ChestLootInfo(item, (stack, stack), chance, chestRegions, slotIndex));
        }

        public static void Unload()
        {
            RegionLootInfo = null;
            FramingToRegions = null;
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
                    // Selects a random Item to be placed in a chest
                    ChestLootInfo itemInfo = WorldGen.genRand.Next(
                        RegionLootInfo[
                            FramingToRegions[tile.TileFrameX + (tile.TileType == 467 ? 2000 : 0)]]);



                    //if (WorldGen.genRand.NextFloat() < displayCaseChance && IsDisplayCaseReplaceable(tile.TileFrameX))
                    //{
                    //    PlaceDisplayCaseOn(chest);

                    //    // Continues because we don't want the code after this to touch a chest that is no longer accessible.
                    //    continue;
                    //}


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

        //private static bool TileMatchesRegionFlags(ChestRegionFlags flags, Tile tile)
        //{
        //    if (flags.HasFlag(ChestRegionFlags.All))
        //    {
        //        return true;
        //    }

        //    ChestRegionFlags[] values = (ChestRegionFlags[])Enum.GetValues(typeof(ChestRegionFlags));

        //    foreach (ChestRegionFlags flag in values)
        //    {
        //        if (flag == ChestRegionFlags.All)
        //        {
        //            continue;
        //        }

        //        if (flags.HasFlag(flag))
        //        {
        //            int[] frames = RegionsToFraming[flag];

        //            foreach (int frame in frames)
        //            {
        //                if (tile.TileFrameX == frame)
        //                {
        //                    return true;
        //                }
        //            }
        //        }
        //    }

        //    return false;
        //}
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