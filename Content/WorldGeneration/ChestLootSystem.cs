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
        private static Dictionary<ChestRegionFlags, List<ChestLootInfo>> RegionExclusiveLootInfo;
        private static Dictionary<int, ChestRegionFlags> FramingToRegion;
        //private const float displayCaseChance = 0.125f;

        public static void Initialize()
        {
            FramingToRegion = new Dictionary<int, ChestRegionFlags>
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
                [1152] = ChestRegionFlags.Mushroom,
                [180] = ChestRegionFlags.Barrel,
                [216] = ChestRegionFlags.Trashcan,
                [648] = ChestRegionFlags.Biome,// Jungle
                [684] = ChestRegionFlags.Biome,// Corruption
                [720] = ChestRegionFlags.Biome,// Crimson
                [756] = ChestRegionFlags.Biome,// Hallowed
                [792] = ChestRegionFlags.Biome,// Ice
                [2432] = ChestRegionFlags.Biome,// Desert
                [2144] = ChestRegionFlags.TrappedUnderground,
                [2360] = ChestRegionFlags.Desert,

            };

            RegionLootInfo = new Dictionary<ChestRegionFlags, List<ChestLootInfo>>();

            foreach (ChestRegionFlags val in Enum.GetValues<ChestRegionFlags>())
                RegionLootInfo.Add(val, new List<ChestLootInfo>());

            RegionExclusiveLootInfo = new Dictionary<ChestRegionFlags, List<ChestLootInfo>>();

            foreach (ChestRegionFlags val in Enum.GetValues<ChestRegionFlags>())
                RegionExclusiveLootInfo.Add(val, new List<ChestLootInfo>());
        }

        public static void Load()
        {
            Initialize();

            AddLoot(ModContent.ItemType<BarbedKnife>(), ChestRegionFlags.Surface | ChestRegionFlags.Barrel);
            AddLoot(ModContent.ItemType<Cheapskates>(), ChestRegionFlags.Ice);
            AddLoot(ModContent.ItemType<Slitherring>(), ChestRegionFlags.Jungle, 0.20f);
            AddLoot(ModContent.ItemType<SojournersScarf>(), ChestRegionFlags.Underground | ChestRegionFlags.Surface | ChestRegionFlags.Granite | ChestRegionFlags.Marble, 0.1f);


            //AddLoot(ItemID.NecromanticScroll, ChestRegionFlags.Livingwood, 0.75f, (2, 3), false);
            //AddLoot(ItemID.AlphabetStatueE, ChestRegionFlags.All, 1f, (50, 100), false, 20);
            //AddLoot(ItemID.Abeemination, ChestRegionFlags.All, 0.75f, (2, 3), false);
            //AddLoot(ItemID.DeepTealPaint, ChestRegionFlags.All, 1f, (2, 67), false);
        }

        private static void AddLoot(int item, ChestRegionFlags chestRegions, float chance, (int, int) stackRange, bool exclusive = true, int slotIndex = -1)
        {
            foreach(ChestRegionFlags flag in chestRegions.GetFlags())
                (exclusive ? RegionExclusiveLootInfo[flag] : RegionLootInfo[flag]).Add(
                    new ChestLootInfo(item, stackRange, chestRegions, chance, slotIndex));
        }
        /// <summary>
        /// legacy overload, uses slot 0 to replace main chest loot
        /// </summary>
        private static void AddLoot(int item, ChestRegionFlags chestRegions, float chance = 0.125f, int stack = 1, bool exclusive = true, int slotIndex = 0)
        {
            foreach (ChestRegionFlags flag in chestRegions.GetFlags())
                (exclusive ? RegionExclusiveLootInfo[flag] : RegionLootInfo[flag]).Add(
                    new ChestLootInfo(item, (stack, stack), chestRegions, chance, slotIndex));
        }

        public static void Unload()
        {
            RegionLootInfo = null;
            RegionExclusiveLootInfo = null;
            FramingToRegion = null;
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
                    // This adds 2000 to the frame offset if the chest uses the alterative sheet
                    if (!FramingToRegion.TryGetValue(tile.TileFrameX + (tile.TileType == 467 ? 2000 : 0), out ChestRegionFlags region))
                        continue;

                    // Item type check is to prevent dungeon wooden chests being treated as surface ones.
                    if (chest.item[0].type != ItemID.GoldenKey)
                    {
                        // Gets all valid items for this chest type plus the all chest type
                        List<ChestLootInfo> itemInfoList = new(RegionExclusiveLootInfo[region]);
                        itemInfoList.AddRange(RegionExclusiveLootInfo[ChestRegionFlags.All]);

                        if (itemInfoList.Count > 0)
                        {
                            // Selects a random exclusive item to be placed in the chest
                            ChestLootInfo exclusiveitemInfo = WorldGen.genRand.Next(itemInfoList);

                            if (WorldGen.genRand.NextFloat() < exclusiveitemInfo.chance)
                                AddChestItem(exclusiveitemInfo, chest);
                        }
                    }

                    // If this is a wooden dungeon chest is not checked for non-exclusive items
                    {
                        // Gets all valid items for this chest type plus the all chest type and then shuffles it.
                        List<ChestLootInfo> itemInfoList = new(RegionLootInfo[region]);
                        itemInfoList.AddRange(RegionLootInfo[ChestRegionFlags.All]);
                        itemInfoList = itemInfoList.OrderBy(x => WorldGen.genRand.Next()).ToList();

                        if(itemInfoList.Count > 0)
                            foreach (ChestLootInfo itemInfo in itemInfoList)
                                if (WorldGen.genRand.NextFloat() < itemInfo.chance)
                                    AddChestItem(itemInfo, chest);
                    }

                    //if (WorldGen.genRand.NextFloat() < displayCaseChance && IsDisplayCaseReplaceable(tile.TileFrameX))
                    //{
                    //    PlaceDisplayCaseOn(chest);
                    //
                    //    // Continues because we don't want the code after this to touch a chest that is no longer accessible.
                    //    continue;
                    //}
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
        private static void AddChestItem(ChestLootInfo info, Chest chest)
        {
            int stack = WorldGen.genRand.Next(info.stackRange.Item1, info.stackRange.Item2 + 1);
            int slot = info.slotIndex;

            // Finds first open slot
            if (slot == -1)
                for (int g = 0; g < chest.item.Length; g++)
                    if (chest.item[g].IsAir)
                    {
                        slot = g;
                        break;
                    }

            // Slot is checked in case no open slot was found, Stack is checked in case the minimum was zero
            if (slot != -1 && stack > 0)
                chest.item[slot] = SetupItem(info.itemType, stack, false);//isRelic option is unused
        }
        //private static void PlaceDisplayCaseOn(Chest chest)
        //{
        //    int type = ItemID.None;

        //    for (int i = 0; i < chest.item.Length; i++)
        //    {
        //        Item Item = chest.item[i];

        //        // Checks if the "main" chest Item is replaceable (weapon or accessory, and not stackable).
        //        if (Item.accessory || (Item.damage > 0 && Item.notAmmo && Item.maxStack == 1))
        //        {
        //            type = chest.item[i].type;

        //            break;
        //        }
        //    }

        //    if (type != ItemID.None)
        //    {
        //        Item Item = SetupItem(type, 1, true);

        //        Helper.PlaceMultitile(new Point16(chest.x, chest.y - 1), ModContent.TileType<DisplayCase>());
        //        TileEntity.PlaceEntityNet(chest.x, chest.y - 1, ModContent.TileEntityType<DisplayCaseEntity>());
        //        (TileEntity.ByPosition[new Point16(chest.x, chest.y - 1)] as DisplayCaseEntity).containedItem = Item;
        //    }
        //}

        //private static bool IsDisplayCaseReplaceable(short frameX)
        //    => frameX == 36 || // Gold. 
        //    frameX == 72 || // Locked Gold.
        //    frameX == 144 || // Locked Evil.
        //    frameX == 288 || // Mahogany.
        //    frameX == 360 || // Ivy.
        //    frameX == 396 || // Ice.
        //    frameX == 468; // Sky.
    }

    public class ChestLootInfo
    {
        public readonly int itemType;//this may need to be a func<int> instead
        public readonly (int, int) stackRange;
        public readonly ChestRegionFlags chestRegions;
        //public readonly bool exclusive;
        public readonly float chance;
        public readonly int slotIndex;
        public ChestLootInfo(int itemType, (int, int) stackRange, ChestRegionFlags chestRegions, float chance, int slotIndex = -1) 
        { 
            this.itemType = itemType;
            this.stackRange = stackRange;
            this.chestRegions = chestRegions;
            //this.exclusive = exclusive;
            this.chance = chance;
            this.slotIndex = slotIndex;
        }
    }
}