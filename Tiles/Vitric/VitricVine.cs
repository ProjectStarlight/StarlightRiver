using Microsoft.Xna.Framework;
using StarlightRiver.Tiles.Vitric.Blocks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric
{
    internal class VitricVine : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileCut[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileType<VitricSand>(), Type };
            drop = 0;
            AddMapEntry(new Color(199, 224, 190));
            dustType = DustType<Dusts.Air>();
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 3;
        }

        public override void RandomUpdate(int i, int j)
        {
            if (!Main.tile[i, j + 1].active() && Main.rand.Next(10) == 0)
                WorldGen.PlaceTile(i, j + 1, Type);
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (Main.tile[i, j + 1].type == Type)
                WorldGen.KillTile(i, j + 1, false, false, true);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!Main.tile[i, j - 1].active()) WorldGen.KillTile(i, j);
        }
    }
}