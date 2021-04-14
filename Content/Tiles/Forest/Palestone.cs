using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Forest
{
    internal class PalestoneItem : QuickTileItem
    {
        public PalestoneItem() : base("Palestone", "", TileType<Palestone>(), 0, AssetDirectory.ForestTile) { }
    }

    internal class Palestone : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.ForestTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileStone[Type] = true;
            soundType = Terraria.ID.SoundID.Tink;

            dustType = Terraria.ID.DustID.Stone;
            drop = ItemType<PalestoneItem>();

            AddMapEntry(new Color(167, 180, 191));
        }
    }
}