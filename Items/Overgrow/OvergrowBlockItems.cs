using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Overgrow
{
    public class HatchOvergrowItem : QuickTileItem { public HatchOvergrowItem() : base("Skyview Vent", "", TileType<Tiles.Overgrow.HatchOvergrow>(), 0) { } }
    public class DartOvergrowItem : QuickTileItem { public DartOvergrowItem() : base("Overgrow Dart Trap", "", TileType<Tiles.Overgrow.DartTile>(), 0) { } }
    public class CrusherOvergrowItem : QuickTileItem { public CrusherOvergrowItem() : base("Crusher Trap", "", TileType<Tiles.Overgrow.CrusherTile>(), 0) { } }
    public class SetpieceOvergrowItem : QuickTileItem
    {
        public SetpieceOvergrowItem() : base("Overgrow Altar", "", TileType<Tiles.Overgrow.SetpieceAltar>(), 0) { }
        public override string Texture => "StarlightRiver/MarioCumming";
    }
    public class BigHatchOvergrowItem : QuickTileItem
    {
        public BigHatchOvergrowItem() : base("Overgrow Godrays", "", TileType<Tiles.Overgrow.BigHatchOvergrow>(), 0) { }
        public override string Texture => "StarlightRiver/MarioCumming";
    }
}