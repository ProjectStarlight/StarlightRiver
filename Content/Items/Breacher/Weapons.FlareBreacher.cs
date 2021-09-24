using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
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
            Tooltip.SetDefault("LMB to launch explosive flares\nRMB to launch a marker flare, calling down fury upon your enemies after a long chargeup");
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
            item.damage = 28;
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

        public override Vector2? HoldoutOffset() => new Vector2(0, 0);


        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            position.Y -= 4;
            type = ModContent.ProjectileType<ExplosiveFlare>();
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }

    internal class ExplosiveFlare : ModProjectile
    {
        int enemyID;
        bool stuck = false;
        int explosionTimer = 100;
        Vector2 offset = Vector2.Zero;

        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 16;

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
            if (stuck)
            {
                NPC target = Main.npc[enemyID];
                projectile.position = target.position + offset;
                explosionTimer--;

                if (explosionTimer <= 0 || !target.active)
                    Explode(target);
                return false;
            }
            else
                projectile.rotation = projectile.velocity.ToRotation() + 1.57f;

            return base.PreAI();
        }

		private void Explode(NPC target)
        {
            Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().Shake = 10;
            int numberOfProjectiles = Main.rand.Next(5, 8);
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float offsetRad = MathHelper.Lerp(0, 0.5f, (float)i / (float)numberOfProjectiles);
                Projectile.NewProjectile(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f) * target.width, Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(0 - offsetRad, offsetRad) - 1.57f) * Main.rand.NextFloat(9, 11), ModContent.ProjectileType<FlareShrapnel>(), projectile.damage, projectile.knockBack, projectile.owner);
            }

            /*for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectileDirect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f) * target.width, Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.7f, 0.7f) - 1.57f) * Main.rand.NextFloat(1, 2), ModContent.ProjectileType<NeedlerEmber>(), 0, 0, projectile.owner).scale = Main.rand.NextFloat(0.65f, 0.85f);
            }*/
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f), 0, 0, ModContent.DustType<FlareBreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(5,10);
                dust.scale = Main.rand.NextFloat(0.4f, 0.7f);
                dust.alpha = 70 + Main.rand.Next(60);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation - 1.57f), 0, 0, ModContent.DustType<FlareBreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(10,20);
                dust.scale = Main.rand.NextFloat(0.75f, 1f);
                dust.alpha = 70 + Main.rand.Next(60);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }
            Gore.NewGore(projectile.position, Vector2.Zero, mod.GetGoreSlot("Assets/Items/Breacher/FlareGore"));
            projectile.active = false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
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
            var source = new Rectangle(0, 0, projectile.width, projectile.height);

            if (stuck)
                source.Y += projectile.height * (Main.GameUpdateCount % 10 < 5 ? 1 : 0);

            Main.spriteBatch.Draw(tex, projectile.position - Main.screenPosition, source, lightColor, projectile.rotation, projectile.Size / 2, projectile.scale, 0, 0);

            return false;
        }
    }
    internal class FlareShrapnel : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;

        private Trail trail;
        public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;

            projectile.ranged = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
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
                return new Color(255, 140, 0);
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
}