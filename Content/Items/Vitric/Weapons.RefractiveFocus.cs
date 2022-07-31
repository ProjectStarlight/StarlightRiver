using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Vitric
{
    // for some godforsaken reason that I cannot for the life of me find out the cause, the sentry oftentimes just despawns. not dies, despawns, and when this happens the projectile doesnt actually die,
    // it just doesnt exist anymore, and doesnt do damage, and doesnt draw its sprite / primitives, but the dust particles are still spawned on the closest enemy. The only fix ive found is spawning a different,
    // sentry, this causes the original projectile to die, but I dont know why, and its something to do with sentry shit but idk why so someone please fix <3. also when its in this invisible state, you cannot,
    // use the item, because according to the game, the projectile is still alive and owned by the player. plz help :(
    // ive also tried to debug this, it seems nothing wrong with the primitive drawing which i thought could be the culprit. I truly have no idea why this is happening.
    public class RecursiveFocus : ModItem
    {
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<RecursiveFocusProjectile>()] <= 0;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Summons an infernal crystal\nThe infernal crystal locks onto enemies, ramping up damage overtime");
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.QueenSpiderStaff);
            Item.sentry = true;
            Item.damage = 10;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 15;
            Item.knockBack = 0f;

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold : 2);

            Item.UseSound = SoundID.Item25;
            Item.useStyle = ItemUseStyleID.HoldUp;

            Item.shoot = ModContent.ProjectileType<RecursiveFocusProjectile>();
            Item.shootSpeed = 1f;

            Item.useTime = Item.useAnimation = 35;
        }
    }

    public class RecursiveFocusProjectile : ModProjectile
    {
        public Player owner => Main.player[Projectile.owner];

        public override string Texture => AssetDirectory.VitricItem + Name;

        public int flashTimer;

        public int trailMult = 5;

        public int trailMult2 = 3;

        public int timeSpentOnTarget;

        public Vector2 targetCenter;

        public bool foundTarget;

        public bool flashing;

        public NPC targetNPC;

        private List<Vector2> cache;
        private Trail trail;

        private List<Vector2> cache2;
        private Trail trail2;

        public override void Load()
        {
            for (int i = 1; i < 5; i++)
            {
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore" + i);
            }
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Infernal Crystal");

            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;

            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;

            Projectile.width = Projectile.height = 26;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true; //otherwise it hogs all iframes, making nothing else able to hit
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            if (!Main.dedServ && foundTarget)
            {
                ManageCaches();
                ManageTrail();
            }

            Vector2 idlePos = new Vector2(owner.Center.X, owner.Center.Y - 70f);

            Vector2 toIdlePos = idlePos - Projectile.Center;
            float speed = toIdlePos.Length() / 5;
            speed = Utils.Clamp(speed, 0, 25);
            toIdlePos.Normalize();
            toIdlePos *= speed;

            Projectile.velocity = (Projectile.velocity * (40f - 1) + toIdlePos) / 40f;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];

                float dist = Vector2.Distance(npc.Center, Projectile.Center);

                if (dist < 1000f)
                {
                    targetCenter = npc.Center;
                    targetNPC = npc;
                    foundTarget = true;
                }
            }

            if (!foundTarget)
                targetNPC = Projectile.FindTargetWithinRange(1000f);

            if (targetNPC != null)
            {
                if (Collision.CanHit(Projectile.Center, 1, 1, targetNPC.Center, 1, 1))
                {
                    foundTarget = true;
                    targetCenter = targetNPC.Center;
                    if (timeSpentOnTarget < 720)
                        timeSpentOnTarget++;
                }

                if (!targetNPC.active || Vector2.Distance(Projectile.Center, targetCenter) > 1000f || !Collision.CanHit(Projectile.Center, 1, 1, targetNPC.Center, 1, 1))
                {
                    foundTarget = false;
                    timeSpentOnTarget = 0;
                }

                if (foundTarget)
                {
                    float scale = 0.4f + MathHelper.Lerp(0, 0.1f, timeSpentOnTarget / 720);
                    Dust.NewDustPerfect(targetCenter, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 100, 65), scale);

                    Vector2 dustPos = Vector2.Lerp(Projectile.Center, targetCenter, Main.rand.NextFloat());
                    float dustScale = MathHelper.Lerp(0.2f, 0.6f, timeSpentOnTarget / 720f);
                    if (Main.rand.NextBool())
                        Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 100, 65), dustScale);
                    else
                        Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 185, 135), dustScale);

                    if (timeSpentOnTarget < 720)
                    {
                        float angle = Main.rand.NextFloat(6.28f);
                        if (Main.rand.NextBool())
                            Dust.NewDustPerfect(Projectile.Center - (angle.ToRotationVector2() * (35f - MathHelper.Lerp(0, 30, timeSpentOnTarget / 720f))), ModContent.DustType<Dusts.Glow>(), angle.ToRotationVector2() * 0.5f, 0, new Color(255, 100, 65), 0.25f);
                        else
                            Dust.NewDustPerfect(Projectile.Center - (angle.ToRotationVector2() * (35f - MathHelper.Lerp(0, 30, timeSpentOnTarget / 720f))), ModContent.DustType<Dusts.Glow>(), angle.ToRotationVector2() * 0.5f, 0, new Color(255, 185, 135), 0.25f);
                    }

                    if (timeSpentOnTarget == 240)
                    {
                        flashing = true;
                        for (int i = 0; i < 30; i++)
                        {
                            if (Main.rand.NextBool())
                                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.8f, 0, new Color(255, 100, 65), 0.6f);
                            else
                                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.8f, 0, new Color(255, 185, 135), 0.6f);
                        }

                        for (int i = 0; i < 45; i++)
                        {
                            Vector2 pos = Vector2.Lerp(Projectile.Center, targetCenter, Main.rand.NextFloat());
                            if (Main.rand.NextBool())
                                Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.35f, 0, new Color(255, 100, 65), 0.5f);
                            else
                                Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.35f, 0, new Color(255, 185, 135), 0.5f);
                        }
                    }
                    else if (timeSpentOnTarget == 480)
                    {
                        flashing = true;
                        for (int i = 0; i < 35; i++)
                        {
                            if (Main.rand.NextBool())
                                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 100, 65), 0.85f);
                            else
                                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 185, 135), 0.85f);
                        }

                        for (int i = 0; i < 55; i++)
                        {
                            Vector2 pos = Vector2.Lerp(Projectile.Center, targetCenter, Main.rand.NextFloat());
                            if (Main.rand.NextBool())
                                Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.45f, 0, new Color(255, 100, 65), 0.65f);
                            else
                                Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.45f, 0, new Color(255, 185, 135), 0.65f);
                        }
                    }
                    else if (timeSpentOnTarget == 719)
                    {
                        flashing = true;
                        for (int i = 0; i < 40; i++)
                        {
                            if (Main.rand.NextBool())
                                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 100, 65), 0.95f);
                            else
                                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 185, 135), 0.95f);
                        }

                        for (int i = 0; i < 60; i++)
                        {
                            Vector2 pos = Vector2.Lerp(Projectile.Center, targetCenter, Main.rand.NextFloat());
                            if (Main.rand.NextBool())
                                Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.55f, 0, new Color(255, 100, 65), 0.75f);
                            else
                                Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 0.55f, 0, new Color(255, 185, 135), 0.75f);
                        }
                    }

                    if (timeSpentOnTarget < 240)
                    {
                        trailMult = 4;
                        trailMult2 = 2;
                    }
                    else if (timeSpentOnTarget < 480)
                    {
                        trailMult = 9;
                        trailMult2 = 7;
                    }
                    else if (timeSpentOnTarget < 720)
                    {
                        trailMult = 14;
                        trailMult2 = 12;
                    }
                    else
                    {
                        trailMult = 19;
                        trailMult2 = 17;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail(Main.spriteBatch);

            Texture2D crystalTex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D baseTex = ModContent.Request<Texture2D>(Texture + "Base").Value;
            Texture2D bloomTex = ModContent.Request<Texture2D>(Texture + "_Bloom").Value;

            Color color = Color.Lerp(Color.Transparent, Color.Orange, (timeSpentOnTarget / 360f) - 1f);
            color.A = 0;

            if (timeSpentOnTarget > 360)
                Main.EntitySpriteDraw(bloomTex, Projectile.Center - Main.screenPosition, null, color * 0.5f, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 1.35f, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(baseTex, new Vector2(owner.Center.X, owner.Center.Y - 50) - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, baseTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(crystalTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, crystalTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

            if (foundTarget)
            {
                Texture2D crystalTexOrange = ModContent.Request<Texture2D>(Texture + "_Orange").Value;
                Texture2D baseTexOrange = ModContent.Request<Texture2D>(Texture + "Base_Orange").Value;

                Main.EntitySpriteDraw(baseTexOrange, new Vector2(owner.Center.X, owner.Center.Y - 50) - Main.screenPosition, null, Projectile.GetAlpha(lightColor) * (timeSpentOnTarget / 720f), Projectile.rotation, baseTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(crystalTexOrange, Projectile.Center - Main.screenPosition, null, Color.White * (timeSpentOnTarget / 720f), Projectile.rotation, crystalTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

                if (timeSpentOnTarget >= 720)
                {
                    Texture2D crystalTexGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
                    Main.EntitySpriteDraw(crystalTexGlow, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, Color.White, flashTimer <= 1 ? 1 : flashTimer / 35f), Projectile.rotation, crystalTexGlow.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
                }

                if (flashing)
                {
                    flashTimer++;
                    if (flashTimer > 35)
                    {
                        flashing = false;
                        flashTimer = 0;
                    }
                    Texture2D crystalTexWhite = ModContent.Request<Texture2D>(Texture + "_White").Value;
                    if (flashTimer > 0)
                        Main.EntitySpriteDraw(crystalTexWhite, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.White, Color.Transparent, flashTimer / 35f), Projectile.rotation, crystalTexWhite.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
                }
                Texture2D laserBloom = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
                Color color2 = new Color(255, 100, 65);
                color2.A = 0;
                Color color3 = new Color(255, 185, 135);
                color3.A = 0;

                Main.EntitySpriteDraw(laserBloom, targetCenter - Main.screenPosition, null, color2, 0f, laserBloom.Size() / 2f, 0.3f + MathHelper.Lerp(0, 0.4f, timeSpentOnTarget / 720f), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(laserBloom, targetCenter - Main.screenPosition, null, color3, 0f, laserBloom.Size() / 2f, 0.2f + MathHelper.Lerp(0, 0.4f, timeSpentOnTarget / 720f), SpriteEffects.None, 0);
            }
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float useless = 0f;
            return timeSpentOnTarget > 2 && foundTarget && Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, targetCenter, 15, ref useless);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage = (int)(damage * (1f + (timeSpentOnTarget / 720f)));

            if (target != targetNPC)
                damage = damage / 2;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (target == targetNPC)
                for (int i = 0; i < 7; i++)
                {
                    float scale = 0.45f + MathHelper.Lerp(0, 0.1f, timeSpentOnTarget / 720);
                    Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 185, 135), scale);
                }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 1; i < 5; i++)
            {
                Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, Mod.Find<ModGore>(Name + "_Gore" + i).Type, 1f).timeLeft = 90;
            }
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
        }

        private void ManageCaches()
        {
            cache = new List<Vector2>();
            for (int i = 0; i < 13; i++)
            {
                cache.Add(Vector2.Lerp(Projectile.Center, targetCenter, i / 13f));
            }
            cache.Add(targetCenter);

            cache2 = new List<Vector2>();
            for (int i = 0; i < 10; i++)
            {
                cache2.Add(Vector2.Lerp(Projectile.Center, targetCenter, i / 10f));
            }
            cache2.Add(targetCenter);
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 14, new TriangularTip(4), factor => trailMult, factor =>
            {
                return new Color(255, 100, 65) * 0.6f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = targetCenter;

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 11, new TriangularTip(4), factor => trailMult2, factor =>
            {
                return new Color(255, 185, 135) * 0.8f * factor.X;
            });

            trail2.Positions = cache2.ToArray();
            trail2.NextPosition = targetCenter;
        }

        private void DrawTrail(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.03f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            if (foundTarget && timeSpentOnTarget > 2)
            {
                trail?.Render(effect);

                trail2?.Render(effect);
            }

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

            if (foundTarget && timeSpentOnTarget > 2)
            {
                trail?.Render(effect);

                trail2?.Render(effect);
            }

            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
