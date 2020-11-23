using StarlightRiver.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    public class HostileIcicle : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = default(AnchorData);
            TileObjectData.addTile(Type);
            dustType = 0;
            adjTiles = new int[] { TileID.Torches };
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Main.tile[i, j];
            Player player = Main.LocalPlayer;
            if (Math.Abs(player.position.X - (i * 16)) < 30 && player.position.Y > (j * 16) && player.position.Y - (j * 16) < 600)
            {
                if (Main.rand.NextBool(30) && tile.frameY == 0)
                {
                    tile.active(false);
                    Main.tile[i, j + 1].active(false);
                    Projectile.NewProjectile((i * 16) + 4, (j * 16), 0, 0, ModContent.ProjectileType<HostileIcicleProj>(), 30, 2);
                }
            }
        }
    }
}