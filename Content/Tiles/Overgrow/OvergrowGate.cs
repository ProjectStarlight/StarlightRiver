using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Overgrow
{
    internal class OvergrowGate : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = true;

            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 7;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(0, 0);

            TileObjectData.addTile(Type);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].frameX == 0 && Main.tile[i, j].frameY == 0)
            {
                if (!Main.projectile.Any(proj => proj.modProjectile is Projectiles.Dummies.OvergrowGateDummy && proj.active))
                {
                    Projectile.NewProjectile(new Vector2(i + 1, j + 3.5f) * 16, Vector2.Zero, ProjectileType<Projectiles.Dummies.OvergrowGateDummy>(), 1, 1);
                }
            }
        }
    }
}