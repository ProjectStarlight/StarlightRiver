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
            Tooltip.SetDefault("Melts through enemies to build up heat\nReleasing launches the blazing wheel, melting through enemies and sliding on tiles before returning\n'Rip and tear'");
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

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ModContent.ItemType<SteampunkSet.Buzzsaw>()).
                AddIngredient(ItemID.HellstoneBar, 12).
                AddIngredient(ItemID.Bone, 35).
                AddTile(TileID.Anvils).
                Register();
        }
    }

    class ThousandthDegreeProjectile : ModProjectile
    {
        private const int MAXHEAT = 30;

        private int flashTimer;

        private float wheelRot;

        private bool fired;

        private bool flashed;

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
            if (CurrentHeat >= MAXHEAT && !fired)
            {
                if (flashTimer == 0 && !flashed)
                {
                    flashed = true;
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.2f }, Projectile.position);
                    flashTimer = 20;
                }
            }
            else
                CurrentHeat += 0.1f;

            if (flashTimer > 0)
                flashTimer--;

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
                    LaunchWheel();
                else if (owner.ownedProjectileCounts[ModContent.ProjectileType<ThousandthDegreeProjectileFired>()] <= 0)
                    Projectile.Kill();

                if (fireProjectile is null || !(fireProjectile.ModProjectile is ThousandthDegreeProjectileFired)) //weird bug where sometimes during Ceiros fight the game thought that the fireProjectile was a Ceiros attack projectile. Didn't actually do anything besides throw an error, weapon still worked as normal. This is just an extra safety check
                    return;

                if (((ThousandthDegreeProjectileFired)fireProjectile.ModProjectile).lerpTimer <= 0)
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

            owner.ChangeDir(Projectile.direction);
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(60f) * owner.direction);

            if (Projectile.soundDelay == 0 && !fired)
            {
                Projectile.soundDelay = 20;
                SoundEngine.PlaySound(SoundID.Item34, Projectile.position);
            }

            if (Projectile.alpha > 0)
                Projectile.alpha -= 75;
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
                for (int d = 0; d < 2; d++)
                {
                    Dust.NewDustPerfect(wheelPos + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<Dusts.Glow>(), direction.RotatedByRandom(0.35f) * -Main.rand.NextFloat(6f), 25, new Color(255, 50, 15), Main.rand.NextFloat(0.5f, 0.6f));
                    Dust.NewDustPerfect(wheelPos + Main.rand.NextVector2Circular(15f, 15f), DustID.Torch, direction.RotatedByRandom(0.3f) * -Main.rand.NextFloat(6f), 0, default, 1.25f);
                }

                for (int k = 0; k < 2; k++)
                {
                    Dust.NewDustPerfect(wheelPos + Main.rand.NextVector2Circular(15f, 15f) + new Vector2(0, 55), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedByRandom(0.35f) * -Main.rand.NextFloat(6f), 0, new Color(255, 160, 50), 1.9f);
                }
            }

            Helper.PlayPitched("Impacts/FireBladeStab", 0.25f, Main.rand.NextFloat(-0.05f, 0.05f), wheelPos);
            Core.Systems.CameraSystem.Shake += 1;
            target.AddBuff(BuffID.OnFire, 240);
            target.AddBuff(BuffID.OnFire3, 240);
            if (CurrentHeat < MAXHEAT)
                CurrentHeat += 1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glowy").Value;
            Texture2D wheelTex = ModContent.Request<Texture2D>(AssetDirectory.DungeonItem + "ThousandthDegreeProjectileFired").Value;
            Texture2D wheelTexGlow = ModContent.Request<Texture2D>(AssetDirectory.DungeonItem + "ThousandthDegreeProjectileFired_Glowy").Value;
            Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, owner.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0f);

            if (flashTimer > 0)
            {
                Color color = Color.Lerp(new Color(255, 50, 15), Color.Transparent, 1 - (flashTimer / 20f));
                color.A = 0;
                Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, texGlow.Size() / 2f, Projectile.scale, owner.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0f);
            }

            Color bloomColor = Color.Lerp(Color.Transparent, new Color(255, 50, 15, 0), CurrentHeat / MAXHEAT);

            if (!fired)
            {
                Main.spriteBatch.Draw(wheelTex, wheelPos - Main.screenPosition, null, Color.White, wheelRot, wheelTex.Size() / 2f, Projectile.scale, owner.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0f);
                Main.spriteBatch.Draw(wheelTexGlow, wheelPos - Main.screenPosition, null, bloomColor, wheelRot, wheelTexGlow.Size() / 2f, Projectile.scale, owner.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0f);

                for (int i = 0; i < 3; i++)
                {
                    Main.spriteBatch.Draw(bloomTex, wheelPos - Main.screenPosition, null, bloomColor, wheelRot, bloomTex.Size() / 2f, 0.85f, 0, 0f);
                }
            }
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
            Point tilePos = wheelPos.ToTileCoordinates();
            if (WorldGen.SolidTile(tilePos.X, tilePos.Y)) //do not spawn a projectile if the wheelPos is in the ground, prevents wacky stuff with the fired projectile
            {
                Projectile.Kill();
                return; 
            }
            fired = true;
            if (Main.myPlayer == Projectile.owner)
            {
                int damage =  (int)(Projectile.damage * MathHelper.Lerp(0.9f, 2f, CurrentHeat / MAXHEAT));
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), wheelPos, Projectile.DirectionTo(Main.MouseWorld) * 20f, ModContent.ProjectileType<ThousandthDegreeProjectileFired>(), damage, 2.5f, Projectile.owner);
                if (proj.ModProjectile is ThousandthDegreeProjectileFired wheel)
                {
                    wheel.parentProj = Projectile;
                    wheel.inputHeat = CurrentHeat;
                }
                fireProjectile = proj;
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 velo = Projectile.DirectionTo(Main.MouseWorld);
                Dust.NewDustPerfect(wheelPos + Main.rand.NextVector2Circular(2f, 2f), ModContent.DustType<Dusts.MagmaSmoke>(), velo.RotatedByRandom(0.5f) * Main.rand.NextFloat(1.8f, 2.5f) + (Vector2.UnitY * -1f), 155, default, Main.rand.NextFloat(0.6f, 0.9f));
                
                Dust.NewDustPerfect(wheelPos, ModContent.DustType<Dusts.Glow>(), velo.RotatedByRandom(0.65f) * Main.rand.NextFloat(5f, 7f), 0, new Color(255, 160, 50), 0.4f);
                
                Dust.NewDustPerfect(wheelPos, ModContent.DustType<Dusts.Glow>(), velo.RotatedByRandom(0.65f) * Main.rand.NextFloat(6f, 8f), 0, new Color(255, 50, 15), 0.45f);
                
                Dust.NewDustPerfect(wheelPos, DustID.Torch, velo.RotatedByRandom(0.65f) * Main.rand.NextFloat(6f, 8f), 0, default, 1.35f);
            }

            Helper.PlayPitched("Magic/FireHit", 0.45f, 0, wheelPos);
            Core.Systems.CameraSystem.Shake += 4;
        }
    }

    class ThousandthDegreeProjectileFired : ModProjectile
    {
        private const int MAXHEAT = 30;

        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;

        public Projectile parentProj;

        public float inputHeat;
        public int lerpTimer;
        public int resetDelay;

        public bool returning;
        public int returningTimer;
        public bool switched;

        public Vector2 moveDirection;
        public Vector2 newVelocity = Vector2.Zero;
        public float speed => MathHelper.Lerp(3f, 10f, speedTimer / 65f);
        public int speedTimer;

        bool collideX = false;
        bool collideY = false;
        bool collided;

        public Player owner => Main.player[Projectile.owner];
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

            Projectile.width = Projectile.height = 36;
            Projectile.timeLeft = 2400;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }

            if ((Projectile.Distance(((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos) < (switched ? 300f : 200f) || Projectile.Distance(((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos) > 850f) && Projectile.timeLeft < 2340)
                returning = true;

            if (Projectile.velocity == Vector2.Zero)
            {
                returning = true;
                collided = true;
            }

            if (!returning && collided) //wheel has touched the ground and is not returning, it will now travel along blocks like blazing wheel
            {
                SlidingAI();
            }
            else if (!collided) // the wheel is effected by gravity once it is fired, before it has touched the ground.
            {
                Projectile.rotation += Projectile.velocity.Length() * 0.05f;
                Projectile.velocity.Y += 0.6f; //gravity
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;

                if (returning)// if returning is ever true in this if statement, set collided to true to run the if(returning).
                    collided = true;
            }
            else if (returning)
            {
                collided = true;
                if (lerpTimer > 0) // lerptimer is greater than zero, it is returning to the parentProj via lerping to its wheelPos.
                {
                    Projectile.Center = Vector2.Lerp(Projectile.Center, ((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos, 1f - lerpTimer / 30f);
                    lerpTimer--;
                    if (lerpTimer <= 0)
                        Projectile.Kill();
                }
                else // act like a boomerang after jumping off the ground, returning to the player.
                {
                    if (returningTimer < 15)
                    {
                        if (returningTimer == 0f)
                        {
                            Projectile.velocity = Projectile.DirectionTo(((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos) * 10f + (Vector2.UnitY * -5f);//jumping effect
                            for (int i = 0; i < 35; ++i)
                            {
                                float angle2 = 6.2831855f * (float)i / (float)35;
                                Dust dust3 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 0, new Color(255, 50, 15), 0.6f);
                                dust3.velocity = Utils.ToRotationVector2(angle2) * 2.75f;
                                dust3.noGravity = true;
                            }
                        }

                        if (returningTimer > 5f)
                            Projectile.velocity.Y += 0.6f; //gravity

                        if (Projectile.velocity.Y > 16f)
                            Projectile.velocity.Y = 16f;

                        returningTimer++;
                    }
                    else
                    {
                        VanillaBoomerangAI();
                        Projectile.tileCollide = false; //vanilla boomerangs don't collide with tiles
                    }

                    if (Projectile.Center.Distance(((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos) < 50f && lerpTimer == 0) //if close enough to the parent proj, activate the lerping to its wheelpos.
                        lerpTimer = 30;

                }

                Projectile.rotation += Projectile.velocity.Length() * 0.05f;

                if (Projectile.Center.Distance(((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos) < 2f) //if close enough to wheelpos, kill the projectile
                    Projectile.Kill();
            }

            if (Main.rand.NextBool(2))
            {
                if (Main.rand.NextBool())
                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 25, new Color(255, 50, 15), Main.rand.NextFloat(0.4f, 0.6f));
                else
                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), DustID.Torch, null, 0, default, Main.rand.NextFloat(1.25f, 1.6f)).noGravity = Main.rand.NextBool();

                if (Main.rand.NextBool(4))
                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<Dusts.MagmaSmoke>(), Vector2.UnitX.RotatedBy(Projectile.rotation) + Vector2.UnitY * -3f, 160, default, Main.rand.NextFloat(0.6f, 0.9f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glowy").Value;
            Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
            Color bloomColor = Color.Lerp(Color.Transparent, new Color(255, 50, 15, 0), inputHeat / MAXHEAT);
            DrawPrimitives();
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0, 0f);
            Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, bloomColor, Projectile.rotation, texGlow.Size() / 2f, Projectile.scale, 0, 0f);
            for (int i = 0; i < 3; i++)
            {
                Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, bloomColor, Projectile.rotation, bloomTex.Size() / 2f, 0.85f, 0, 0f);
            }

            return false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 240);
            target.AddBuff(BuffID.OnFire3, 240);

            Helper.PlayPitched("Impacts/FireBladeStab", 0.2f, Main.rand.NextFloat(-0.05f, 0.05f), Projectile.Center);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 35; ++i)
            {
                float angle2 = 6.2831855f * (float)i / (float)35;
                Dust dust3 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 0, new Color(255, 50, 15), 0.6f);
                dust3.velocity = Utils.ToRotationVector2(angle2) * 2.75f;
                dust3.noGravity = true;
            }

            for (int i = 0; i < 35; ++i)
            {
                float angle2 = 6.2831855f * (float)i / (float)35;
                Dust dust3 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 0, new Color(255, 160, 50), 0.45f);
                dust3.velocity = Utils.ToRotationVector2(angle2) * 2f;
                dust3.noGravity = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!returning)
                collided = true;

            return false;
        }

        protected virtual Vector2 Collide()
        {
            return Collision.noSlopeCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true, true);
        }

        private void SlidingAI()
        {
            if (speedTimer < 65) //speed timer to give the speed easing
                speedTimer++;

            if (resetDelay > 0) //delay of the wheel being able to reset its movement
                resetDelay--;

            if (Projectile.tileCollide) //the projectile just collided with the tile, give the moveDirection the opposite of the owners direction
                moveDirection = new Vector2(-owner.direction, 1);

            Projectile.tileCollide = false;

            if (Projectile.Distance(((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos) > 750f && !switched) //projectile is too far away from player, switch the direction
            {
                speedTimer = 0;
                switched = true;
                moveDirection.X = Projectile.Center.X < owner.Center.X ? 1 : -1;
            }

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
                    Projectile.ai[0] = 2f;

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
                Projectile.rotation -= (float)(moveDirection.X * moveDirection.Y) * 0.45f;
                if (collideX)
                    Projectile.ai[0] = 2f;

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
            Projectile.velocity = Collide(); // set the velo based of Collision.NoSlopeCollision() and speed + movedirection.
            if (Projectile.Center.Y < owner.Center.Y - 200 && resetDelay <= 0) //if the wheel is too far above the player it resets its movement
            {
                if (!switched)
                    switched = true;
                else
                {
                    collided = false;
                    Projectile.tileCollide = true;
                }

                resetDelay = 120;
            }

            Vector2 pos = Projectile.direction == 1 ? Projectile.BottomLeft : Projectile.BottomRight;
            Dust.NewDustPerfect(pos + new Vector2(Projectile.direction * 25f, 0), ModContent.DustType<Dusts.GlowFastDecelerate>(), (Vector2.UnitX * Projectile.direction) + Vector2.UnitY * Main.rand.NextFloat(), 0, new Color(255, 160, 50), Main.rand.NextFloat(0.3f, 0.5f));

            Dust.NewDustPerfect(pos + new Vector2(Projectile.direction * 45f, 45), ModContent.DustType<Dusts.BuzzSpark>(), (Vector2.UnitX * -Projectile.direction) + Vector2.UnitY * -Main.rand.NextFloat(1f, 2f), 0, new Color(255, 160, 50), 2.3f);
        }

        private void VanillaBoomerangAI() //this is bad vanilla code but I tried to make it as readable as I could
        {
            Vector2 wheelPosition = ((ThousandthDegreeProjectile)parentProj.ModProjectile).wheelPos;
            Vector2 pos = Projectile.Center;

            float betweenX = wheelPosition.X - pos.X;
            float betweenY = wheelPosition.Y - pos.Y;

            float distance = (float)Math.Sqrt(betweenX * betweenX + betweenY * betweenY);
            float speed = distance > 450f ? 17f : 12f;
            float adjust = 1f;

            if (distance > 3000f && lerpTimer == 0)
                lerpTimer = 30;

            distance = speed / distance;
            betweenX *= distance;
            betweenY *= distance;

            if (Projectile.velocity.X < betweenX)
            {
                Projectile.velocity.X += adjust;
                if (Projectile.velocity.X < 0f && betweenX > 0f)
                    Projectile.velocity.X += adjust;
            }
            else if (Projectile.velocity.X > betweenX)
            {
                Projectile.velocity.X -= adjust;
                if (Projectile.velocity.X > 0f && betweenX < 0f)
                    Projectile.velocity.X -= adjust;
            }
            if (Projectile.velocity.Y < betweenY)
            {
                Projectile.velocity.Y += adjust;
                if (Projectile.velocity.Y < 0f && betweenY > 0f)
                    Projectile.velocity.Y += adjust;
            }
            else if (Projectile.velocity.Y > betweenY)
            {
                Projectile.velocity.Y -= adjust;
                if (Projectile.velocity.Y > 0f && betweenY < 0f)
                    Projectile.velocity.Y -= adjust;
            }
        }

        private void ManageCaches()
        {
            if (cache is null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 16; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 16)
            {
                cache.RemoveAt(0);
            }

        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 16, new TriangularTip(8), factor => 20f, factor =>
            {
                return new Color(255, 160, 50) * 0.5f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 16, new TriangularTip(8), factor => 25f, factor =>
            {
                return new Color(255, 50, 15) * 0.75f * factor.X;
            });

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = Projectile.Center + Projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Main.spriteBatch.End();
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.04f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail2?.Render(effect);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

            trail2?.Render(effect);

            trail?.Render(effect);
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
