using StarlightRiver.Items;
using StarlightRiver.Structures;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.World.Generation;

namespace StarlightRiver
{
    internal class ChestLootData
    {
        internal int type;
        internal int stack;
        internal Func<Chest, bool> condition;


        internal ChestLootData(int type,int stack, Func<Chest, bool> condition)
        {
            this.type = type;
            this.stack = stack;
            this.condition = condition;
        }

    }

    public partial class StarlightRiver
    {

        static internal List<ChestLootData> ChestLoots;

        static public void AddToGeneratedChests(int type, int stack, Func<Chest, bool> condition)
        {
            ChestLoots.Add(new ChestLootData(type, stack, condition));
        }

        static public void InitWorldGenChests()
        {
            ChestLoots = new List<ChestLootData>();

            for(int i=0; i<ItemLoader.ItemCount; i+=1)
            {
                ModItem modItem = ItemLoader.GetItem(i);
                if (modItem!=null && modItem is IChestItem iChestItem)
                {
                    AddToGeneratedChests(modItem.item.type, iChestItem.ItemStack(), iChestItem.GenerateCondition);
                    Instance.Logger.Warn("Chestloot: " + modItem.item.type + " found");
                }
            }
            Instance.Logger.Warn("Chestloot: Size of list is " + ChestLoots.Count);
        }

        static public void FIllChests()
        {

            foreach (ChestLootData lootindachest in ChestLoots)//Cycle all, again
            {

                for (int chestIndexx = 0; chestIndexx < 1000; chestIndexx++)
                {
                    Chest chest = Main.chest[chestIndexx];
                    if (chest != null && lootindachest.condition(chest))
                    {

                        int item = lootindachest.type;
                        int stack = lootindachest.stack;

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
    }
}