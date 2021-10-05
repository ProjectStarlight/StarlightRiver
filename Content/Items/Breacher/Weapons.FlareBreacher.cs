using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Bosses.VitricBoss;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Breacher
{
	public class FlareBreacher : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flare Breacher");
            Tooltip.SetDefault("Fires explosive flares that embed in enemies, blasting shrapnel through and behind them");
        }

        public override void SetDefaults()
        {
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 30;
            item.useTime = 30;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item11;
            item.width = 24;
            item.height = 28;
            item.damage = 30;
            item.rare = ItemRarityID.Orange;
            item.value = Item.sellPrice(0, 10, 0, 0);
            item.noMelee = true;
            item.autoReuse = true;
            item.useTurn = false;
            item.useAmmo = AmmoID.Flare;
            item.ranged = true;
            item.shoot = ModContent.ProjectileType<ExplosiveFlare>();
            item.shootSpeed = 17;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, 0);
        }


        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            type = ModContent.ProjectileType<ExplosiveFlare>();
            position.Y -= 4;
            Vector2 direction = new Vector2(speedX, speedY);

            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(position + (direction * 1.35f), 6, (direction.RotatedBy(Main.rand.NextFloat(-1, 1)) / 5f) * Main.rand.NextFloat());
                dust.noGravity = true;
            }

            Helper.PlayPitched("Guns/FlareFire", 0.6f, Main.rand.NextFloat(-0.1f,0.1f));
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }
    #region LMB projectiles
    internal class ExplosiveFlare : ModProjectile
    {
        public override string Texture => AssetDirectory.BreacherItem + Name;

        int enemyID;

        bool stuck = false;

        int explosionTimer = 100;

        Vector2 offset = Vector2.Zero;

        bool red;

        int blinkCounter = 0;

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;
            projectile.ranged = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.aiStyle = 1;
            aiType = 163;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosive Flare");
            Main.projFrames[projectile.type] = 2;
        }

        public override bool PreAI()
        {
            Lighting.AddLight(projectile.Center, Color.Purple.ToVector3());
            Vector2 direction = (projectile.rotation + 1.57f + Main.rand.NextFloat(-0.2f, 0.2f)).ToRotationVector2();

            if (stuck)
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<BreacherDust>(), direction * Main.rand.NextFloat(3, 4));
                dust.scale = 1.15f;
                dust.noGravity = true;
                NPC target = Main.npc[enemyID];
                projectile.position = target.position + offset;
                explosionTimer--;

                blinkCounter++;
                int timerVal = 3 + (int)Math.Sqrt(explosionTimer);

                if (blinkCounter > timerVal)
                {
                    if (!red)
                    {
                        red = true;
                        Helper.PlayPitched("Effects/Bleep", 0.6f, 1 - (explosionTimer / 100f));
                        blinkCounter = 0;
                    }
                    else
                    {
                        red = false;
                        blinkCounter = 0;
                    }
                }

                if (explosionTimer <= 0 || !target.active)
                    Explode(target);

                return false;
            }
            else
            {
                for (float i = 0; i < 1; i+= 0.25f)
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center - (projectile.velocity * i), ModContent.DustType<BreacherDustFour>(), direction * Main.rand.NextFloat(3, 4));
                    dust.scale = 0.85f;
                    dust.noGravity = true;
                }

                projectile.rotation = projectile.velocity.ToRotation() + 1.57f;
            }

            return base.PreAI();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Dust.NewDustPerfect(projectile.Center + oldVelocity, ModContent.DustType<FlareBreacherDust>(), Vector2.Zero, 60, default, 0.7f).rotation = Main.rand.NextFloat(6.28f);
            return base.OnTileCollide(oldVelocity);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Player player = Main.player[projectile.owner];
            if (!stuck && target.life > 0)
            {
                stuck = true;
                projectile.friendly = false;
                projectile.tileCollide = false;
                enemyID = target.whoAmI;
                offset = projectile.position - target.position;
                offset -= projectile.velocity;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = Main.projectileTexture[projectile.type];
            var source = new Rectangle(0, 0, projectile.width, 16);

            if (stuck)
                source.Y += 16 * (red ? 1 : 0);

            Main.spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, source, lightColor, projectile.rotation, (tex.Size() / 2) * new Vector2(1, 0.5f), projectile.scale, 0, 0);

            return false;
        }

        private void Explode(NPC target)
        {
            Helper.PlayPitched("Guns/FlareBoom", 0.6f, Main.rand.NextFloat(-0.1f, 0.1f));
            target.StrikeNPC(projectile.damage, 0f, 0);
            Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().Shake = 10;
            int numberOfProjectiles = Main.rand.Next(5, 8);

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float offsetRad = MathHelper.Lerp(0, 0.5f, (float)i / (float)numberOfProjectiles);
                Projectile.NewProjectile(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f) * target.width, Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(0 - offsetRad, offsetRad) - 1.57f) * Main.rand.NextFloat(9, 11), ModContent.ProjectileType<FlareShrapnel>(), projectile.damage, projectile.knockBack, projectile.owner, target.whoAmI);
            }

            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f), 0, 0, ModContent.DustType<FlareBreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(5,10);
                dust.scale = Main.rand.NextFloat(0.4f, 0.7f);
                dust.alpha = 40 + Main.rand.Next(40);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f), 0, 0, ModContent.DustType<FlareBreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(10,20);
                dust.scale = Main.rand.NextFloat(0.75f, 1f);
                dust.alpha = 40 + Main.rand.Next(40);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }

            for (int i = 0; i < 24; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f), 0, 0, ModContent.DustType<BreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(1, 5);
                dust.scale = Main.rand.NextFloat(0.75f, 1.1f);
            }

            projectile.active = false;
        }
    }
    internal class FlareShrapnel : ModProjectile, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

        private List<Vector2> cache;

        private Trail trail;

        private NPC source => Main.npc[(int)projectile.ai[0]];

        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;
            projectile.ranged = true;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = Main.rand.Next(50,70);
            projectile.extraUpdates = 4;
            projectile.alpha = 255;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosive Shrapnel");
            Main.projFrames[projectile.type] = 2;
        }
        public override void AI()
        {
            projectile.velocity *= 0.96f;
            ManageCaches();
            ManageTrail();
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target == source)
                return false;
            return base.CanHitNPC(target);
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 20; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            cache.Add(projectile.Center);

            while (cache.Count > 20)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(40 * 4), factor => factor * 6, factor =>
            {
                return new Color(255, 50, 180);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["ShrapnelTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));
            effect.Parameters["progress"].SetValue(MathHelper.Lerp(projectile.timeLeft / 60f, 0, 0.3f));

            trail?.Render(effect);
        }
    }
    #endregion
}