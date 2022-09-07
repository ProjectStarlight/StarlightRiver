using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.SpaceEvent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;

namespace StarlightRiver.Content.Items.Dungeon
{
    public class ThousandthDegree : ModItem
    {
        public override string Texture => AssetDirectory.DungeonItem + Name;

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<ThousandthDegreeProjectile>()] <= 0;

        public override void SetStaticDefaults()
        {
            //probably a bad tooltip idk
            Tooltip.SetDefault("Melts through enemies to build up heat\nReleasing launches the blazing wheel, flying through enemies and sliding on tiles before returning\n'Rip and tear'");
        }

        public override void SetDefaults()
        {
            Item.shoot = ModContent.ProjectileType<ThousandthDegreeProjectile>();
            Item.shootSpeed = 2f;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.DamageType = DamageClass.Melee;
            Item.damage = 42;
            Item.knockBack = 2.5f;

            Item.width = 80;
            Item.height = 50;
            Item.useTime = 40;
            Item.useAnimation = 40;

            Item.value = Item.sellPrice(gold: 3, silver: 50);
            Item.rare = ItemRarityID.Orange;

            Item.noMelee = true;
            Item.autoReuse = false;
            Item.channel = true;
            Item.noUseGraphic = true;
        }
    }

    class ThousandthDegreeProjectile : ModProjectile
    {
        private const int MAXHEAT = 30;

        private int flashTimer;

        private float wheelRot;

        private bool fired;

        private Projectile fireProjectile;

        public ref float CurrentHeat => ref Projectile.ai[0];

        public Vector2 armPos => owner.RotatedRelativePoint(owner.MountedCenter, true) + Projectile.velocity.SafeNormalize(owner.direction * Vector2.UnitX) * 35f;

        public Vector2 wheelPos => armPos + Projectile.velocity.SafeNormalize(owner.direction * Vector2.UnitX) * 20f;

        public Player owner => Main.player[Projectile.owner];

        public bool CanHold => owner.channel && !owner.CCed && !owner.noItems;

        public override string Texture => AssetDirectory.DungeonItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Thousandth Degree");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.width = 96;
            Projectile.height = 64;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 2;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (CurrentHeat >= MAXHEAT)
            {
                if (flashTimer == 0)
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.2f}, Projectile.position);
                flashTimer = 20;
            }
            else
                CurrentHeat += 0.1f;

            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            Projectile.timeLeft = 2;

            if (CanHold && !fired)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    float interpolant = Utils.GetLerpValue(5f, 25f, Projectile.Distance(Main.MouseWorld), true);

                    Vector2 oldVelocity = Projectile.velocity;

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, owner.DirectionTo(Main.MouseWorld), interpolant);
                    if (Projectile.velocity != oldVelocity)
                    {
                        Projectile.netSpam = 0;
                        Projectile.netUpdate = true;
                    }
                }

                wheelRot += MathHelper.Lerp(0.1f, 0.35f, CurrentHeat / MAXHEAT);

                if (Main.rand.NextBool((int)MathHelper.Lerp(15, 2, CurrentHeat / MAXHEAT)))
                {
                    Dust.NewDustPerfect(wheelPos + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 25, new Color(255, 50, 15), Main.rand.NextFloat(0.4f, 0.6f));
                    if (Main.rand.NextBool(4))
                        Dust.NewDustPerfect(armPos, ModContent.DustType<Dusts.MagmaSmoke>(), (Vector2.UnitX.RotatedBy(Projectile.rotation) * -owner.direction) + Vector2.UnitY * -3f, 160, default, Main.rand.NextFloat(0.6f, 0.9f));
                }
            }
            else
            {
                Projectile.friendly = false;
                if (!fired)
                {
                    LaunchWheel();
                }
                else if (owner.ownedProjectileCounts[ModContent.ProjectileType<ThousandthDegreeProjectileFired>()] <= 0)
                    Projectile.Kill();

                if (!((ThousandthDegreeProjectileFired)fireProjectile.ModProjectile).returning)
                {
                    float interpolant = Utils.GetLerpValue(5f, 25f, Projectile.Distance(fireProjectile.Center), true);

                    Vector2 oldVelocity = Projectile.velocity;

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, owner.DirectionTo(fireProjectile.Center), interpolant);
                    if (Projectile.velocity != oldVelocity)
                    {
                        Projectile.netSpam = 0;
                        Projectile.netUpdate = true;
                    }
                }
            }

            Projectile.rotation = Utils.ToRotation(Projectile.velocity);
            owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

            Projectile.spriteDirection = Projectile.direction;
            if (Projectile.spriteDirection == -1)
                Projectile.rotation += 3.1415927f;


            Projectile.position = armPos - Projectile.Size * 0.5f;

            if (Projectile.alpha >= 255)
                Projectile.alpha = 0;

            owner.ChangeDir(Projectile.direction);
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(60f) * owner.direction);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage = (int)(damage * MathHelper.Lerp(0.9f, 2f, CurrentHeat / MAXHEAT));
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Vector2 direction = Projectile.DirectionTo(target.Center);

            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<Dusts.GlowLineFast>(), owner.Center.DirectionTo(wheelPos).RotatedByRandom(0.3f) * -Main.rand.NextFloat(5f), 0, new Color(255, 50, 15) * 0.75f, 1.5f);
                for (int d = 0; d < 2; d++)
                {
                    Dust.NewDustPerfect(wheelPos + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<Dusts.Glow>(), direction.RotatedByRandom(0.35f) * -Main.rand.NextFloat(6f), 25, new Color(255, 50, 15), Main.rand.NextFloat(0.5f, 0.6f));
                    Dust.NewDustPerfect(wheelPos + Main.rand.NextVector2Circular(15f, 15f), DustID.Torch, direction.RotatedByRandom(0.3f) * -Main.rand.NextFloat(6f), 0, default, 1.25f);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D wheelTex = ModContent.Request<Texture2D>(AssetDirectory.DungeonItem + "ThousandthDegreeProjectileFired").Value;
            Texture2D wheelTexGlow = ModContent.Request<Texture2D>(AssetDirectory.DungeonItem + "ThousandthDegreeProjectileFired_Glowy").Value;
            Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0f);

            Color bloomColor = Color.Lerp(Color.Transparent, new Color(255, 50, 15, 0), CurrentHeat / MAXHEAT);

            /*if (!fired)
            {
                Main.spriteBatch.Draw(wheelTex, wheelPos - Main.screenPosition, null, Color.White, wheelRot, wheelTex.Size() / 2f, Projectile.scale, Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0f);
                Main.spriteBatch.Draw(wheelTexGlow, wheelPos - Main.screenPosition, null, bloomColor, wheelRot, wheelTexGlow.Size() / 2f, Projectile.scale, Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0f);

                for (int i = 0; i < 3; i++)
                {
                    Main.spriteBatch.Draw(bloomTex, wheelPos - Main.screenPosition, null, bloomColor, wheelRot, bloomTex.Size() / 2f, 0.85f, 0, 0f);
                }
            }*/
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - wheelPos;
            line.Normalize();   
            line *= 20f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), wheelPos, wheelPos + line);
        }

        public void LaunchWheel()
        {
            fired = true;
            if (Main.myPlayer == Projectile.owner)
            {
                int damage =  (int)(Projectile.damage * MathHelper.Lerp(0.9f, 2f, CurrentHeat / MAXHEAT));
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), wheelPos, Projectile.DirectionTo(Main.MouseWorld) * 15f, ModContent.ProjectileType<ThousandthDegreeProjectileFired>(), damage, 2.5f, Projectile.owner);
                if (proj.ModProjectile is ThousandthDegreeProjectileFired wheel)
                {
                    wheel.parentProj = Projectile;
                }
                fireProjectile = proj;
            }
        }
    }

    class ThousandthDegreeProjectileFired : ModProjectile
    {
        public Projectile parentProj;

        public int lerpTimer;

        public bool returning;

        public Vector2 moveDirection;
        public Vector2 newVelocity = Vector2.Zero;
        public float speed = 10f;

        bool collideX = false;
        bool collideY = false;
        bool collided;

        public override string Texture => AssetDirectory.DungeonItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blazing Wheel");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.tileCollide = true;

            Projectile.width = Projectile.height = 44;
            Projectile.timeLeft = 2400;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            if ((Projectile.Distance(((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos) < 100f || Projectile.Distance(((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos) > 2000f) && Projectile.timeLeft < 2340)
            {
                returning = true;
                if (lerpTimer == 0)
                    lerpTimer = 60;
            }
            if (!returning && collided)
            {
                if (Projectile.tileCollide == true)
                    moveDirection = new Vector2(-Main.player[Projectile.owner].direction, 1);

                Projectile.tileCollide = false;
                
                newVelocity = Collide();
                if (Math.Abs(newVelocity.X) < 0.5f)
                    collideX = true;
                else
                    collideX = false;
                if (Math.Abs(newVelocity.Y) < 0.5f)
                    collideY = true;
                else
                    collideY = false;

                if (Projectile.ai[1] == 0f)
                {
                    Projectile.rotation += (float)(moveDirection.X * moveDirection.Y) * 0.75f;
                    if (collideY)
                    {
                        Projectile.ai[0] = 2f;
                    }
                    if (!collideY && Projectile.ai[0] == 2f)
                    {
                        moveDirection.X = -moveDirection.X;
                        Projectile.ai[1] = 1f;
                        Projectile.ai[0] = 1f;
                    }
                    if (collideX)
                    {
                        moveDirection.Y = -moveDirection.Y;
                        Projectile.ai[1] = 1f;
                    }
                }
                else
                {
                    Projectile.rotation -= (float)(moveDirection.X * moveDirection.Y) * 0.13f;
                    if (collideX)
                    {
                        Projectile.ai[0] = 2f;
                    }
                    if (!collideX && Projectile.ai[0] == 2f)
                    {
                        moveDirection.Y = -moveDirection.Y;
                        Projectile.ai[1] = 0f;
                        Projectile.ai[0] = 1f;
                    }
                    if (collideY)
                    {
                        moveDirection.X = -moveDirection.X;
                        Projectile.ai[1] = 0f;
                    }
                }
                Projectile.velocity = speed * moveDirection;
                Projectile.velocity = Collide();
            }
            else if (!collided)
            {
                Projectile.velocity.Y += 0.6f;
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;
            }
            else if (returning)
            {
                lerpTimer--;
                Projectile.Center = Vector2.Lerp(Projectile.Center, ((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos, 1f - lerpTimer / 60f);
                Projectile.velocity = Vector2.Zero;
                if (lerpTimer <= 0)
                    Projectile.Kill();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            collided = true;
            return false;
        }
        protected virtual Vector2 Collide()
        {
            return Collision.noSlopeCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true, true);
        }
    }
}
