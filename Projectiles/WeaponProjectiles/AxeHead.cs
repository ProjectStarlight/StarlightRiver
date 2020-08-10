using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
	public class AxeHead : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Executioner's Axe");
		}

		public override void SetDefaults()
		{
			projectile.hostile = false;
			projectile.magic = true;
			projectile.width = 48;
			projectile.height = 48;
			projectile.aiStyle = -1;
			projectile.friendly = false;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
            projectile.alpha = 255;
		}
        bool released = false;
        int chargeTime = 60;
        float angularMomentum = 1;
        double radians = 0;
        int lingerTimer = 0;
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle? sourceRectangle = null;
            Main.spriteBatch.Draw(ModContent.GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/AxeHead"), Main.player[projectile.owner].Center - Main.screenPosition, sourceRectangle, lightColor, (float)radians + 3.9f, new Vector2(0, 84), projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {

            if (projectile.ai[0] >= 60)
            {
                Texture2D tex = GetTexture("StarlightRiver/Tiles/Interactive/WispSwitchGlow2");
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), Color.LightYellow * (6.28f - StarlightWorld.rottime) * 0.2f, 0, tex.Size() / 2, StarlightWorld.rottime * 0.17f, 0, 0);
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.rottime + 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.rottime + 3.14f) * 0.17f, 0, 0);
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.rottime - 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.rottime - 3.14f) * 0.17f, 0, 0);
            }
        }
        private void Smash(Vector2 position)
        {
            Player player = Main.player[projectile.owner];
            player.GetModPlayer<StarlightPlayer>().Shake += (int)(projectile.ai[0] * 0.2f);
            for (int k = 0; k <= 100; k++)
            {
                Dust.NewDustPerfect(projectile.oldPosition + new Vector2(projectile.width / 2, projectile.height / 2), DustType<Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * projectile.ai[0] / 10f);
            }
            Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y - 32, 0, 0, ModContent.ProjectileType<AxeFire>(), projectile.damage / 3, projectile.knockBack / 2, projectile.owner, 15, player.direction);
        }
		public override bool PreAI()
		{
            projectile.scale = projectile.ai[0] < 10 ? (projectile.ai[0] / 10f) : 1;
            Player player = Main.player[projectile.owner];
            int degrees = (int)(((player.itemAnimation) * -0.7) + 55) * player.direction;
            if (player.direction == 1)
            {
                degrees += 180;
            }
            radians = degrees * (Math.PI / 180);
            if (player.channel && !released)
            {
                if (projectile.ai[0] == 0)
                {
                    player.itemTime = 180;
                    player.itemAnimation = 180;
                }
                if (projectile.ai[0] < chargeTime)
                {
                    projectile.ai[0]++;
                    float rot = Main.rand.NextFloat(6.28f);
                    Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot) * 35, DustType<Dusts.Gold2>(), -Vector2.One.RotatedBy(rot) * 1.5f, 0, default, projectile.ai[0] / 100f);
                    if (projectile.ai[0] < chargeTime / 1.5f || projectile.ai[0] % 2 == 0)
                    {
                        angularMomentum = -1;
                    }
                    else
                    {
                        angularMomentum = 0;
                    }
                }
                else
                {
                    if (projectile.ai[0] == chargeTime)
                    {
                        for (int k = 0; k <= 100; k++)
                        {
                            Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Gold2>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.5f);
                        }
                        Main.PlaySound(SoundID.NPCDeath7, projectile.Center);
                        projectile.ai[0]++;
                    }
                    Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Gold2>(), Vector2.One.RotatedByRandom(6.28f));
                    angularMomentum = 0;
                }
                projectile.damage = 20 + (int)projectile.ai[0];
            }
            else
            {
                projectile.scale = 1;
                if (angularMomentum < 10)
                {
                    angularMomentum += 1.2f;
                }
                if (!released)
                {
                    released = true;
                    projectile.friendly = true;
                }
                if (projectile.ai[0] > chargeTime)
                {
                    Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Gold2>(), Vector2.One.RotatedByRandom(6.28f));
                }
            }

            projectile.position.Y = player.Center.Y - (int)(Math.Sin(radians * 0.96) * 86) - (projectile.height / 2);
            projectile.position.X = player.Center.X - (int)(Math.Cos(radians * 0.96) * 86) - (projectile.width / 2);
            if (lingerTimer == 0)
            {
                player.itemTime++;
                player.itemAnimation++;
                if (player.itemTime > angularMomentum + 1)
                {
                    player.itemTime -= (int)angularMomentum;
                    player.itemAnimation -= (int)angularMomentum;
                }
                else
                {
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                }
                if (player.itemTime == 2 || (Main.tile[(int)projectile.Center.X / 16, (int)((projectile.Center.Y + 24) / 16)].collisionType == 1 && released))
                {
                    lingerTimer = 30;
                    if (projectile.ai[0] >= chargeTime)
                    {
                        this.Smash(projectile.Center);

                    }
                    projectile.damage = (int)projectile.damage / 3;
                    Main.PlaySound(SoundID.Item70, projectile.Center);
                    Main.PlaySound(SoundID.NPCHit42, projectile.Center);
                }
            }
            else
            {
                lingerTimer--;
                if (lingerTimer == 1)
                {
                    projectile.active = false;
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                }
                player.itemTime++;
                player.itemAnimation++;
            }
            return true;
		}
	}
}
