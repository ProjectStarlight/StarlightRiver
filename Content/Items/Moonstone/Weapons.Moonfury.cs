using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Gravedigger;
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
            Tooltip.SetDefault("Send a shard of the moon down upon your enemies");
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 direction = new Vector2(0, -1);
            direction = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
            position = Main.MouseWorld + (direction * 800);

            direction *= -10;
            speedX = direction.X;
            speedY = direction.Y;
            return true;
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
                Dust.NewDustPerfect(projectile.Bottom + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(15), ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(50, 50, 255), 0.4f);
                ManageCaches();
                projectile.rotation = projectile.velocity.ToRotation() - 1.57f;
            }
            else
            {
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
                Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().Shake += 10;
                Projectile.NewProjectileDirect(projectile.Bottom, Vector2.Zero, ModContent.ProjectileType<GravediggerSlam>(), 0, 0, projectile.owner).timeLeft = 60 ;
                stuck = true;
                projectile.extraUpdates = 0;
                projectile.velocity = Vector2.Zero;
                projectile.timeLeft = 60;
            }
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            spriteBatch.Draw(tex, (projectile.Bottom + new Vector2(0, 20)) - Main.screenPosition, null, lightColor, projectile.rotation, new Vector2(tex.Width / 2, tex.Height), projectile.scale, SpriteEffects.None, 0);
            return false;
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
                return new Color(120, 20 + (int)(100 * factor.X), 255) * factor.X;
            });

            trail.Positions = cache.ToArray();

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(20), factor => (80 + 0 + factor * 0) * trailWidth, factor =>
            { 
                return new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * 0.15f;
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
            Color color = Color.White;
            spriteBatch.Draw(tex, (projectile.Bottom + new Vector2(0, 20)) - Main.screenPosition, null, color, projectile.rotation, new Vector2(tex.Width / 2, tex.Height), projectile.scale, SpriteEffects.None, 0);
        }
    }
}