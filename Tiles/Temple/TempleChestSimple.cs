using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using StarlightRiver.Items.Herbology.Materials;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Temple
{
    class TempleChestSimple : LootChest
    {
        internal override List<Loot> GoldLootPool
        {
            get => new List<Loot>
            {
                new Loot(ItemType<Items.Temple.TemplePick>(), 1),
                new Loot(ItemType<Items.Temple.TempleSpear>(), 1),
                new Loot(ItemType<Items.Temple.TempleRune>(), 1),
                new Loot(ItemType<Items.Temple.TempleLens>(), 1)
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
                new Loot(ItemType<IvySeeds>(), 4, 8)
            };
        }

        public override void SafeSetDefaults() => QuickBlock.QuickSetFurniture(this, 2, 2, DustID.GoldCoin, SoundID.Tink, false, new Color(151, 151, 151));
    }
}
