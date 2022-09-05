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
    //kinda just turned magmite gore into projectile cause I think it would be good for grains of sand
    class SoilgunSandGrain : ModProjectile
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sand Grain");
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.width = Projectile.height = 10;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 240;
            Projectile.penetrate = -1;
        }

        public override Color? GetAlpha(Color lightColor) => lightColor * (Projectile.scale < 0.5f ? Projectile.scale * 2 : 1);
        public override void AI()
        {
            if (Projectile.wet)
                Projectile.Kill();

            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.scale *= 0.99f;

            if (Projectile.scale < 0.3f)
                Projectile.Kill();

            if (Projectile.velocity.Y == 0)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = 0;
                Projectile.frame = 1;
            }

            if (Projectile.frame == 0)
            {
                Projectile.velocity.Y += 0.5f;
                if (Projectile.velocity.Y > 16f)
                {
                    Projectile.velocity.Y = 16f;
                }
            }
            if (Projectile.frame == 1)
            {
                Projectile.position.Y += 0.02f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.position += oldVelocity;
            Projectile.velocity *= 0f;
            return false;
        }
    }

    class SoilgunLifeSteal : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;
        public ref float LifeStealAmount => ref Projectile.ai[0];
        public override string Texture => AssetDirectory.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lifesteal Orb");
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 4;
            Projectile.timeLeft = 120;

            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            float homingSpeed = player.lifeMagnet ? 20f : 16f;
            Vector2 playerVector = player.Center - Projectile.Center;
            if (Projectile.Hitbox.Intersects(player.Hitbox))
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    player.HealEffect((int)LifeStealAmount, false);
                    player.statLife += (int)LifeStealAmount;
                    if (player.statLife > player.statLifeMax2)
                    {
                        player.statLife = player.statLifeMax2;
                    }
                    NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, Projectile.owner, (float)LifeStealAmount, 0f, 0f, 0, 0, 0);
                }
                Projectile.Kill();
            }
            Vector2 velo = Utils.SafeNormalize(playerVector, Vector2.UnitY);
            Projectile.velocity = (Projectile.velocity * 20f + velo * homingSpeed) / 21f;

            if (!player.active)
            {
                Projectile.Kill();
            }

            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.LifeDrain, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.1f));
                dust.noGravity = true;
            }

            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }
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
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 4, factor =>
            {
                return Color.Red * 0.8f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.position + Projectile.velocity;
        }
        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/LightningTrail").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/ShadowTrail").Value);

            trail?.Render(effect);
        }
    }

    internal class SoilgunExplosion : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        private float Progress => 1 - (Projectile.timeLeft / 5f);

        private float Radius => Projectile.ai[0] * (float)Math.Sqrt(Math.Sqrt(Progress));

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosion");
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
            line.Normalize();
            line *= Radius;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
            {
                return true;
            }
            return false;
        }
    }

    internal class SoilgunIcicleProj : ModProjectile
    {
        public Vector2 pos;
        public ref float TargetWhoAmI => ref Projectile.ai[0];
        public override string Texture => AssetDirectory.MiscItem + "SoilgunIcicles";
        public override bool? CanDamage() => false;

        public override void Load()
        {
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore1");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore2");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore3");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore4");
        }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            DisplayName.SetDefault("Icicle");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.frame = Main.rand.Next(4);
            Projectile.height = Projectile.width = 12;
        }

        public override void AI()
        {
            NPC npc = Main.npc[(int)TargetWhoAmI];
            if (Projectile.localAI[0] == 0f)
            {
                pos = new Vector2(Main.rand.Next(-npc.width / 2, npc.width / 2), Main.rand.Next(-npc.height / 2, npc.height / 2));
                Projectile.localAI[0] = 1f;
            }
            Projectile.Center = npc.Center + pos;
            if (Projectile.timeLeft < 50)
                Projectile.alpha += 5;

            if (!npc.active)
            {
                Projectile.ai[1] = 1f;
                Projectile.Kill();
            }
        }

        public override void Kill(int timeLeft)
        {
            NPC npc = Main.npc[(int)TargetWhoAmI];
            SoilgunGlobalNPC globalNPC = npc.GetGlobalNPC<SoilgunGlobalNPC>();
            if (Projectile.ai[1] == 1f)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.1f));
                }
                for (int i = 0; i < 2; i++)
                {
                    int GoreType = Mod.Find<ModGore>("SoilgunIcicles_Gore" + Main.rand.Next(1, 5).ToString()).Type;
                    Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(1, 1), GoreType).timeLeft = 60;
                }
                if (Main.myPlayer == Projectile.owner)
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SoilgunExplosion>(), (int)(Projectile.damage * 0.33f), 0f, Projectile.owner, 35);
            }
            if (globalNPC.GlassAmount > 0)
                globalNPC.GlassAmount--;
        }
    }

    internal class SoilgunCoinsProjectile : ModProjectile
    {
        public bool foundTarget;

        public bool HasCharged;
        public override bool? CanDamage() => Projectile.timeLeft < 90;
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Coin");
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 12;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.frame = Main.rand.Next(4);
            Projectile.timeLeft = 120;
            if (Projectile.frame < 2)
                Projectile.penetrate = 2;
            else
                Projectile.penetrate = 1;
        }

        public override void AI()
        {
            NPC npc = Projectile.FindTargetWithinRange(450f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (npc != null && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1) && !npc.dontTakeDamage && !npc.immortal && !foundTarget && Projectile.timeLeft < 90)
            {
                foundTarget = true;
                if (!HasCharged)
                {
                    const int Repeats = 35;
                    for (int i = 0; i < Repeats; ++i)
                    {
                        float angle2 = 6.2831855f * (float)i / (float)Repeats;
                        Dust dust3 = Dust.NewDustPerfect(Projectile.Center, ChooseDustType(), null, 0, default(Color), 1.1f);
                        dust3.velocity = Utils.ToRotationVector2(angle2) * 2.5f;
                        dust3.noGravity = true;
                    }
                    Projectile.velocity = Vector2.Normalize(npc.Center - Projectile.Center) * 18f;
                    HasCharged = true;
                }
            }
            if (!foundTarget)
            {
                Projectile.velocity.Y += 0.07f;
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Coins, Projectile.position);
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ChooseDustType(), 0f, 0f).scale = Main.rand.NextFloat(0.8f, 1.1f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            int FrameHeight = texture.Height / Main.projFrames[Projectile.type];

            Rectangle frameRect = new Rectangle(0, FrameHeight * Projectile.frame, texture.Width, FrameHeight);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, frameRect, color, Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            switch (Projectile.frame)
            {
                case 0: damage = (int)(damage * 1.3f); break; // platinum coin do more dmg then gold, gold more than silver, etc
                case 1: damage = (int)(damage * 1.2f); break;
                case 2: damage = (int)(damage * 1.1f); break;
            }
        }

        public int ChooseDustType()
        {
            switch (Projectile.frame)
            {
                case 0: return DustID.PlatinumCoin;
                case 1: return DustID.GoldCoin;
                case 2: return DustID.SilverCoin;
                case 3: return DustID.CopperCoin;
            }
            return 0;
        }
    }

    //maybe a sprite for this
    public class HauntedSoul : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        public bool foundTarget;
        public override string Texture => AssetDirectory.Invisible;

        public ref float NPCWhoSpawnedThis => ref Projectile.ai[0];

        public override bool? CanDamage() => Projectile.timeLeft < 330;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soul");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 360;
            Projectile.friendly = true;
            Projectile.width = Projectile.height = 16;
        }

        public override void AI()
        {
            Vector2 npcCenter = Projectile.Center;
            NPC npc = Projectile.FindTargetWithinRange(2000f);

            if (npc != null && !npc.dontTakeDamage && !npc.immortal && Projectile.timeLeft < 330)
            {
                npcCenter = npc.Center;
                foundTarget = true;
            }
            if (foundTarget)
            {
                float speed = 10f;
                Vector2 velo = Utils.SafeNormalize(npcCenter - Projectile.Center, Vector2.UnitY);
                Projectile.velocity = (Projectile.velocity * 20f + velo * speed) / (21f);
            }

            if (Main.rand.NextBool(3))
                Dust.NewDustDirect(Projectile.position, 1, 1, DustID.Shadowflame, 0f, 0f, 25, default, 0.9f);

            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override void Kill(int timeLeft)
        {
            Helper.PlayPitched("ShadowDeath", 1f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);
            const int Repeats = 45;
            for (int i = 0; i < Repeats; ++i)
            {
                float angle2 = 6.2831855f * (float)i / (float)Repeats;
                Dust dust3 = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, null, 0, default(Color), 1.1f);
                dust3.velocity = Utils.ToRotationVector2(angle2) * 2.25f;
                dust3.noGravity = true;
            }
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
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 16, factor =>
            {
                return new Color(52, 21, 141) * 0.8f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.position + Projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/ShadowTrail").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(effect);
        }
    }

    class SoilgunVitricCrystals : ModProjectile
    {
        internal bool hitByBullet = false;

        internal bool exploding;

        internal int enemyID;

        internal Vector2 offset = Vector2.Zero;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override bool? CanDamage() => false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Volatile Crystal");
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.width = Projectile.height = 16;

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;

            Projectile.tileCollide = false;
            Projectile.netUpdate = true;

            Projectile.frame = Main.rand.Next(3);

            Projectile.timeLeft = 240;

            Projectile.scale = 0.4f;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            NPC target = Main.npc[enemyID];
            Projectile.position = target.position + offset;

            if (!target.active || target.GetGlobalNPC<SoilgunGlobalNPC>().ShardTimer <= 0)
                Projectile.Kill();
            else if (!exploding)
            {
                if (Main.rand.NextBool(15))
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.GlassGravity>(), 0f, 0f, 0, default, Main.rand.NextFloat(0.5f, 0.8f));

                Projectile.timeLeft = 2;
            }
            else if (exploding)
            {
                if (Projectile.timeLeft < 100 && Main.rand.NextBool(15))
                {
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<MoltenGlassGravity>(), 0f, 0f, 0, default, Main.rand.NextFloat(0.5f, 0.8f));
                    if (Main.rand.NextBool(2))
                        Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.FireSparkle>(), 0f, 0f, 0, default, Main.rand.NextFloat(0.3f, 0.5f));
                }

                if (Projectile.timeLeft < 20)
                    Projectile.scale += 0.02f;

                if (Projectile.timeLeft == 60)
                    Helper.PlayPitched("Magic/FireCast", 0.35f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);
            }

            if (Projectile.ai[0] < 60)
                Projectile.scale += 0.01f;
        }

        public override void Kill(int timeLeft)
        {
            Main.npc[enemyID].GetGlobalNPC<SoilgunGlobalNPC>().ShardAmount--;

            SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
            if (exploding)
            {
                if (Main.myPlayer == Projectile.owner)
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SoilgunExplosion>(), Projectile.damage, 0f, Projectile.owner, 50);

                CameraSystem.Shake += 2;

                Helper.PlayPitched("Magic/FireHit", 0.45f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);

                for (int i = 0; i < 2; i++)
                {
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<MoltenGlassGravity>(), 0f, 0f, 0, default, Main.rand.NextFloat(0.6f, 0.85f));

                    Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.18f) * Main.rand.NextFloat(0.6f, 0.9f), 10, Color.Orange, Main.rand.NextFloat(0.6f, 0.75f));

                    Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.18f) * Main.rand.NextFloat(0.5f, 0.7f), 10, Color.DarkOrange, Main.rand.NextFloat(0.7f, 0.9f));
                }

                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.NeedlerDust>(), Vector2.One.RotatedByRandom(6.18f) * Main.rand.NextFloat(3f, 4.5f), 85, default, Main.rand.NextFloat(0.5f, 0.8f)).rotation = Main.rand.NextFloat(6.18f);

                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.NeedlerDustTwo>(), Vector2.One.RotatedByRandom(6.18f) * Main.rand.NextFloat(3f, 4.5f), 85, default, Main.rand.NextFloat(0.5f, 0.8f)).rotation = Main.rand.NextFloat(6.18f);

                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.NeedlerDustFour>(), Vector2.One.RotatedByRandom(6.18f) * Main.rand.NextFloat(3f, 4.5f), 85, default, Main.rand.NextFloat(0.5f, 0.8f)).rotation = Main.rand.NextFloat(6.18f);
                return;
            }
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.GlassGravity>(), 0f, 0f).scale = Main.rand.NextFloat(0.7f, 1.1f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);

            Rectangle frameRect = texture.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame);
            Vector2 drawOrigin = frameRect.Size() / 2f;

            Color drawColor = Projectile.GetAlpha(lightColor);

            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                frameRect, drawColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            if (exploding)
            {
                texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Orange");
                drawColor = Color.Lerp(Color.Transparent, Color.White, (1 - (Projectile.timeLeft / 120f)));
                spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                    frameRect, drawColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WritePackedVector2(offset);
            writer.Write(enemyID);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            offset = reader.ReadPackedVector2();
            enemyID = reader.ReadInt32();
        }
    }
}
