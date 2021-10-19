using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Content.Tiles;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

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
                new Loot(ItemType<RuneStaff>(), 1),
                new Loot(ItemType<TempleLens>(), 1)
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

        public TestBubble() : base("Bubble", "Debug item", TileType<TempleLootBubble>(), 5, AssetDirectory.UndergroundTempleTile) { }
    }

}
