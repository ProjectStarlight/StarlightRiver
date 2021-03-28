using StarlightRiver.Items;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Misc;
using Terraria.DataStructures;
using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver
{
    internal class ChestLootData
    {
        internal int type;
        internal Func<Chest, int> stack;
        internal Func<Chest, bool> condition;

        internal ChestLootData(int type, Func<Chest, int> stack, Func<Chest, bool> condition)
        {
            this.type = type;
            this.stack = stack;
            this.condition = condition;
        }

    }

    public partial class StarlightRiver
    {
        static internal List<ChestLootData> ChestLoots;

        static public void AddToGeneratedChests(int type, Func<Chest, int> stack, Func<Chest, bool> condition)
        {
            ChestLoots.Add(new ChestLootData(type, stack, condition));
        }

        static public void InitWorldGenChests()
        {
            ChestLoots = new List<ChestLootData>();

            for (int i = 0; i < ItemLoader.ItemCount; i += 1)
            {
                ModItem modItem = ItemLoader.GetItem(i);
                if (modItem != null && modItem is IChestItem iChestItem)
                {
                    AddToGeneratedChests(modItem.item.type, iChestItem.ItemStack, iChestItem.GenerateCondition);
                    Instance.Logger.Warn("Chestloot: " + modItem.item.type + " found");
                }
            }
            Instance.Logger.Warn("Chestloot: Size of list is " + ChestLoots.Count);
        }

        static public void FIllChests()
        {

            for (int index = 0; index < 1000; index++)
            {
                Chest chest = Main.chest[index];

                if (chest is null)
                    continue;

                var tile = Framing.GetTileSafely(chest.x, chest.y);

                if (!tile.active())
                    continue;

                //Display case replacement possibility
                if (WorldGen.genRand.Next(8) == 0 && ChestFrameWhitelist(tile.frameX))
                {
                    int type = 0;

                    for (int k = 0; k < chest.item.Length; k++)
                    {
                        var item = chest.item[k];

                        if (item.accessory || (item.damage > 0 && item.notAmmo && (item.melee || item.ranged || item.magic || item.summon) && item.maxStack == 1)) //might need more checks?
                        {
                            type = chest.item[k].type;
                            break;
                        }
                    }

                    if (type != 0)
                    {
                        Item item = new Item();
                        item.SetDefaults(type);
                        item.GetGlobalItem<RelicItem>().isRelic = true;
                        item.Prefix(ItemLoader.ChoosePrefix(item, Main.rand));

                        Helpers.Helper.PlaceMultitile(new Point16(chest.x, chest.y - 1), ModContent.TileType<DisplayCase>());
                        TileEntity.PlaceEntityNet(chest.x, chest.y - 1, ModContent.TileEntityType<DisplayCaseEntity>());
                        (TileEntity.ByPosition[new Point16(chest.x, chest.y - 1)] as DisplayCaseEntity).containedItem = item;
                    }
                }

                foreach (ChestLootData lootindachest in ChestLoots)//Cycle all, again
                {
                    if (lootindachest.condition(chest))
                    {

                        int item = lootindachest.type;
                        int stack = lootindachest.stack(chest);

                        for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                        {
                            if (chest.item[inventoryIndex].IsAir)
                            {
                                chest.item[inventoryIndex].SetDefaults(item);
                                chest.item[inventoryIndex].stack = stack;
                                break;
                            }
                        }
                    }
                }
            }

        }

        private static bool ChestFrameWhitelist(short frameX)
        {
            if (frameX == 36 || //gold
                frameX == 72 || //lockedGold
                frameX == 144 || //lockedDemonite
                frameX == 288 || //mahogany
                frameX == 360 || //ivy
                frameX == 396 || //ice
                frameX == 468) //sky
                return true;

            return false;
        }
    }
}