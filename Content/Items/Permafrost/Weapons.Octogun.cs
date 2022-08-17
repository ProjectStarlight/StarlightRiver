using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;
using Terraria.ID;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Permafrost
{
    public class Octogun : ModItem
    {
        public override string Texture => AssetDirectory.PermafrostItem + Name;

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Converts Musket Balls into Auroracle Ink\nCritical hits and kills cause Tentapistols to sprout out from you, that fire at your cursor");
        }

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 30;

            Item.DamageType = DamageClass.Ranged;
            Item.damage = 15;
            Item.useTime = Item.useAnimation = 16;
            Item.useStyle = Terraria.ID.ItemUseStyleID.Shoot;
            Item.noMelee = true;

            Item.UseSound = new Terraria.Audio.SoundStyle("StarlightRiver/Sounds/SquidBoss/MagicSplash") with {Volume = 0.85f};

            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Bullet;

            Item.rare = ItemRarityID.Green;
            Item.autoReuse = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.Bullet)
                type = ModContent.ProjectileType<AuroracleInkBullet>();

            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(4f));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (type == ModContent.ProjectileType<AuroracleInkBullet>())
            {
                float sin = 1 + (float)Math.Sin(Main.GameUpdateCount * 10); //yes ive reused this color like 17 times shh
                float cos = 1 + (float)Math.Cos(Main.GameUpdateCount * 10);
                Color color = Main.masterMode ? new Color(1, 0.25f + sin * 0.25f, 0f) : new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
                for (int k = 0; k < 18; k++)
                {
                    Dust.NewDustPerfect(position + (Vector2.Normalize(velocity) * 45f), ModContent.DustType<Dusts.Cinder>(), (velocity * Main.rand.NextFloat(0.2f)).RotatedByRandom(0.2f),
                        125, color, Main.rand.NextFloat(0.25f, 0.55f));
                }
            }

            Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI).GetGlobalProjectile<OctogunGlobalProjectile>()
                .shotFromSquidGun = true;
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-5, 0);
        }
    }

    public class OctogunGlobalProjectile : GlobalProjectile
    {
        public bool shotFromSquidGun;

        public override bool InstancePerEntity => true;

        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            if (shotFromSquidGun)
                if (crit || target.life <= 0)
                    if (Main.player[projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<OctogunTentapistol>()] <= 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Projectile.NewProjectile(projectile.GetSource_FromThis(), Main.player[projectile.owner].Center, Vector2.One, ModContent.ProjectileType<OctogunTentapistol>(),
                                projectile.damage * 3/4, projectile.knockBack * 0.5f, projectile.owner, i);
                        }
                    }
                    else
                    {
                        for (int p = 0; p < Main.maxProjectiles; p++)
                        {
                            Projectile proj = Main.projectile[p];

                            if (proj.active && proj.owner == projectile.owner && proj.type == ModContent.ProjectileType<OctogunTentapistol>())
                                if (proj.timeLeft < 330)
                                    proj.timeLeft += 90;
                        }
                    }

        }
    }

    public class OctogunTentapistol : ModProjectile
    {
        private List<Vector2> cache;
        private Trail trail;

        internal const int OffsetIDTopLeft = 0; //offset ids, just for readability and easy remembering. these positions are with the player facing right, positions are flipped when the player is facing left
        internal const int OffsetIDTopRight = 1;
        internal const int OffsetIDBottomLeft = 2;

        public int rotationTimer;

        public Vector2 restingPosition;

        public float Offset => Projectile.ai[0];

        public ref float ShootDelay => ref Projectile.ai[1];

        public Player player => Main.player[Projectile.owner];

        public override string Texture => AssetDirectory.PermafrostItem + "Octogun_Tentapistol";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tentapistol");
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 660;

            Projectile.width = 28;
            Projectile.height = 14;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }
            if (rotationTimer > 0)
                rotationTimer--;

            if (ShootDelay > 0)
                ShootDelay--;

            if (Projectile.timeLeft > 20)
            {
                switch (Offset)
                {
                    case OffsetIDTopLeft: restingPosition = new Vector2(player.Center.X + (35 * player.direction), player.Center.Y - 60); break;
                    case OffsetIDTopRight: restingPosition = new Vector2(player.Center.X - (45 * player.direction), player.Center.Y - 50); break;
                    case OffsetIDBottomLeft: restingPosition = new Vector2(player.Center.X - (35 * player.direction), player.Center.Y + 10); break;
                }
            }
            else
            {
                restingPosition = player.Center;
            }

            Vector2 direction = restingPosition - Projectile.Center;
            if (direction.Length() < 0.0001f)
                direction = Vector2.Zero;
            else
            {
                float speed = Vector2.Distance(restingPosition, Projectile.Center) * 0.2f;
                speed = Utils.Clamp(speed, 1f, 40f);
                direction.Normalize();
                direction *= speed;
            }

            Projectile.velocity = (Projectile.velocity * (40f - 1) + direction) / 40f;

            if (Main.myPlayer == player.whoAmI)
            {
                Projectile.rotation = Projectile.DirectionTo(Main.MouseWorld).ToRotation() - (Main.MouseWorld.X < player.Center.X ? -MathHelper.ToRadians(rotationTimer) : MathHelper.ToRadians(rotationTimer));

                if (ShootDelay <= 0)
                {
                    int initialDelay = Offset == OffsetIDTopLeft ? 630 : 600;
                    if (Offset == OffsetIDBottomLeft)
                        initialDelay = 570;
                    if (Projectile.timeLeft < initialDelay)
                    {
                        Vector2 mouseDirection = Vector2.Normalize(Main.MouseWorld - Projectile.Center);
                        Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, mouseDirection * 20f,
                            ModContent.ProjectileType<AuroracleInkBullet>(), Projectile.damage, Projectile.knockBack, player.whoAmI, 1); // [ai] is one for tiny ink

                        Helper.PlayPitched("SquidBoss/SuperSplash", 0.45f, -0.1f, Projectile.position);

                        float sin = 1 + (float)Math.Sin(Main.GameUpdateCount * 10); //yes ive reused this color like 17 times shh
                        float cos = 1 + (float)Math.Cos(Main.GameUpdateCount * 10);
                        Color color = Main.masterMode ? new Color(1, 0.25f + sin * 0.25f, 0f) : new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
                        for (int k = 0; k < 12; k++)
                        {
                            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Cinder>(), (Vector2.Normalize(Main.MouseWorld - Projectile.Center) * Main.rand.NextFloat(3.5f)).RotatedByRandom(0.35f), 125, color, Main.rand.NextFloat(0.2f, 0.5f));
                        }
                        rotationTimer = 35;
                        Projectile.velocity += mouseDirection * -4f;
                        ShootDelay = 90;
                    }
                }                   
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            var texture = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;
            float sin = 1 + (float)Math.Sin(Projectile.timeLeft * 10);
            float cos = 1 + (float)Math.Cos(Projectile.timeLeft * 10);
            Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
            for (int i = 0; i < 3; i++)
                Main.spriteBatch.Draw(texture, (Projectile.Center - Projectile.velocity) - Main.screenPosition, null, color, Projectile.rotation, texture.Size() / 2, 0.45f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

            DrawPrimitives(Main.spriteBatch);
            #region tentacle drawing
            Texture2D tentacleTex = ModContent.Request<Texture2D>(AssetDirectory.PermafrostItem + "Octogun_Tentacle").Value;
            Vector2 offset = new Vector2(-45f, 0);
            if (Offset == OffsetIDTopRight)
                offset = new Vector2(45f, 0);
            if (Offset == OffsetIDBottomLeft)
                offset = new Vector2(0, 35f);

            Vector2 chainMidpoint = Vector2.Lerp(player.Center, Projectile.Center + offset, 0.5f);
            BezierCurve curve = new BezierCurve(new Vector2[] { player.Center, chainMidpoint, Projectile.Center});

            int numPoints = 6;
            Vector2[] chainPositions = curve.GetPoints(numPoints).ToArray();

            for (int i = 1; i < numPoints; i++)
            {
                Vector2 position = chainPositions[i];

                Rectangle frameRect = tentacleTex.Frame(1, 5, frameY: i - 1);

                float rotation = (chainPositions[i] - chainPositions[i - 1]).ToRotation() - MathHelper.PiOver2;
                float yScale = Vector2.Distance(chainPositions[i], chainPositions[i - 1]) / frameRect.Height; 

                Vector2 scale = new Vector2(1, yScale);
                Color chainLightColor = Lighting.GetColor((int)position.X / 16, (int)position.Y / 16);
                Vector2 origin = frameRect.Size() / 2f;
                Main.spriteBatch.Draw(tentacleTex, position - Main.screenPosition, frameRect, chainLightColor, rotation, origin, scale, SpriteEffects.None, 0);
            }
            #endregion tentacle drawing
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            SpriteEffects spriteEffects = (Main.MouseWorld.X < player.Center.X ? SpriteEffects.FlipVertically : SpriteEffects.None);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, spriteEffects, 0);
            return false;
        }

        private void ManageCaches()
        {
            if (cache is null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 10; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            for (int k = 0; k < 10; k++)
            {
                Vector2 offset = new Vector2(-45f, 0);
                if (Offset == OffsetIDTopRight)
                    offset = new Vector2(45f, 0);
                if (Offset == OffsetIDBottomLeft)
                    offset = new Vector2(0, 35f);

                Vector2 chainMidpoint = Vector2.Lerp(player.Center, Projectile.Center + offset, 0.5f);
                BezierCurve curve = new BezierCurve(new Vector2[] { player.Center, chainMidpoint, Projectile.Center });

                int numPoints = 10;
                Vector2[] chainPositions = curve.GetPoints(numPoints).ToArray();

                cache[k] = chainPositions[k];
            }

            while (cache.Count > 10)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => 12, factor =>
            {
                float sin = 1 + (float)Math.Sin(factor.X * 10);
                float cos = 1 + (float)Math.Cos(factor.X * 10);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

                return color * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = cache[9];
        }

        private void DrawPrimitives(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.02f);
            effect.Parameters["repeats"].SetValue(6);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

            trail?.Render(effect);
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    public class AuroracleInkBullet : ModProjectile
    {
        private int FadeInTimer;

        private List<Vector2> cache;
        private Trail trail;

        private bool Tiny => Projectile.ai[0] == 1f;
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Auroracle Ink");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = 1;
            AIType = ProjectileID.Bullet;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 480;
            Projectile.extraUpdates = 1;

            Projectile.width = Projectile.height = 8;  

            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true; //so bullets pierce but dont hog iframes
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.ai[1] += 0.1f;

            if (FadeInTimer < 15)
                FadeInTimer++;

            float sin = 1 + (float)Math.Sin(Projectile.timeLeft * Main.rand.NextFloat(8, 12));
            float cos = 1 + (float)Math.Cos(Projectile.timeLeft * Main.rand.NextFloat(8, 12));
            Color color = Main.masterMode ? new Color(1, 0.25f + sin * 0.25f, 0f) : new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
            Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.35f);

            if (Main.rand.NextBool(3) && Projectile.timeLeft < 475)
            {
                Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), ModContent.DustType<Dusts.AuroraFast>(), Vector2.Zero, 0, color, 0.5f).
                    customData = Main.rand.NextFloat(0.25f, 0.75f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (!Tiny)
                Projectile.position += Projectile.velocity.RotatedBy(1.57f) * (float)Math.Sin(Projectile.timeLeft / 480f * 3.14f * 3 + Projectile.ai[1]) * 0.05f;

            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            float sin = 1 + (float)Math.Sin(Projectile.timeLeft * 10);
            float cos = 1 + (float)Math.Cos(Projectile.timeLeft * 10);
            Color color = Main.masterMode ? new Color(1, 0.25f + sin * 0.25f, 0f) : new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
            for (int i = 0; i < (Tiny ? 7 : 10); i++)
            {
                Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), ModContent.DustType<Dusts.AuroraFast>(),
                    Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3f, 4f), 0, color, 0.65f).
                    customData = Main.rand.NextFloat(0.25f, 0.75f);

                Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), ModContent.DustType<Dusts.Cinder>(),
                    Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.5f, 1f), 0, color, 0.5f);
            }
        }

        public override void Kill(int timeLeft)
        {
            float sin = 1 + (float)Math.Sin(Projectile.timeLeft * 10);
            float cos = 1 + (float)Math.Cos(Projectile.timeLeft * 10);
            Color color = Main.masterMode ? new Color(1, 0.25f + sin * 0.25f, 0f) : new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
            for (int i = 0; i < (Tiny ? 13 : 18); i++)
            {
                Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), ModContent.DustType<Dusts.AuroraFast>(),
                    Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3.5f, 4.5f), 0, color, 0.85f).
                    customData = Main.rand.NextFloat(0.5f, 1f);

                Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), ModContent.DustType<Dusts.Cinder>(),
                    Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.7f, 1.25f), 0, color, 0.5f);
            }

            Helper.PlayPitched("SquidBoss/LightSplash", 0.4f, 0, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawPrimitives(Main.spriteBatch);
            return true;
        }

        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            var tex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;
            float sin = 1 + (float)Math.Sin(Projectile.timeLeft * 10);
            float cos = 1 + (float)Math.Cos(Projectile.timeLeft * 10);
            Color color = Color.Lerp(Color.Transparent, Main.masterMode ? new Color(1, 0.25f + sin * 0.25f, 0f) : new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f), FadeInTimer / 15f);
            for (int i = 0; i < 4; i++)
                Main.spriteBatch.Draw(tex, (Projectile.Center - Projectile.velocity) - Main.screenPosition, null, color, Projectile.rotation, tex.Size() / 2, Tiny ? 0.25f : 0.4f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 20; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 20)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(Tiny ? 2f : 2.5f), factor => (Tiny ? 2.5f : 4.5f) * (factor * 2f), factor =>
            {
                float sin = 1 + (float)Math.Sin(factor.X * 10);
                float cos = 1 + (float)Math.Cos(factor.X * 10);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
                if (Main.masterMode)
                    color = new Color(1, 0.25f + sin * 0.25f, 0.25f);

                return Color.Lerp(Color.Transparent, color, FadeInTimer / 15f);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;
        }

        public void DrawPrimitives(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.02f);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);
            trail?.Render(effect);
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
