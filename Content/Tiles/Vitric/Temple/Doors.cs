using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
    class DoorVertical : ModTile
    {
        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(1, 7, DustType<Content.Dusts.Air>(), SoundID.Tink, false, new Color(200, 180, 100), false, true);
        }
    }

    class DoorVerticalItem : QuickTileItem
    {
        public override string Texture => Directory.Debug;

        public DoorVerticalItem() : base("Vertical Temple Door", "CUM IN ME DADDY OH YES YES YES", TileType<DoorVertical>(), ItemRarityID.Blue) { }
    }

    class DoorHorizontal : ModTile
    {
        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(7, 1, DustType<Content.Dusts.Air>(), SoundID.Tink, false, new Color(200, 180, 100), false, true);
        }
    }

    class DoorHorizontalItem : QuickTileItem
    {
        public override string Texture => Directory.Debug;

        public DoorHorizontalItem() : base("Horizontal Temple Door", "CUM IN ME DADDY OH YES YES YES", TileType<DoorHorizontal>(), ItemRarityID.Blue) { }
    }
}
