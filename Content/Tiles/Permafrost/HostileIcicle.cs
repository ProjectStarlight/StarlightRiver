
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Permafrost
{
	public class HostileIcicle : ModTile
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/HostileIcicle";

        public override bool Dangersense(int i, int j, Player Player) => true;

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
            Player Player = Main.LocalPlayer;

            if (Math.Abs(Player.position.X - (i * 16)) < 30 && Player.position.Y > (j * 16) && Player.position.Y - (j * 16) < 600)
            {
                if (Main.rand.NextBool(30) && tile.TileFrameY == 0)
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
            Projectile.hostile = true;
            Projectile.width = 8;
            Projectile.height = 3;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.damage = 5;
        }

        public override void AI()
        {
            if (Projectile.ai[0] < 45)
            {
                Projectile.ai[0]++;

                if (Projectile.ai[0] < 12)
                    Dust.NewDust(Projectile.position, Projectile.width, 3, 80, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(1, 2));

                if (Projectile.ai[0] == 44)
                    Projectile.velocity.Y = 4;

                Projectile.rotation += Main.rand.NextFloat(-0.05f, 0.05f);
                Projectile.rotation = MathHelper.Clamp(Projectile.rotation, -.35f, .35f);
            }
            else
            {
                Projectile.rotation = 0;
                Projectile.height = 16;
                Projectile.velocity.Y += 0.2f;
                Projectile.tileCollide = true;
            }
        }

        public override void Kill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(2, (int)Projectile.position.X, (int)Projectile.position.Y, 27);

            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 80);
            }
        }
    }
}