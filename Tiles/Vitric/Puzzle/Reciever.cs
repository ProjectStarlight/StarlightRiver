using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric.Puzzle
{
    class RecieverPuzzle : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 2, 3, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(0, 255, 255), false, true, "Reciever");
    }

    class RecieverPlacable : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(0, 255, 255), false, true, "Reciever");
    }

    class RecieverItem : QuickTileItem
    {
        public RecieverItem() : base("Light Reciever", "", TileType<RecieverPlacable>(), 0) { }
    }

    class RecieverItem2 : QuickTileItem
    {
        public RecieverItem2() : base("Debug Puzzle Reciever", "", TileType<RecieverPuzzle>(), 0) { }
    }
}
