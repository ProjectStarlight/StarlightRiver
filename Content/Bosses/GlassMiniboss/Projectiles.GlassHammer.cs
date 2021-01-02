using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class GlassHammer : ModProjectile
    {
        Vector2 origin;

        public override string Texture => "StarlightRiver/Assets/Bosses/GlassMiniboss/GlassHammer";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Hammer");

        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.hostile = true;
            projectile.timeLeft = 60;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 60)
            {
                origin = projectile.Center; //sets origin when spawned
                Projectile.NewProjectile(projectile.Center, Vector2.UnitX * 10, ProjectileType<Shockwave>(), 22, 0, Main.myPlayer); //Shockwave spawners
                Projectile.NewProjectile(projectile.Center, Vector2.UnitX * -10, ProjectileType<Shockwave>(), 22, 0, Main.myPlayer);
            }

            if (projectile.timeLeft >= 30)
            {
                float radius = (60 - projectile.timeLeft) * 2;
                float rotation = -(60 - projectile.timeLeft) / 30f * 0.8f; //ai 0 is direction

                projectile.Center = origin - Vector2.UnitY.RotatedBy(rotation * projectile.ai[0]) * radius;
            }
            else if (projectile.timeLeft >= 1)
            {
                float rotation = -0.8f + (60 - projectile.timeLeft - 30) / 30f * ((float)Math.PI / 2 + 1.2f);

                projectile.Center = origin - Vector2.UnitY.RotatedBy(rotation * projectile.ai[0]) * 120;

                if (projectile.timeLeft == 1)
                {
                    Main.PlaySound(SoundID.Shatter, projectile.Center);
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 15;

                    for (int k = 0; k < 30; k++)
                    {
                        Vector2 vector = Vector2.UnitY.RotatedByRandom((float)Math.PI / 2);
                        Dust.NewDustPerfect(projectile.Center + vector * Main.rand.NextFloat(25), DustType<Dusts.Sand>(), vector * Main.rand.NextFloat(3, 5), 150, Color.White, 0.5f);
                    }

                    GlassMiniboss.SpawnShards(6, projectile.Center);
                }
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (projectile.timeLeft <= 30 && projectile.timeLeft >= 20)
                target.AddBuff(BuffType<Buffs.Squash>(), 180);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle frame = new Rectangle(0, 166 * (int)((60 - projectile.timeLeft) / 40f * 12), 214, 166);
            if (projectile.timeLeft <= 20) frame.Y = 12 * 166;
            spriteBatch.Draw(GetTexture(Texture), origin + new Vector2(-100, -130) - Main.screenPosition, frame, Color.White, 0, Vector2.Zero, 1, projectile.ai[0] == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

            return false;
        }
    }

    class Shockwave : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Shockwave");

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.timeLeft = 150;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void AI()
        {
            Tile tile = Framing.GetTileSafely((int)projectile.Center.X / 16 + (projectile.velocity.X > 0 ? 1 : -1), (int)projectile.Center.Y / 16);
            Main.NewText(tile.type);

            if (projectile.timeLeft < 140 && tile.type == mod.TileType("VitricGlass"))
                projectile.position.Y -= 128;

            if (projectile.timeLeft < 150 && projectile.velocity.Y == 0 && projectile.timeLeft % 10 == 0)
                Projectile.NewProjectile(projectile.Center + Vector2.UnitY * 16, Vector2.Zero, ProjectileType<ShockwaveSpike>(), projectile.damage, 0, projectile.owner);

            if (projectile.velocity.X == 0)
                projectile.Kill();

            projectile.velocity.Y = 50;
        }
    }

    class ShockwaveSpike : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Stone Pillar");

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.hostile = true;
            projectile.timeLeft = 80;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            projectile.ai[0]++; //ticks up the timer

            if (projectile.ai[0] == 1) projectile.ai[1] = projectile.position.Y;

            if (projectile.ai[0] == 50)
            {
                projectile.hostile = true; //when this projectile goes off

                for (int k = 0; k < 20; k++)
                    Dust.NewDustPerfect(projectile.Center + Vector2.UnitY * -4, DustType<Dusts.Stone>(), Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(-4, -2));

                Main.PlaySound(SoundID.Item70, projectile.Center);
            }

            if (projectile.ai[0] >= 50 && projectile.ai[0] <= 55)
            {
                int factor = (int)((projectile.ai[0] - 50) / 5f * 128);

                projectile.position.Y = projectile.ai[1] - factor;
                projectile.height = factor;
            }

            if (projectile.ai[0] >= 55 && projectile.ai[0] <= 80)
            {
                int factor = 128 - (int)((projectile.ai[0] - 55) / 25f * 128);

                projectile.position.Y = projectile.ai[1] - factor;
                projectile.height = factor;
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[0] > 50)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Bosses/GlassMiniboss/Spike");

                int off = projectile.ai[0] < 55 ? (int)((projectile.ai[0] - 50) / 5f * 128) : 128 - (int)((projectile.ai[0] - 55) / 25f * 128);
                Rectangle targetRect = new Rectangle((int)(projectile.position.X - Main.screenPosition.X), (int)(projectile.ai[1] - off - Main.screenPosition.Y), tex.Width, off);
                Rectangle sourceRect = new Rectangle(0, 0, tex.Width, off);
                spriteBatch.Draw(tex, targetRect, sourceRect, lightColor, 0, Vector2.Zero, 0, 0);
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (projectile.ai[0] <= 50)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Bosses/GlassMiniboss/SpikeTell");
                float factor = 2 * projectile.ai[0] / 25f - (float)Math.Pow(projectile.ai[0], 2) / 625f;
                spriteBatch.Draw(tex, projectile.Center + new Vector2(0, -128) - Main.screenPosition, null, new Color(255, 230, 71) * factor);
            }

        }
    }
}
