using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Items.Herbology.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Hell
{
	class HellChest : LootChest
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Hell/HellChest";

        internal override List<Loot> GoldLootPool
        {
            get => new List<Loot>
            {
                new Loot(ItemType<Items.Hell.MagmaSword>(), 1)
            };
        }

        internal override List<Loot> SmallLootPool
        {
            get => new List<Loot>
            {
                new Loot(ItemID.LesserHealingPotion, 4, 8),
                new Loot(ItemID.LesserManaPotion, 3, 6),
                new Loot(ItemID.JestersArrow, 40, 60),
                new Loot(ItemID.SilverBullet, 20, 30),
                new Loot(ItemID.Dynamite, 2, 4),
                new Loot(ItemID.SpelunkerGlowstick, 15),
                new Loot(ItemType<IvySeeds>(), 5, 8)
            };
        }

        public override void SafeSetDefaults() => this.QuickSetFurniture(2, 2, DustID.AmberBolt, SoundID.Unlock, false, new Color(255, 255, 155));

        public override bool CanOpen(Player Player)
        {
            for (int k = 0; k < Player.inventory.Length; k++)
            {
                Item Item = Player.inventory[k];
                if (Item.type == ItemType<Items.Hell.HellKey>())
                {
                    if (Item.stack > 1) Item.stack--;
                    else Item.TurnToAir();
                    return true;
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Player Player = Main.LocalPlayer;
            Player.noThrow = 2;
            Player.cursorItemIconEnabled = true;
            Player.cursorItemIconID = ItemType<Items.Hell.HellKey>();
        }

    }
}
