using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Temple
{
    class TempleLootBubble : LootBubble
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

        public override void SafeSetDefaults()
        {
            minPick = int.MaxValue;
            QuickBlock.QuickSetFurniture(this, 2, 2, DustType<Dusts.BlueStamina>(), SoundID.Drown, false, new Color(151, 151, 151));
        }
    }

    class TestBubble : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Tiles/Bubble";

        public TestBubble() : base("Bubble", "ngh", TileType<TempleLootBubble>(), 5) { }
    }

}
