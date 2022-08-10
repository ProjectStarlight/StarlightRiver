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
using Terraria.DataStructures;
using StarlightRiver.Content.Tiles.Misc;

namespace StarlightRiver.Core.Systems.ChestLootSystem
{
    public class ChestLootSystem : IPostLoadable
    {
        public static ChestLootSystem Instance { get; private set; } //temp fix until a better solution is implemented in a future pr

        private const float displayCaseChance = 0.925f; // = 0.125f;

        private Dictionary<int, ChestRegionFlags> FramingToRegion;
        private HashSet<ChestRegionFlags> DisplayCaseReplaceable;
        private Dictionary<ChestRegionFlags, List<ChestLootInfo>> RegionLootInfo;
        private Dictionary<ChestRegionFlags, List<ChestLootInfo>> RegionExclusiveLootInfo;

        public void PostLoad()
        {
            Instance = this;

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
                [ModContent.TileType<Content.Tiles.Vitric.VitricRock>() + 10000] = ChestRegionFlags.Vitric,//placeholder tile
                [ModContent.TileType<Content.Tiles.Permafrost.AuroraIce>() + 10000] = ChestRegionFlags.Permafrost,//placeholder tile
                [ModContent.TileType<Content.Tiles.Overgrow.Rock2x2>() + 10000] = ChestRegionFlags.Overgrowth,//placeholder tile
            };

            DisplayCaseReplaceable = new HashSet<ChestRegionFlags> // Could be a dictionary if seperate chances are needed
            {
                ChestRegionFlags.Underground,
                //ChestRegionFlags.Dungeon,
                //ChestRegionFlags.Underworld,
                ChestRegionFlags.Jungle,
                ChestRegionFlags.JungleShrine,
                ChestRegionFlags.Sky,
                ChestRegionFlags.Ice,
            };

            RegionLootInfo = new Dictionary<ChestRegionFlags, List<ChestLootInfo>>();

            foreach (ChestRegionFlags val in Enum.GetValues<ChestRegionFlags>())
                RegionLootInfo.Add(val, new List<ChestLootInfo>());

            RegionExclusiveLootInfo = new Dictionary<ChestRegionFlags, List<ChestLootInfo>>();

            foreach (ChestRegionFlags val in Enum.GetValues<ChestRegionFlags>())
                RegionExclusiveLootInfo.Add(val, new List<ChestLootInfo>());

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                if (!type.IsAbstract && type.IsSubclassOf(typeof(LootPool)))
                {
                    LootPool toLoad = (LootPool)Activator.CreateInstance(type);

                    toLoad.AddLoot();

                    foreach (KeyValuePair<ChestRegionFlags, List<ChestLootInfo>> pair in toLoad.LootInfo)
                        RegionLootInfo[pair.Key].AddRange(pair.Value);

                    foreach (KeyValuePair<ChestRegionFlags, List<ChestLootInfo>> pair in toLoad.ExclusiveLootInfo)
                        RegionExclusiveLootInfo[pair.Key].AddRange(pair.Value);
                }
        }

        public void PostLoadUnload()
        {
            Instance = null;
            FramingToRegion = null;
            DisplayCaseReplaceable = null;
            RegionLootInfo = null;
            RegionExclusiveLootInfo = null;
        }

        public void PopulateAllChests()
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
                    // Modded tiles will always be checked by having 10000 added to their ID
                    if (!FramingToRegion.TryGetValue(
                        ModContent.GetModTile(tile.TileType) != null ?
                            tile.TileType + 10000 :
                            tile.TileFrameX + (tile.TileType == 467 ? 2000 : 0), out ChestRegionFlags region))
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

                    if (DisplayCaseReplaceable.Contains(region) && WorldGen.genRand.NextFloat() < displayCaseChance)
                        if (PlaceDisplayCaseOn(chest))
                            continue;// Doesn't add non-exclusive items if a display case has placed so it does not try and access a chest that no longer exists

                    {
                        // Gets all valid items for this chest type plus the all chest type and then shuffles it.
                        List<ChestLootInfo> itemInfoList = new(RegionLootInfo[region]);
                        itemInfoList.AddRange(RegionLootInfo[ChestRegionFlags.All]);
                        itemInfoList = itemInfoList.OrderBy(x => WorldGen.genRand.Next()).ToList();

                        if (itemInfoList.Count > 0)
                            foreach (ChestLootInfo itemInfo in itemInfoList)
                                if (WorldGen.genRand.NextFloat() < itemInfo.chance)
                                    AddChestItem(itemInfo, chest);
                    }
                }
            }
        }

        private Item SetupItem(int type, int stack, bool isRelic)
        {
            Item Item = new Item(type, stack);

            Item.GetGlobalItem<RelicItem>().isRelic = isRelic;
            Item.Prefix(ItemLoader.ChoosePrefix(Item, Main.rand));

            return Item;
        }
        private void AddChestItem(ChestLootInfo info, Chest chest)
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
                chest.item[slot] = SetupItem(info.itemType, stack, false);
        }

        private bool PlaceDisplayCaseOn(Chest chest)
        {
            int type = ItemID.None;

            for (int i = 0; i < chest.item.Length; i++)
            {
                Item Item = chest.item[i];

                // Checks if the "main" chest Item is replaceable (weapon or accessory, and not stackable).
                if (Item.accessory || Item.damage > 0 && Item.notAmmo && Item.maxStack == 1)
                {
                    type = chest.item[i].type;

                    break;
                }
            }

            if (type != ItemID.None)
            {
                Item Item = SetupItem(type, 1, true);

                Helper.PlaceMultitile(new Point16(chest.x, chest.y - 1), ModContent.TileType<DisplayCase>());
                TileEntity.PlaceEntityNet(chest.x, chest.y - 1, ModContent.TileEntityType<DisplayCaseEntity>());
                (TileEntity.ByPosition[new Point16(chest.x, chest.y - 1)] as DisplayCaseEntity).containedItem = Item;
                return true;
            }
            return false;
        }
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