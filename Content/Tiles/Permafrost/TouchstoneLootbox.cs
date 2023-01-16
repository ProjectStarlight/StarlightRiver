using Microsoft.Xna.Framework;
using StarlightRiver.Content.Tiles;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class TouchstoneLootbox : LootChest
    {
        public override string Texture => AssetDirectory.PermafrostTile + Name;

        internal override List<Loot> GoldLootPool
        {
            get => new List<Loot>
            {
                new Loot(ItemType<Content.Items.UndergroundTemple.TemplePick>(), 1),
                new Loot(ItemType<Content.Items.UndergroundTemple.TempleSpear>(), 1),
                new Loot(ItemType<Content.Items.UndergroundTemple.TempleRune>(), 1)
            };
        }

        internal override List<Loot> SmallLootPool
        {
            get => new List<Loot>
            {
                new Loot(ItemID.HealingPotion, 4, 8),
                new Loot(ItemID.ManaPotion, 3, 6),
                new Loot(ItemID.FrostburnArrow, 40, 60),
                new Loot(ItemID.LifeCrystal, 1, 1),
                new Loot(ItemID.ManaCrystal, 2, 2),
                new Loot(ItemID.PlatinumBar, 10, 20),
                new Loot(ItemID.Diamond, 1, 1),
                new Loot(ItemID.IceBlock, 200, 800),
                new Loot(ItemType<AuroraIceBar>(), 2, 5)
            };
        }

        public override void SafeSetDefaults() => QuickBlock.QuickSetFurniture(this, 2, 2, DustID.GoldCoin, SoundID.Tink, false, new Color(151, 151, 151));
    }

    class TouchstoneLootboxItem : QuickTileItem
    {
        public TouchstoneLootboxItem() : base("Touchstone Chest Placer", "", "TouchstoneLootbox", 0, AssetDirectory.PermafrostTile) { }
    }
}
