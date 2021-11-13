using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Content.Buffs;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Moonstone
{
    public class Moonfury : ModItem
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        private int cooldown = 0;

        public override void SetDefaults()
        {
            item.damage = 25;
            item.melee = true;
            item.width = 36;
            item.height = 38;
            item.useTime = 25;
            item.useAnimation = 25;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 7.5f;
            item.value = 1000;
            item.rare = ItemRarityID.Green;
            item.UseSound = SoundID.Item1;
            item.shootSpeed = 14f;
            item.autoReuse = false;
            item.shoot = ModContent.ProjectileType<MoonfuryProj>();
            item.useTurn = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonfury");
            Tooltip.SetDefault("Call down a shard of moonstone, afflicting enemies with Dreamfire\nAfflicted enemies take extra damage on hit from Moonfury");
        }

        public override void HoldItem(Player player)
        {
            cooldown--;
            base.HoldItem(player);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (cooldown > 0)
                return false;
            cooldown = 75;
            Vector2 direction = new Vector2(0, -1);
            direction = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
            position = Main.MouseWorld + (direction * 800);

            direction *= -10;
            speedX = direction.X;
            speedY = direction.Y;
            damage *= 2;
            return true;
        }

        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            if (target.HasBuff(ModContent.BuffType<MoonfuryDebuff>()))
            {
                player.GetModPlayer<StarlightPlayer>().Shake += 10;
                int index = target.FindBuffIndex(ModContent.BuffType<MoonfuryDebuff>());
                target.DelBuff(index);
                Helper.PlayPitched("Magic/Shadow1", 1, Main.rand.NextFloat(-0.1f, 0.1f));
                damage += 5;
                damage += (int)((float)target.defense / (float)5);
                Projectile.NewProjectile(target.Center, Vector2.Zero, ModContent.ProjectileType<MoonfuryRing>(), 0, 0, player.whoAmI);

                for (int i = 0; i < 16; i++)
                    Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.Glow>(), Vector2.UnitX.RotatedBy(Main.rand.NextFloat(6.28f)) * Main.rand.NextFloat(12), 0, new Color(50, 50, 255), 0.4f);
            }
        }
    }
    internal class MoonfuryProj : ModProjectile, IDrawPrimitive, IDrawAdditive
    {
        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;

        private float trailWidth = 1;
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        private bool stuck = false;
        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 50;
            projectile.melee = true;
            projectile.friendly = true;
            projectile.tileCollide = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 300;
            projectile.extraUpdates = 6;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonfury Spike");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }
        public override void AI()
        {
            if (projectile.ai[0] == 0)
                projectile.ai[0] = Main.MouseWorld.Y;

            if (projectile.Bottom.Y > projectile.ai[0])
                projectile.tileCollide = true;
            else
                projectile.tileCollide = false;
            if (!stuck)
            {
                var d = Dust.NewDustPerfect(projectile.Bottom + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), ModContent.DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(20, 20, 100), 0.8f);
                d.customData = Main.rand.NextFloat(0.6f, 1.3f);
                d.fadeIn = 10;
                Dust.NewDustPerfect(projectile.Bottom + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(50, 50, 255), 0.4f).fadeIn = 10;
                ManageCaches();

                projectile.rotation = projectile.velocity.ToRotation() - 1.44f;
            }
            else
            {
                projectile.friendly = false;

                if (projectile.timeLeft <= 30)
                    projectile.alpha += 10;

                trailWidth *= 0.93f;

                if (trailWidth > 0.05f)
                    trailWidth -= 0.05f;
                else
                    trailWidth = 0;
            }
            ManageTrail();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!stuck)
            {
                //add hit sound effect here
                Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().Shake += 10;
                Projectile.NewProjectileDirect(projectile.Bottom, Vector2.Zero, ModContent.ProjectileType<GravediggerSlam>(), 0, 0, projectile.owner).timeLeft = 194;
                stuck = true;
                projectile.extraUpdates = 0;
                projectile.velocity = Vector2.Zero;
                projectile.timeLeft = 90;
            }
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            spriteBatch.Draw(tex, (projectile.Bottom + new Vector2(0, 20)) - Main.screenPosition, null, lightColor * (1 - (projectile.alpha / 255f)), projectile.rotation, new Vector2(tex.Width / 2, tex.Height), projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<MoonfuryDebuff>(), 150);
        }
        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 50; i++)
                {
                    cache.Add(projectile.Bottom + new Vector2(0, 20));
                }
            }
            if (projectile.oldPos[0] != Vector2.Zero)
                cache.Add(projectile.oldPos[0] + new Vector2(projectile.width / 2, projectile.height) + new Vector2(0, 20));

            while (cache.Count > 50)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(20), factor => (10 + factor * 25) * trailWidth, factor =>
            {
                return new Color(120, 20 + (int)(100 * factor.X), 255) * factor.X * trailWidth;
            });

            trail.Positions = cache.ToArray();

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(20), factor => (80 + 0 + factor * 0) * trailWidth, factor =>
            {
                return new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * 0.15f * trailWidth;
            });

            trail2.Positions = cache.ToArray();

            if (projectile.velocity.Length() > 1)
            {
                trail.NextPosition = projectile.Bottom + new Vector2(0, 20) + projectile.velocity;
                trail2.NextPosition = projectile.Bottom + new Vector2(0, 20) + projectile.velocity;
            }
        }

        public void DrawPrimitives()
        {

            Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
            effect.Parameters["repeats"].SetValue(8f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));
            effect.Parameters["sampleTexture2"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2"));

            trail?.Render(effect);

            effect.Parameters["sampleTexture2"].SetValue(Main.magicPixel);

            trail2?.Render(effect);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.GetTexture(Texture + "_Additive");
            Color color = Color.White * (1 - (projectile.alpha / 255f));
            spriteBatch.Draw(tex, (projectile.Bottom + new Vector2(0, 20)) - Main.screenPosition, null, color, projectile.rotation, new Vector2(tex.Width / 2, tex.Height), projectile.scale, SpriteEffects.None, 0);
        }
    }
    internal class MoonfuryRing : ModProjectile, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.MoonstoneItem + "MoonfuryProj";

        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;

        private float Progress => 1 - (projectile.timeLeft / 10f);

        private float Radius => 66 * (float)Math.Sqrt(Math.Sqrt(Progress));
        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 80;
            projectile.ranged = true;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 10;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orbital Strike");
        }

        public override void AI()
        {
            ManageCaches();
            ManageTrail();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override bool? CanHitNPC(NPC target)
        {
            if (target.whoAmI == (int)projectile.ai[0])
                return false;
            return base.CanHitNPC(target);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - projectile.Center;
            line.Normalize();
            line *= Radius;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + line))
            {
                return true;
            }
            return false;
        }

        private void ManageCaches()
        {
            cache = new List<Vector2>();
            float radius = Radius;
            for (int i = 0; i < 33; i++)
            {
                double rad = (i / 32f) * 6.28f;
                Vector2 offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
                offset *= radius;
                cache.Add(projectile.Center + offset);
            }

            while (cache.Count > 33)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 38 * (1 - Progress), factor =>
            {
                return Color.Lerp(new Color(180, 180, 255), new Color(85, 85, 200), Progress);
            });

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 20 * (1 - Progress), factor =>
            {
                return Color.White;
            });
            float nextplace = 33f / 32f;
            Vector2 offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
            offset *= Radius;

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + offset;

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = projectile.Center + offset;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));
            effect.Parameters["alpha"].SetValue(1);

            trail?.Render(effect);
            trail2?.Render(effect);
        }
    }
    class MoonfuryDebuff : SmartBuff
    {
        public MoonfuryDebuff() : base("Dreamfire", "Next Moonfury hit has increased damage", false) { }

        public override void Update(NPC npc, ref int buffIndex)
        {
            Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<Dusts.Glow>(), 0, 0, 0, new Color(50, 50, 255), 0.4f).velocity = Vector2.Zero;

            var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<Dusts.Aurora>(), 0, 0, 0, new Color(20, 20, 100), 0.8f);
            d.customData = Main.rand.NextFloat(0.6f, 1.3f);
            d.fadeIn = 10; 
        }
    }
}