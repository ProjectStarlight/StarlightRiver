using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Utility
{
    class ArmorBag : ModItem
    {
        public Item[] storedArmor = new Item[3];

        public override bool CloneNewInstances => true;

        public override bool CanRightClick() => true;

        public override string Texture => "StarlightRiver/Assets/Items/Utility/ArmorBag";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Armor Bag");
            Tooltip.SetDefault("Stores armor for quick use\nContains:");
        }

        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.rare = ItemRarityID.Blue;
        }

        public override ModItem Clone()
        {
            var newBag = (ArmorBag)MemberwiseClone();

            newBag.storedArmor = new Item[3];

            for (int k = 0; k < 3; k++)
            {
                newBag.storedArmor[k] = storedArmor[k]?.Clone();
            }

            if (newBag.storedArmor[0] is null || newBag.storedArmor[1] is null || newBag.storedArmor[2] is null)
            {
                for (int k = 0; k < 3; k++)
                {
                    var item = new Item();
                    item.TurnToAir();
                    newBag.storedArmor[k] = item;
                }
            }

            return newBag;
        }

        public override void RightClick(Player player)
        {
            item.stack++;

            Item mouseItem = Main.mouseItem;

            if (mouseItem.IsAir)
            {
                if(player.controlSmart)
                {
                    for(int k = 0; k < 3; k++)
                    {
                        if (storedArmor[k].IsAir)
                            continue;

                        var index = Item.NewItem(player.Center, storedArmor[k].type);
                        Main.item[index] = storedArmor[k].Clone();
                        storedArmor[k].TurnToAir();
                    }
                    return;
                }

                for (int k = 0; k < 3; k++)
                {
                    var temp = player.armor[k];
                    player.armor[k] = storedArmor[k];
                    storedArmor[k] = temp;
                }
                return;
            }

            if (mouseItem.headSlot != -1)
            {
                var temp = storedArmor[0];
                storedArmor[0] = mouseItem;
                Main.mouseItem = temp;
            }

            if (mouseItem.bodySlot != -1)
            {
                var temp = storedArmor[1];
                storedArmor[1] = mouseItem;
                Main.mouseItem = temp;
            }

            if (mouseItem.legSlot != -1)
            {
                var temp = storedArmor[2];
                storedArmor[2] = mouseItem;
                Main.mouseItem = temp;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (storedArmor[0] is null || storedArmor[1] is null || storedArmor[2] is null)
                return;

            TooltipLine armorLineHead = new TooltipLine(mod, "HelmetSlot",
                (storedArmor[0].IsAir) ? "No helmet" : storedArmor[0].Name)
            {
                overrideColor = (storedArmor[0].IsAir) ? new Color(150, 150, 150) : ItemRarity.GetColor(storedArmor[0].rare)
            };
            tooltips.Add(armorLineHead);

            TooltipLine armorLineChest = new TooltipLine(mod, "ChestSlot",
                (storedArmor[1].IsAir) ? "No chestplate" : storedArmor[1].Name)
            {
                overrideColor = (storedArmor[1].IsAir) ? new Color(150, 150, 150) : ItemRarity.GetColor(storedArmor[1].rare)
            };
            tooltips.Add(armorLineChest);

            TooltipLine armorLineLegs = new TooltipLine(mod, "LegsSlot",
                (storedArmor[2].IsAir) ? "No leggings" : storedArmor[2].Name)
            {
                overrideColor = (storedArmor[2].IsAir) ? new Color(150, 150, 150) : ItemRarity.GetColor(storedArmor[2].rare)
            };
            tooltips.Add(armorLineLegs);

            TooltipLine line = new TooltipLine(mod, "Starlight", 
                "Right click to equip stored armor\n" +
                "Right click with armor to add it to the bag\n" +
                "Shift-Right click to empty the bag");

            tooltips.Add(line);
        }

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                ["Head"] = storedArmor[0],
                ["Chest"] = storedArmor[1],
                ["Legs"] = storedArmor[2]
            };
        }

        public override void Load(TagCompound tag)
        {
            storedArmor[0] = tag.Get<Item>("Head");
            storedArmor[1] = tag.Get<Item>("Chest");
            storedArmor[2] = tag.Get<Item>("Legs");
        }
    }
}
