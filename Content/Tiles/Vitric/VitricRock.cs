using Microsoft.Xna.Framework;
using StarlightRiver.Tiles.Vitric.Blocks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric
{
    internal class VitricRock : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.RandomStyleRange = 6;
            TileObjectData.newTile.AnchorAlternateTiles = new int[] { TileType<VitricSand>(), TileType<VitricSoftSand>() };
            TileObjectData.addTile(Type);
            soundType = SoundID.Shatter;
            dustType = DustType<Dusts.Glass2>();
            AddMapEntry(new Color(114, 78, 80));
        }
    }
}