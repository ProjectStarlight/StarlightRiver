using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Tiles;
using StarlightRiver.Tiles.Interactive;
using StarlightRiver.Tiles.Purified;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.Ability
{
    internal class Purifier : ModProjectile
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 900;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Corona of Purity");
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 900)
            {
                Filters.Scene.Activate("PurityFilter", projectile.position).GetShader().UseDirection(new Vector2(0.1f, 0.1f));
            }
            else if (projectile.timeLeft >= 750)
            {
                projectile.ai[0] += 2;
            }
            else if (projectile.timeLeft < 150)
            {
                projectile.ai[0] -= 2;
            }

            Filters.Scene["PurityFilter"].GetShader().UseProgress((projectile.ai[0] / 255) * 0.125f).UseIntensity((projectile.ai[0] / 255) * 0.006f);

            Dust.NewDust(projectile.Center - Vector2.One * 32, 32, 32, DustType<Dusts.Purify>());

            for (int x = -40; x < 40; x++)
            {
                for (int y = -40; y < 40; y++)
                {
                    Vector2 check = (projectile.position / 16) + new Vector2(x, y);
                    if (Vector2.Distance((check * 16), projectile.Center) <= projectile.ai[0] - 2)
                    {
                        TransformTile((int)check.X, (int)check.Y);
                    }
                    else
                    {
                        RevertTile((int)check.X, (int)check.Y);
                    }

                    //just in case
                    if (projectile.timeLeft == 1)
                    {
                        RevertTile((int)check.X, (int)check.Y);
                    }
                }
            }

            if (projectile.timeLeft == 1)
            {
                for (int k = 0; k <= 50; k++)
                {
                    Dust.NewDustPerfect(projectile.Center - Vector2.One * 8, DustType<Dusts.Purify2>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2.4f), 0, default, 1.2f);
                }
                Projectile.NewProjectile(projectile.Center - Vector2.One * 16, Vector2.Normalize((projectile.Center - Vector2.One * 16) - Main.player[projectile.owner].Center).RotatedBy(0.3f) * 6,
                    ProjectileType<PurifierReturn>(), 0, 0, projectile.owner);

            }
            else if (projectile.timeLeft == 60)
            {
                if (Filters.Scene["PurityFilter"].IsActive())
                {
                    Filters.Scene.Deactivate("PurityFilter");
                }
            }
        }

        public static void TransformTile(int x, int y)
        {
            if (!WorldGen.InWorld(x, y)) return;

            Tile target = Framing.GetTileSafely(x, y);
            {
                if (target.type == TileID.Stone || target.type == TileID.Ebonstone || target.type == TileID.Crimstone || target.type == TileID.Pearlstone) { target.type = (ushort)TileType<StonePure>(); SpawnDust(x, y); }
                if (target.type == TileID.Grass || target.type == TileID.CorruptGrass || target.type == TileID.FleshGrass || target.type == TileID.HallowedGrass) { target.type = (ushort)TileType<GrassPure>(); SpawnDust(x, y); }
                if (target.type == TileID.Sand || target.type == TileID.Ebonsand || target.type == TileID.Crimsand || target.type == TileID.Pearlsand) { target.type = (ushort)TileType<SandPure>(); SpawnDust(x, y); }
                if (target.type == (ushort)TileType<OreEbony>()) { target.type = (ushort)TileType<OreIvory>(); SpawnDust(x, y); }
                if (target.type == (ushort)TileType<VoidDoorOn>()) { target.type = (ushort)TileType<VoidDoorOff>(); } //No Dust.

                //walls
                if (target.wall == WallID.Stone || target.wall == WallID.EbonstoneUnsafe || target.wall == WallID.CrimstoneUnsafe || target.wall == WallID.PearlstoneBrickUnsafe) { target.wall = (ushort)WallType<WallStonePure>(); SpawnDust(x, y); }
                if (target.wall == WallID.GrassUnsafe || target.wall == WallID.CorruptGrassUnsafe || target.wall == WallID.CrimsonGrassUnsafe || target.wall == WallID.HallowedGrassUnsafe) { target.wall = (ushort)WallType<WallGrassPure>(); SpawnDust(x, y); }
            }
        }

        public static void RevertTile(int x, int y)
        {
            if (!WorldGen.InWorld(x, y)) return;

            Tile target = Framing.GetTileSafely(x, y);
            {
                if (target.type == (ushort)TileType<StonePure>()) { target.type = TileID.Stone; SpawnDust(x, y); }
                if (target.type == (ushort)TileType<GrassPure>()) { target.type = TileID.Grass; SpawnDust(x, y); }
                if (target.type == (ushort)TileType<SandPure>()) { target.type = TileID.Sand; SpawnDust(x, y); }
                if (target.type == (ushort)TileType<OreIvory>()) { target.type = (ushort)TileType<OreEbony>(); SpawnDust(x, y); }
                if (target.type == (ushort)TileType<VoidDoorOff>()) { target.type = (ushort)TileType<VoidDoorOn>(); SpawnDust(x, y); }

                //walls
                if (target.wall == WallType<WallStonePure>()) { target.wall = WallID.Stone; SpawnDust(x, y); }
                if (target.wall == WallType<WallGrassPure>()) { target.wall = WallID.GrassUnsafe; SpawnDust(x, y); }
            }
        }

        private static void SpawnDust(int x, int y)
        {
            for (int k = 0; k <= 4; k++)
            {
                Dust.NewDustPerfect(new Vector2(x, y) * 16 + Main.rand.NextVector2Square(-2, 18), DustType<Dusts.Purify2>(), new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-0.1f, -0.5f)), 0, Color.White, 0.5f);
            }
        }

        private readonly Texture2D cirTex = GetTexture("StarlightRiver/Projectiles/Ability/ArcaneCircle");
        private readonly Texture2D cirTex2 = GetTexture("StarlightRiver/Projectiles/Ability/ArcaneCircle2");
        //private readonly Texture2D starTex = ModContent.GetTexture("StarlightRiver/Projectiles/Ability/ArcaneStar");

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(cirTex, projectile.Center - Vector2.One * 16 - Main.screenPosition, cirTex.Frame(), Color.White, -(projectile.timeLeft / 900f), cirTex.Size() / 2, (projectile.ai[0] / cirTex.Width * 2.1f), 0, 0);
            spriteBatch.Draw(cirTex2, projectile.Center - Vector2.One * 16 - Main.screenPosition, cirTex2.Frame(), Color.White, projectile.timeLeft / 900f, cirTex2.Size() / 2, (projectile.ai[0] / cirTex.Width * 2.1f), 0, 0);

            Texture2D tex = GetTexture("StarlightRiver/Pickups/PureCrown");
            spriteBatch.Draw(tex, projectile.Center + new Vector2(-16, -16 + (float)Math.Sin(StarlightWorld.rottime) * 2) - Main.screenPosition, tex.Frame(),
                Color.White * ((projectile.timeLeft < 500) ? 1 : (projectile.ai[0] / 250f)), 0, tex.Size() / 2, 1, 0, 0);

            /*for (float k = 0; k <= 6.28f; k += 0.1f)
            {
                Texture2D tex2 = ModContent.GetTexture("StarlightRiver/Projectiles/Ability/WhiteLine"); //move this outside the for loop lol

                spriteBatch.Draw(tex2, projectile.Center + (Vector2.One * (projectile.ai[0] * 0.72f)).RotatedBy(k) - Vector2.One * 16 - Main.screenPosition, tex2.Frame(),
                    Color.White * (projectile.timeLeft / 600f), k - 1.58f / 2, tex2.Size() / 2, 1, 0, 0);
            }*/
        }
    }

    internal class PurifierReturn : ModProjectile
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 120;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Returning Crown");
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (projectile.timeLeft < 120)
            {
                for (int k = 0; k <= 8; k++)
                {
                    Dust.NewDustPerfect(Vector2.Lerp(projectile.position, projectile.oldPosition, k / 8f), DustType<Dusts.Purify>(), Vector2.Zero, 0, default, 2.4f);
                }
            }

            Vector2 target = player.Center + new Vector2(0, -16);
            projectile.velocity += Vector2.Normalize(projectile.Center - target) * -0.8f;

            if (projectile.velocity.Length() >= 6)
            {
                projectile.velocity = Vector2.Normalize(projectile.velocity) * 6f;
            }

            if (projectile.Hitbox.Intersects(new Rectangle((int)player.Center.X - 2, (int)player.Center.Y - 14, 4, 4)) || projectile.timeLeft == 1)
            {
                for (int k = 0; k <= 50; k++)
                {
                    Dust.NewDustPerfect(player.Center + new Vector2(0, -16), DustType<Dusts.Purify2>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(1.4f));
                }
                for (int k = 0; k <= Vector2.Distance(player.Center + new Vector2(0, -16), projectile.position); k++)
                {
                    Dust.NewDustPerfect(Vector2.Lerp(player.Center + new Vector2(0, -16), projectile.Center, k / Vector2.Distance(player.Center + new Vector2(0, -16), projectile.position))
                        , DustType<Dusts.Purify>());
                }

                projectile.timeLeft = 0;
            }
        }
    }
}