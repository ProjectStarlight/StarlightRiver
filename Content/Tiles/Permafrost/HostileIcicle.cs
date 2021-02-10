
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

using StarlightRiver.Core;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    public class HostileIcicle : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Permafrost/HostileIcicle";
            return true;
        }

        public override bool Dangersense(int i, int j, Player player) => true;

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

    public class HostileIcicleProj : ModProjectile
    {
        public override string Texture => AssetDirectory.PermafrostTile + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Icicle");

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 8;
            projectile.height = 3;
            projectile.penetrate = 1;
            projectile.timeLeft = 180;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.damage = 5;
        }

        public override void AI()
        {
            if (projectile.ai[0] < 45)
            {
                projectile.ai[0]++;

                if (projectile.ai[0] < 12)
                    Dust.NewDust(projectile.position, projectile.width, 3, 80, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(1, 2));

                if (projectile.ai[0] == 44)
                    projectile.velocity.Y = 4;

                projectile.rotation += Main.rand.NextFloat(-0.05f, 0.05f);
                projectile.rotation = MathHelper.Clamp(projectile.rotation, -.35f, .35f);
            }
            else
            {
                projectile.rotation = 0;
                projectile.height = 16;
                projectile.velocity.Y += 0.2f;
                projectile.tileCollide = true;
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(2, (int)projectile.position.X, (int)projectile.position.Y, 27);

            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 80);
            }
        }
    }
}