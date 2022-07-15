using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using System.Text;
using StarlightRiver.Core.Systems;
using System.IO;
using StarlightRiver.Content.Items.Vitric;
using System.Linq;

namespace StarlightRiver.Content.Items.Misc.SoilgunFiles
{
    public abstract class BaseSoilProjectile : ModProjectile, IDrawPrimitive
    {
        public bool Gravity = true;

        public bool DrawTrail = true;

        public bool DrawWhite = false;

        public Color TrailColor = Color.White;

        private List<Vector2> cache;
        private Trail trail;

        public float AmmoType => Projectile.ai[0];

        public ref float Time => ref Projectile.ai[1];

        public sealed override string Texture => AssetDirectory.MiscItem + "Soilgun";

        public sealed override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soil");
        }

        public virtual void SafeSetDefaults() { }

        public sealed override void SetDefaults()
        {
            Projectile.penetrate = 1;
            SafeSetDefaults();

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.Size = new Vector2(12);
            Projectile.friendly = true;
            Projectile.timeLeft = 240;
        }

        public virtual void SafeAI() { }

        public sealed override void AI()
        {
            SafeAI();

            Time++;

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.timeLeft < 230 && Gravity)
            {
                Projectile.velocity.Y += 0.96f;
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;
            }

            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        //pretty much just override this for custom draw, only used for vitric soil
        public override bool PreDraw(ref Color lightColor)
        {
            //this predraw code is kinda bad examplemod boilerplate but it works
            Main.instance.LoadItem((int)AmmoType);
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Item[(int)AmmoType].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);


            Vector2 origin = sourceRectangle.Size() / 2f;

            float offsetX = 0f;
            origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

