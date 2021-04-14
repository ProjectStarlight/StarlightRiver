using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles;

namespace StarlightRiver.Tiles.Temple
{
    class TempleLootBubble : LootBubble
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.UndergroundTempleTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override List<Loot> GoldLootPool
        {
            get => new List<Loot>
            {
                new Loot(1, 1),
                new Loot(2, 1),
                new Loot(3, 1),
                new Loot(4, 1),
                new Loot(5, 1),
                new Loot(6, 1),
                new Loot(7, 1),
                new Loot(8, 1),
                new Loot(9, 1),
                new Loot(10, 1),
                new Loot(11, 1),
                new Loot(12, 1),
                new Loot(13, 1),
                new Loot(14, 1),
                new Loot(15, 1),
                new Loot(16, 1),
                new Loot(17, 1),
                new Loot(18, 1)
            };
        }

        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            QuickBlock.QuickSetFurniture(this, 2, 2, DustType<Content.Dusts.BlueStamina>(), SoundID.Drown, false, new Color(151, 151, 151));
        }
    }

    class TestBubble : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Bubble";

        public TestBubble() : base("Bubble", "ngh", TileType<TempleLootBubble>(), 5, AssetDirectory.UndergroundTempleTile) { }
    }

}
