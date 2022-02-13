using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.WorldGeneration;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver
{
	public partial class StarlightRiver 
    {
        private const float displayCaseChance = 0.125f;

        private const float chanceToReplaceMainChestLootWithModdedItem = 0.125f;

        private List<(int, IChestItem)> chestItems;

        private Dictionary<ChestRegionFlags, int[]> regionsToFraming;

        private void AutoloadChestItems()
        {
            chestItems = new List<(int, IChestItem)>();

            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                ModItem modItem = ItemLoader.GetItem(i);

                if (modItem is IChestItem chestItem)
                {
                    chestItems.Add((i, chestItem));
                }
            }

            // Kind off ugly, but not really sure what else I could do.
            regionsToFraming = new Dictionary<ChestRegionFlags, int[]>
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

        public void PopulateChests()
        {
            for (int i = 0; i < Main.maxChests; i++)
            {
                if (i >= Main.chest.Length) //failsafe
                    return;

                Chest chest = Main.chest[i];

                // Within this block this chest is valid to put an item in.
                if (chest != null && Framing.GetTileSafely(chest.x, chest.y) is Tile tile && tile.active())
                {
                    if (WorldGen.genRand.NextFloat() < displayCaseChance && IsDisplayCaseReplaceable(tile.frameX))
                    {
                        PlaceDisplayCaseOn(chest);

                        // Continues because we don't want the code after this to touch a chest that is no longer accessible.
                        continue;
                    }

                    // Selects a random item to be placed in a chest.
                    (int, IChestItem) typeAndChestItem = Main.rand.Next(chestItems);

                    IChestItem chestItem = typeAndChestItem.Item2;

                    // Type check is to prevent dungeon wooden chests being treated as surface ones.
                    if (chest.item[0].type != ItemID.GoldenKey && TileMatchesRegionFlags(chestItem.Regions, tile) && WorldGen.genRand.NextFloat() < chanceToReplaceMainChestLootWithModdedItem)
                    {
                        // Replaces the "main" chest item. I'm assuming this is always in slot 0.
                        chest.item[0] = SetupItem(typeAndChestItem.Item1, chestItem.Stack, false);
                    }
                }
            }

            chestItems.Clear();
        }

        private void PlaceDisplayCaseOn(Chest chest)
        {
            int type = ItemID.None;

            for (int i = 0; i < chest.item.Length; i++)
            {
                Item item = chest.item[i];

                // Checks if the "main" chest item is replaceable (weapon or accessory, and not stackable).
                if (item.accessory || (item.damage > 0 && item.notAmmo && (item.melee || item.ranged || item.magic || item.summon) && item.maxStack == 1))
                {
                    type = chest.item[i].type;

                    break;
                }
            }

            if (type != ItemID.None)
            {
                Item item = SetupItem(type, 1, true);
            }
        }

        private bool IsDisplayCaseReplaceable(short frameX)
            => frameX == 36 || // Gold. 
            frameX == 72 || // Locked Gold.
            frameX == 144 || // Locked Evil.
            frameX == 288 || // Mahogany.
            frameX == 360 || // Ivy.
            frameX == 396 || // Ice.
            frameX == 468; // Sky.

        private Item SetupItem(int type, int stack, bool isRelic)
        {
            Item item = new Item(); // TODO: Come 1.4 we can make this into the constructor that takes an ID.

            item.SetDefaults(type);
            item.stack = stack;
            item.GetGlobalItem<RelicItem>().isRelic = isRelic;
            item.Prefix(ItemLoader.ChoosePrefix(item, Main.rand));

            return item;
        }

        private bool TileMatchesRegionFlags(ChestRegionFlags flags, Tile tile)
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
                    int[] frames = regionsToFraming[flag];

                    foreach (int frame in frames)
                    {
                        if (tile.frameX == frame)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}