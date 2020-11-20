using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class ShakerBall : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.friendly = false;
            projectile.width = 64;
            projectile.height = 64;
            projectile.penetrate = -1;
            projectile.timeLeft = 2;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.melee = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shaker");
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            if (projectile.timeLeft < 2) projectile.timeLeft = 2;
            projectile.scale = projectile.ai[0] < 10 ? (projectile.ai[0] / 10f) : 1;
            projectile.damage = (int)(projectile.ai[0] * 1.2f * player.meleeDamage);

            if (projectile.ai[0] == 100)
            {
                Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Gold2>(), Vector2.One.RotatedByRandom(6.28f));
            }

            if (projectile.ai[1] == 0)
            {
                projectile.position = player.position + new Vector2(-15, -64);
            }

            if (projectile.ai[1] == 0 && projectile.ai[0] < 100)
            {
                projectile.ai[0]++;
                if (projectile.ai[0] == 100)
                {
                    for (int k = 0; k <= 100; k++)
                    {
                        Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Gold2>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.5f);
                    }
                    Main.PlaySound(SoundID.NPCDeath7, projectile.Center);
                }
                projectile.velocity = Vector2.Zero;
                float rot = Main.rand.NextFloat(6.28f);
                Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot) * 35, DustType<Dusts.Gold2>(), -Vector2.One.RotatedBy(rot) * 1.5f, 0, default, projectile.ai[0] / 100f);
            }

            if (!player.channel && projectile.ai[0] > 10 && projectile.ai[1] == 0)
            {
                projectile.velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * projectile.ai[0] * 0.1f;
                projectile.tileCollide = true;
                projectile.friendly = true;
                projectile.ai[1] = 1;
            }

            if (projectile.ai[1] == 1)
            {
                projectile.velocity.Y += 0.4f;

                if (projectile.velocity.Y == 0.4f)
                {
                    projectile.velocity *= 0;
                    projectile.timeLeft = 120;
                    projectile.ai[1] = 2;

                    player.GetModPlayer<StarlightPlayer>().Shake += (int)(projectile.ai[0] * 0.2f);
                    for (int k = 0; k <= 100; k++)
                    {
                        Dust.NewDustPerfect(projectile.Center + new Vector2(0, 32), DustType<Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * projectile.ai[0] / 10f);
                    }
                    Main.PlaySound(SoundID.Item70, projectile.Center);
                    Main.PlaySound(SoundID.NPCHit42, projectile.Center);
                }
            }

            if (projectile.ai[1] == 2)
            {
                projectile.velocity += -Vector2.Normalize(projectile.Center - player.Center) * 0.1f;
                if (projectile.velocity.Length() >= 5) projectile.ai[1] = 3;

                if (Vector2.Distance(projectile.Center, player.Center) <= 30) projectile.timeLeft = 0;
                if (projectile.timeLeft == 3) projectile.ai[1] = 4;
            }

            if (projectile.ai[1] == 3)
            {
                projectile.velocity = -Vector2.Normalize(projectile.Center - player.Center) * 5;
                projectile.velocity.Y += 3;

                if (Vector2.Distance(projectile.Center, player.Center) <= 30) projectile.timeLeft = 0;
                if (projectile.timeLeft == 3) projectile.ai[1] = 4;
            }

            if (projectile.ai[1] == 4)
            {
                projectile.velocity = -Vector2.Normalize(projectile.Center - player.Center) * 18;
                projectile.tileCollide = false;

                if (Vector2.Distance(projectile.Center, player.Center) <= 30) projectile.timeLeft = 0;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[1] != 0)
            {
                Player player = Main.player[projectile.owner];
                for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(player.Center, projectile.Center) / 16))
                {
                    spriteBatch.Draw(GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/ShakerChain"), Vector2.Lerp(projectile.Center, player.Center, k) - Main.screenPosition,
                        new Rectangle(0, 0, 8, 16), lightColor, (projectile.Center - player.Center).ToRotation() + 1.58f, new Vector2(4, 8), 1, 0, 0);
                }
            }
            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[1] == 0)
            {
                float colormult = projectile.ai[0] / 100f * 0.7f;
                float scale = 1.2f - projectile.ai[0] / 100f * 0.5f;
                Texture2D tex = GetTexture("StarlightRiver/Tiles/Interactive/WispSwitchGlow2");
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), Color.LightYellow * colormult, 0, tex.Size() / 2, scale, 0, 0);
            }

            if (projectile.ai[0] == 100)
            {
                Texture2D tex = GetTexture("StarlightRiver/Tiles/Interactive/WispSwitchGlow2");
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), Color.LightYellow * (6.28f - StarlightWorld.rottime) * 0.2f, 0, tex.Size() / 2, StarlightWorld.rottime * 0.17f, 0, 0);
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.rottime + 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.rottime + 3.14f) * 0.17f, 0, 0);
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.rottime - 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.rottime - 3.14f) * 0.17f, 0, 0);
            }
        }
    }
}