            Color drawColor = Projectile.GetAlpha(lightColor);

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, DrawWhite ? Color.White : drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            return false;
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 13; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 13)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 7, factor =>
            {
                return TrailColor * 0.8f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.position + Projectile.velocity;
        }
        public void DrawPrimitives()
        {
            if (!DrawTrail)
                return;

            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "SoilgunMuddyTrail").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(effect);
        }
    }

    public class SoilgunDirtSoil : BaseSoilProjectile
    {
        public override void SafeSetDefaults()
        {
            TrailColor = new Color(30, 19, 12);
        }
        public override void SafeAI()
        {
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.25f));
                dust.noGravity = true;
                if (Main.rand.NextBool(3))
                {
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<SandNoGravity>(), 0f, 0f, 120, default, Main.rand.NextFloat(0.7f, 1.1f));
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0f, 0f, 15, default, Main.rand.NextFloat(0.8f, 1f));
            }
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Sand>(), 0f, 0f, 140, default, Main.rand.NextFloat(0.8f, 1.1f));
            }
        }
    }

    public class SoilgunSandSoil : BaseSoilProjectile
    {
        public override void SafeSetDefaults()
        {
            TrailColor = new Color(58, 49, 18);
        }

        public override void SafeAI()
        {
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0f, 0f, 35, default, Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 12; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.1f));

                Dust.NewDustPerfect(Projectile.Center, DustID.Sand, (Vector2.UnitY * Main.rand.NextFloat(-3, -1)).RotatedByRandom(0.35f), 35, default, Main.rand.NextFloat(0.8f, 1.1f));
            }
            for (int i = 0; i < 6; i++)
            {
                if (Main.myPlayer == Projectile.owner)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-6.5f, -1)).RotatedByRandom(0.45f), ModContent.ProjectileType<SoilgunSandGrain>(), (int)(Projectile.damage * 0.33f), 0f, Projectile.owner);
            }
        }
    }

    public class SoilgunCrimsandSoil : BaseSoilProjectile
    {
        public override void SafeSetDefaults()
        {
            TrailColor = new Color(39, 17, 14);
        }

        public override void SafeAI()
        {
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CrimsonPlants, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.25f));
                dust.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Main.rand.NextBool(3) && Main.player[Projectile.owner].statLife < Main.player[Projectile.owner].statLifeMax2)
            {
                for (int i = 0; i < 12; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.LifeDrain, (Projectile.DirectionTo(Main.player[Projectile.owner].Center) * Main.rand.NextFloat(2f, 3f)).RotatedByRandom(MathHelper.ToRadians(5f)), 50, default, Main.rand.NextFloat(0.75f, 1f));
                    dust.noGravity = true;
                }
                if (Main.myPlayer == Projectile.owner && !target.SpawnedFromStatue && target.lifeMax > 5 && target.type != NPCID.TargetDummy)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(Main.player[Projectile.owner].Center), ModContent.ProjectileType<SoilgunLifeSteal>(), 0, 0f, Projectile.owner, 2 + (int)(damage * 0.1f));
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CrimsonPlants, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
            }
        }
    }

    public class SoilgunEbonsandSoil : BaseSoilProjectile
    {
        public override void SafeSetDefaults()
        {
            TrailColor = new Color(26, 18, 31);
        }

        public override void SafeAI()
        {
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ebonwood, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.15f));
                dust.noGravity = true;
                if (Main.rand.NextBool(2))
                {
                    Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.2f));
                    dust2.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            SoilgunGlobalNPC globalNPC = target.GetGlobalNPC<SoilgunGlobalNPC>();
            globalNPC.HauntedSoulDamage = damage * 3;
            globalNPC.HauntedStacks++;
            globalNPC.HauntedTimer = 420;
            globalNPC.HauntedSoulOwner = Projectile.owner;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 12; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ebonwood, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
            }
        }
    }

    public class SoilgunPearlsandSoil : BaseSoilProjectile
    {
        private bool foundTarget;
        public override void SafeSetDefaults()
        {
            TrailColor = new Color(87, 77, 106);
            DrawWhite = true;
        }

        public override void SafeAI()
        {
            Gravity = !foundTarget;

            Vector2 npcCenter = Projectile.Center;
            NPC npc = Projectile.FindTargetWithinRange(1500f);

            if (npc != null && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1) && !npc.dontTakeDamage && !npc.immortal)
            {
                npcCenter = npc.Center;
                foundTarget = true;
            }
            if (foundTarget)
            {
                float speed = Main.player[Projectile.owner].HeldItem.shootSpeed;
                Vector2 velo = Utils.SafeNormalize(npcCenter - Projectile.Center, Vector2.UnitY);
                Projectile.velocity = (Projectile.velocity * 20f + velo * speed) / (21f);
            }

            if (Main.rand.NextBool(5))
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Pearlsand, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.1f)).noGravity = true;
                if (Main.rand.NextBool(2))
                    Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.MoonstoneShimmer>(), Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.1f, 0.2f), 25, new Color(0.3f, 0.2f, 0.3f, 0f), Main.rand.NextFloat(0.2f, 0.3f));
            }

        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Pearlsand, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
            }
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.MoonstoneShimmer>(), Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.3f, 0.4f), 25, new Color(0.3f, 0.2f, 0.3f, 0f), Main.rand.NextFloat(0.3f, 0.4f));
            }
        }
    }

    public class SoilgunVitricSandSoil : BaseSoilProjectile
    {
        private int EnemyID;

        private bool stuck;

        private Vector2 offset;

        public override void SafeSetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override bool PreAI()
        {
            if (stuck)
            {
                NPC target = Main.npc[EnemyID];
                Projectile.position = target.position + offset;
            }
            return true;
        }

        public override void SafeAI()
        {
            DrawTrail = !stuck;

            float progress = 1 - (Projectile.timeLeft / 240f);
            TrailColor = Color.Lerp(new Color(86, 57, 47), Color.Lerp(Color.Orange, Color.Red, progress), progress);

            if (stuck)
            {
                if (!Main.npc[EnemyID].active)
                    Projectile.Kill();
                int decreasing = 0;
                for (int i = 0; i < Time / 2; i += 10)
                {
                    decreasing += 3;
                }
                if (Projectile.timeLeft < 120 && Main.rand.NextBool(5))
                {
                    float angle = Main.rand.NextFloat(6.28f);
                    Dust dust = Dust.NewDustPerfect((Projectile.Center - new Vector2(15, 15)) - (angle.ToRotationVector2() * (60 - decreasing)), ModContent.DustType<Dusts.NeedlerDustFive>());
                    dust.scale = 0.05f;
                    dust.velocity = angle.ToRotationVector2() * (Time < 60 ? 0.08f : 0.15f);
                }
                if (Projectile.timeLeft == 60)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireCast"), Projectile.position);
                }
            }
            else if (Main.rand.NextBool(5) && !stuck)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.GlassGravity>()).scale = Main.rand.NextFloat(0.8f, 1.1f);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //slighty rotates after sticking idk why
            if (!stuck && target.life > 0)
            {
                stuck = true;
                Projectile.friendly = false;
                Projectile.tileCollide = false;
                EnemyID = target.whoAmI;
                offset = Projectile.position - target.position;
                offset -= Projectile.velocity;
                Projectile.netUpdate = true;
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireHit"), Projectile.Center);
            if (!stuck)
            {
                CameraSystem.Shake += 2;
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SoilgunExplosion>(), 5 + (int)(Projectile.damage * 0.25f), 2.5f, Projectile.owner, 55);
                    for (int i = 0; i < 2; i++)
                    {
                        Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<NeedlerEmber>(), 0, 0, Projectile.owner);
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(7, 7);
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<Dusts.NeedlerDust>(), velocity.X, velocity.Y, 75 + Main.rand.Next(65), default, Main.rand.NextFloat(1.1f, 1.5f));
                    dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

                    Vector2 velocity2 = Main.rand.NextVector2Circular(7, 7);
                    Dust dust2 = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<Dusts.NeedlerDustTwo>(), velocity2.X, velocity2.Y, 45 + Main.rand.Next(85), default, Main.rand.NextFloat(1.1f, 1.5f));
                    dust2.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), ModContent.DustType<Dusts.NeedlerDustFour>()).scale = 0.75f;
                }
            }
            else
            {
                CameraSystem.Shake += 4;
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SoilgunExplosion>(), 5 + (int)(Projectile.damage * 0.5f), 2.5f, Projectile.owner, 95);
                    for (int i = 0; i < 6; i++)
                    {
                        Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<NeedlerEmber>(), 0, 0, Projectile.owner);
                    }
                }
                for (int i = 0; i < 6; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(9, 9);
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<Dusts.NeedlerDust>(), velocity.X, velocity.Y, 70 + Main.rand.Next(60), default, Main.rand.NextFloat(1.3f, 1.7f));
                    dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

                    Vector2 velocity2 = Main.rand.NextVector2Circular(9, 9);
                    Dust dust2 = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<Dusts.NeedlerDustTwo>(), velocity2.X, velocity2.Y, 40 + Main.rand.Next(80), default, Main.rand.NextFloat(1.3f, 1.7f));
                    dust2.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<Dusts.NeedlerDustFour>()).scale = 0.85f;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, stuck ? ModContent.DustType<MoltenGlassGravity>() : ModContent.DustType<Dusts.GlassGravity>(), 0f, 0f).scale = Main.rand.NextFloat(0.7f, 1.1f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadItem((int)AmmoType);
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Item[(int)AmmoType].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);


            Vector2 origin = sourceRectangle.Size() / 2f;

            float offsetX = 0f;
            origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

            Color drawColor = Projectile.GetAlpha(lightColor);

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, DrawWhite ? Color.White : drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            float progress = 1 - (Projectile.timeLeft / 240f);
            Color explodingColor = Color.Lerp(Color.Transparent, Color.Lerp(Color.Orange, Color.Red, progress) * 0.5f, progress);
            Texture2D WhiteVitricSandTex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "SoilgunVitricSandWhite").Value;
            Main.EntitySpriteDraw(WhiteVitricSandTex,
            Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
            sourceRectangle, explodingColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(stuck);
            writer.WritePackedVector2(offset);
            writer.Write(EnemyID);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            stuck = reader.ReadBoolean();
            offset = reader.ReadPackedVector2();
            EnemyID = reader.ReadInt32();
        }
    }

    public class SoilgunSlushSoil : BaseSoilProjectile
    {
        public override void SafeSetDefaults()
        {
            TrailColor = new Color(27, 40, 51);
        }

        public override void SafeAI()
        {
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, 0f, 0f, 35, default, Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            SoilgunGlobalNPC globalNPC = target.GetGlobalNPC<SoilgunGlobalNPC>();
            globalNPC.GlassPlayerID = Projectile.owner;
            globalNPC.GlassAmount++;

            if (Main.myPlayer == Projectile.owner)
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SoilgunIcicleProj>(), (int)(Projectile.damage * 0.65f), 0f, Projectile.owner, target.whoAmI);

            if (globalNPC.GlassAmount > 15)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<SoilgunIcicleProj>() && proj.active && proj.ai[0] == target.whoAmI)
                    {
                        proj.ai[1] = 1f;
                        proj.Kill();
                    }
                }
                globalNPC.GlassAmount = 0;
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath.WithVolumeScale(3f), Projectile.position);
                CameraSystem.Shake += 5;
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<Dusts.Mist>(), Main.rand.NextVector2Circular(1, 1), 0, Color.LightBlue, Main.rand.NextFloat(0.8f, 1.1f));
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Slush, 0f, 0f, 15, default, Main.rand.NextFloat(0.8f, 1f));
            }         
        }
    }

    public class SoilgunSiltSoil : BaseSoilProjectile
    {
        public override void SafeSetDefaults()
        {
            TrailColor = new Color(22, 24, 32);
        }

        public override void SafeAI()
        {
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Main.rand.Next(new int[] { DustID.CopperCoin, DustID.SilverCoin, DustID.GoldCoin, DustID.PlatinumCoin }), 0f, 0f, 35, default, Main.rand.NextFloat(0.8f, 1.2f));
                dust.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, Main.rand.Next(new int[] { DustID.CopperCoin, DustID.SilverCoin, DustID.GoldCoin, DustID.PlatinumCoin }), (Vector2.UnitY * Main.rand.NextFloat(-4, -1)).RotatedByRandom(0.25f), 35, default, Main.rand.NextFloat(1f, 1.3f));
            }
            for (int i = 0; i < 1 + Main.rand.Next(2); i++)
            {
                if (Main.myPlayer == Projectile.owner)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-9f, -1)).RotatedByRandom(0.35f), ModContent.ProjectileType<SoilgunCoinsProjectile>(), (int)(Projectile.damage * 0.66f), 1f, Projectile.owner);
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Silt, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
            }
        }
    }
}
