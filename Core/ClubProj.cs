using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.GameContent;
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
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = Projectile.height = 48;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
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
            Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, ((Main.player[Projectile.owner].Center - Main.screenPosition) + new Vector2(0, Main.player[Projectile.owner].gfxOffY)).PointAccur(), new Rectangle(0, 0, Size, Size), color, (float)radians + 3.9f, new Vector2(0, Size), Projectile.scale, SpriteEffects.None, 0);
            SafeDraw(spriteBatch, lightColor);
            if (Projectile.ai[0] >= chargeTime && !released && flickerTime < 16)
            {
                flickerTime++;
                color = Color.White;
                float flickerTime2 = (float)(flickerTime / 20f);
                float alpha = 1.5f - (((flickerTime2 * flickerTime2) / 2) + (2f * flickerTime2));
                if (alpha < 0)
                    alpha = 0;
                Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, ((Main.player[Projectile.owner].Center - Main.screenPosition) + new Vector2(0, Main.player[Projectile.owner].gfxOffY)), new Rectangle(0, Size, Size, Size), color * alpha, (float)radians + 3.9f, new Vector2(0, Size), Projectile.scale, SpriteEffects.None, 1);
            }
            return false;
        }

        public virtual void Smash(Vector2 position)
        {

        }

        public sealed override bool PreAI()
        {
            SafeAI();
            Projectile.scale = Projectile.ai[0] < 10 ? (Projectile.ai[0] / 10f) : 1;
            Player Player = Main.player[Projectile.owner];
            int degrees = (int)(((Player.ItemAnimation) * -0.7) + 55) * Player.direction;
            if (Player.direction == 1)
            {
                degrees += 180;
            }
            radians = degrees * (Math.PI / 180);
            if (Player.channel && !released)
            {
                if (Projectile.ai[0] == 0)
                {
                    Player.ItemTime = 180;
                    Player.ItemAnimation = 180;
                }
                if (Projectile.ai[0] < chargeTime)
                {
                    Projectile.ai[0]++;
                    float rot = Main.rand.NextFloat(6.28f);
                    if (dustType != -1)
                        Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * 35, dustType, -Vector2.One.RotatedBy(rot) * 1.5f, 0, default, Projectile.ai[0] / 100f);
                    if (Projectile.ai[0] < chargeTime / 1.5f || Projectile.ai[0] % 2 == 0)
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
                    if (Projectile.ai[0] == chargeTime)
                    {
                        for (int k = 0; k <= 100; k++)
                        {
                            if (dustType != -1)
                                Dust.NewDustPerfect(Projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.5f);
                        }
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath7, Projectile.Center);
                        Projectile.ai[0]++;
                    }
                    if (dustType != -1)
                        Dust.NewDustPerfect(Projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f));
                    angularMomentum = 0;
                }
                Projectile.damage = (int)((minDamage + (int)((Projectile.ai[0] / chargeTime) * (maxDamage - minDamage))) * Player.meleeDamage);
                Projectile.knockBack = minKnockback + (int)((Projectile.ai[0] / chargeTime) * (maxKnockback - minKnockback));
            }
            else
            {
                Projectile.scale = 1;
                if (angularMomentum < MaxSpeed)
                {
                    angularMomentum += Acceleration;
                }
                if (!released)
                {
                    released = true;
                    Projectile.friendly = true;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                }
                if (Projectile.ai[0] > chargeTime)
                {
                    //  Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Gold2>(), Vector2.One.RotatedByRandom(6.28f));
                }
            }

            Projectile.position.Y = Player.Center.Y - (int)(Math.Sin(radians * 0.96) * Size) - (Projectile.height / 2);
            Projectile.position.X = Player.Center.X - (int)(Math.Cos(radians * 0.96) * Size) - (Projectile.width / 2);
            if (lingerTimer == 0)
            {
                Player.ItemTime++;
                Player.ItemAnimation++;
                if (Player.ItemTime > angularMomentum + 1)
                {
                    Player.ItemTime -= (int)angularMomentum;
                    Player.ItemAnimation -= (int)angularMomentum;
                }
                else
                {
                    Player.ItemTime = 2;
                    Player.ItemAnimation = 2;
                }
                if (Player.ItemTime == 2 || (Main.tile[(int)Projectile.Center.X / 16, (int)((Projectile.Center.Y + 24) / 16)].collisionType == 1 && released))
                {
                    lingerTimer = 30;
                    if (Projectile.ai[0] >= chargeTime)
                    {
                        this.Smash(Projectile.Center);

                    }
                    if (Main.tile[(int)Projectile.Center.X / 16, (int)((Projectile.Center.Y + 24) / 16)].collisionType == 1)
                    {
                        Player.GetModPlayer<StarlightPlayer>().Shake += (int)(Projectile.ai[0] * 0.2f);
                    }
                    Projectile.friendly = false;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit42, Projectile.Center);
                }
            }
            else
            {
                lingerTimer--;
                if (lingerTimer == 1)
                {
                    Projectile.active = false;
                    Player.ItemTime = 2;
                    Player.ItemAnimation = 2;
                }
                Player.ItemTime++;
                Player.ItemAnimation++;
            }
            return true;
        }
    }
}
