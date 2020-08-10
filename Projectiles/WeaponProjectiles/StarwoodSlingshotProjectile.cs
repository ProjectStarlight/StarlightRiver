using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    class StarwoodSlingshotProjectile : ModProjectile, IDrawAdditive
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shooting Star");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;   
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        //These stats get scaled when empowered
        private float ScaleMult = 1;
        //private Color glowColor = new Color(255, 220, 200, 150);
        private Vector3 lightColor = new Vector3(0.2f, 0.1f, 0.05f);
        private int dustType = ModContent.DustType<Dusts.Stamina>();
        private bool empowered;


        public override void SetDefaults()
        {
            projectile.timeLeft = 600;

            projectile.width = 22;
            projectile.height = 24;
            projectile.friendly = true;
            projectile.penetrate = 2;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
            projectile.aiStyle = -1;
            projectile.rotation = Main.rand.NextFloat(4f);
        }


        public override void AI()
        {
            if (projectile.timeLeft == 600)
            {
                StarlightPlayer mp = Main.player[projectile.owner].GetModPlayer<StarlightPlayer>();
                if (mp.Empowered)
                {
                    projectile.frame = 1;
                    //glowColor = new Color(220, 200, 255, 150);
                    lightColor = new Vector3(0.05f, 0.1f, 0.2f);
                    ScaleMult = 1.5f;
                    dustType = ModContent.DustType<Dusts.BlueStamina>();
                    projectile.velocity *= 1.25f;//TODO: This could be on on the item's side like the staff does, thats generally the better way
                    empowered = true;
                }
            }

            if (projectile.timeLeft % 25 == 0)//delay between star sounds
            {
                Main.PlaySound(SoundID.Item9, projectile.Center);
            }

            projectile.rotation += 0.2f;

            Lighting.AddLight(projectile.Center, lightColor);
            if (projectile.velocity.Y < 50)
            {
                projectile.velocity.Y += 0.25f;
            }
            projectile.velocity.X *= 0.995f;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (empowered)
            {
                damage += 5;
                if (projectile.penetrate <= 1)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        Main.NewText(k);
                        Projectile.NewProjectile(projectile.position, projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(0.5f, 0.8f), ModContent.ProjectileType<WeaponProjectiles.StarwoodSlingshotFragment>(), damage / 2, knockback, projectile.owner, Main.rand.Next(2));

                    }
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            DustHelper.DrawStar(projectile.Center, dustType, pointAmount: 5, mainSize: 1.2f * ScaleMult, dustDensity: 0.5f, pointDepthMult: 0.3f);
            Main.PlaySound(SoundID.Item10, projectile.Center);
            for (int k = 0; k < 35; k++)
            {
                Dust.NewDustPerfect(projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f) * (Main.rand.NextFloat(0.25f, 1.2f) * ScaleMult), 0, default, 1.5f);
            }

        }

        //private Texture2D LightTrailTexture => ModContent.GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/glow");
        private Texture2D GlowingTrail => ModContent.GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/StarwoodSlingshotGlowTrail");

        //private static Texture2D MainTexture => ModContent.GetTexture("StarlightRiver/Items/StarwoodBoomerang");
        //private Texture2D GlowingTexture => ModContent.GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/StarwoodSlingshotProjectile");
        //private Texture2D AuraTexture => ModContent.GetTexture("StarlightRiver/Tiles/Interactive/WispSwitchGlow2");

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, new Rectangle(0, empowered ? 24 : 0, 22, 24), Color.White, projectile.rotation, new Vector2(11, 12), projectile.scale, default, default);
            Vector2 drawOrigin = new Vector2(Main.projectileTexture[projectile.type].Width * 0.5f, projectile.height * 0.5f);
            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                Color color = projectile.GetAlpha(Color.White) * (((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length) * 0.5f);
                float scale = projectile.scale * (float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length;

                spriteBatch.Draw(GlowingTrail,
                projectile.oldPos[k] + drawOrigin - Main.screenPosition,
                new Rectangle(0, (Main.projectileTexture[projectile.type].Height / 2) * projectile.frame, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height / 2),
                color,
                projectile.oldRot[k],
                new Vector2(Main.projectileTexture[projectile.type].Width / 2, Main.projectileTexture[projectile.type].Height / 4),
                scale, default, default);

                //spriteBatch.Draw(LightTrailTexture,
                //projectile.oldPos[k] + drawOrigin - Main.screenPosition,
                //LightTrailTexture.Frame(),
                //Color.White * 0.3f,
                //0f,
                //new Vector2(LightTrailTexture.Width / 2, LightTrailTexture.Height / 2),
                //scale, default, default);
            }

            //spriteBatch.Draw(Main.projectileTexture[projectile.type],
            //    projectile.Center - Main.screenPosition,
            //    new Rectangle(0, (Main.projectileTexture[projectile.type].Height / 2) * projectile.frame, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height / 2),
            //    lightColor,
            //    projectile.rotation,
            //    new Vector2(Main.projectileTexture[projectile.type].Width / 2, Main.projectileTexture[projectile.type].Height / 4),
            //    1f, default, default);
            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                Color color = (empowered ? new Color(200, 220, 255) * 0.35f : new Color(255, 255, 200) * 0.3f) * ((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                if (k <= 4) color *= 1.2f;
                float scale = projectile.scale * (float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length * 0.8f;
                Texture2D tex = GetTexture("StarlightRiver/Keys/Glow");

                spriteBatch.Draw(tex, (((projectile.oldPos[k] + projectile.Size / 2) + projectile.Center) * 0.50f) - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
            }
        }
    }

    class StarwoodSlingshotFragment : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Star Fragment");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            projectile.timeLeft = 9;
            projectile.width = 12;
            projectile.height = 10;
            projectile.friendly = true;
            projectile.penetrate = 2;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
            projectile.aiStyle = -1;
            projectile.rotation = Main.rand.NextFloat(4f);
        }
        public override void AI()
        {
            projectile.rotation += 0.3f;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDustPerfect(projectile.position, ModContent.DustType<Dusts.StarFragment>(), projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(0.3f, 0.5f), 0, Color.White, 1.5f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, new Rectangle(0, projectile.ai[0] > 0 ? 10 : 0, 12, 10), Color.White, projectile.rotation, new Vector2(6, 5), projectile.scale, default, default);
            return false;
        }
    }
}
