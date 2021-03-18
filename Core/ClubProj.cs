using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    public abstract class ClubProj : ModProjectile
    {
        public readonly int chargeTime;
        private readonly int minDamage;
        private readonly int maxDamage;
        private readonly int dustType;
        private readonly int Size;
        private readonly int minKnockback;
        private readonly int maxKnockback;

        private readonly float Acceleration;
        private readonly float MaxSpeed;
        public ClubProj(int chargetime, int mindamage, int maxdamage, int dusttype, int size, int minknockback, int maxknockback, float acceleration, float maxspeed)
        {
            chargeTime = chargetime;
            minDamage = mindamage;
            maxDamage = maxdamage;
            dustType = dusttype;
            Size = size;
            minKnockback = minknockback;
            maxKnockback = maxknockback;
            Acceleration = acceleration;
            MaxSpeed = maxspeed;
        }

        public virtual void SafeAI() { }
        public virtual void SafeDraw(SpriteBatch spriteBatch, Color lightColor) { }

        public virtual void SafeSetDefaults() { }
        public sealed override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.melee = true;
            projectile.width = projectile.height = 48;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.alpha = 255;
            SafeSetDefaults();
        }

        public bool released = false;
        float angularMomentum = 1;
        public double radians = 0;
        int lingerTimer = 0;
        int flickerTime = 0;

        public sealed override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color color = lightColor;
            Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], ((Main.player[projectile.owner].Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY)).PointAccur(), new Rectangle(0, 0, Size, Size), color, (float)radians + 3.9f, new Vector2(0, Size), projectile.scale, SpriteEffects.None, 0);
            SafeDraw(spriteBatch, lightColor);
            if (projectile.ai[0] >= chargeTime && !released && flickerTime < 16)
            {
                flickerTime++;
                color = Color.White;
                float flickerTime2 = (float)(flickerTime / 20f);
                float alpha = 1.5f - (((flickerTime2 * flickerTime2) / 2) + (2f * flickerTime2));
                if (alpha < 0)
                    alpha = 0;
                Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], ((Main.player[projectile.owner].Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY)), new Rectangle(0, Size, Size, Size), color * alpha, (float)radians + 3.9f, new Vector2(0, Size), projectile.scale, SpriteEffects.None, 1);
            }
            return false;
        }

        public virtual void Smash(Vector2 position)
        {

        }

        public sealed override bool PreAI()
        {
            SafeAI();
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
                    if (dustType != -1)
                        Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot) * 35, dustType, -Vector2.One.RotatedBy(rot) * 1.5f, 0, default, projectile.ai[0] / 100f);
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
                            if (dustType != -1)
                                Dust.NewDustPerfect(projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.5f);
                        }
                        Main.PlaySound(SoundID.NPCDeath7, projectile.Center);
                        projectile.ai[0]++;
                    }
                    if (dustType != -1)
                        Dust.NewDustPerfect(projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f));
                    angularMomentum = 0;
                }
                projectile.damage = (int)((minDamage + (int)((projectile.ai[0] / chargeTime) * (maxDamage - minDamage))) * player.meleeDamage);
                projectile.knockBack = minKnockback + (int)((projectile.ai[0] / chargeTime) * (maxKnockback - minKnockback));
            }
            else
            {
                projectile.scale = 1;
                if (angularMomentum < MaxSpeed)
                {
                    angularMomentum += Acceleration;
                }
                if (!released)
                {
                    released = true;
                    projectile.friendly = true;
                    Main.PlaySound(SoundID.Item1, projectile.Center);
                }
                if (projectile.ai[0] > chargeTime)
                {
                    //  Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Gold2>(), Vector2.One.RotatedByRandom(6.28f));
                }
            }

            projectile.position.Y = player.Center.Y - (int)(Math.Sin(radians * 0.96) * Size) - (projectile.height / 2);
            projectile.position.X = player.Center.X - (int)(Math.Cos(radians * 0.96) * Size) - (projectile.width / 2);
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
                    if (Main.tile[(int)projectile.Center.X / 16, (int)((projectile.Center.Y + 24) / 16)].collisionType == 1)
                    {
                        player.GetModPlayer<StarlightPlayer>().Shake += (int)(projectile.ai[0] * 0.2f);
                    }
                    projectile.friendly = false;
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
