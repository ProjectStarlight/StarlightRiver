using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Forest
{
    internal class PalestoneItem : Items.QuickTileItem
    {
        public PalestoneItem() : base("Palestone", "", TileType<Palestone>(), 0)
        {
        }
    }

    internal class Palestone : ModTile
    {
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