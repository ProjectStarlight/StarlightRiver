using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class VitricLootBox : LootChest
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }


        internal override List<Loot> GoldLootPool
        {
            get => new List<Loot>
            {
                new Loot(ItemType<Items.Vitric.VitricBow>(), 1),
                new Loot(ItemType<Items.Vitric.VitricSword>(), 1)
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
                new Loot(ItemType<Items.Vitric.SandstoneChunk>(), 3, 6),
            };
        }

        public override void SafeSetDefaults() => (this).QuickSetFurniture(2, 2, DustID.GoldCoin, SoundID.Tink, false, new Color(151, 151, 151));
    }
}