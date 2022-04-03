using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class DoorVertical : ModTile
    {
        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(1, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 180, 100), false, true);
        }
    }

    class DoorVerticalItem : QuickTileItem
    {
        public DoorVerticalItem() : base("Vertical Temple Door", "Temple Door, But what if it was vertical?", TileType<DoorVertical>(), ItemRarityID.Blue, AssetDirectory.Debug, true) { }
    }

    class DoorHorizontal : ModTile
    {
        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(7, 1, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 180, 100), false, true);
        }
    }

    class DoorHorizontalItem : QuickTileItem
    {
        public DoorHorizontalItem() : base("Horizontal Temple Door", "Temple Door, But what if it was horizontal?", TileType<DoorHorizontal>(), ItemRarityID.Blue, AssetDirectory.Debug, true) { }
    }
}
