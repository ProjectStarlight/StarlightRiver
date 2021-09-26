using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
    internal class VitricArmorProjectileIdle : ModProjectile
    {
        public Vector2 offset = Vector2.Zero;
        public float rotOffset = 0;
        public VitricHead parent;
        public int index;
        public float maxSize;

        Vector2 posTarget;
        float rotTarget;

        public ref float State => ref projectile.ai[0];
        public ref float Timer => ref projectile.ai[1];
        public Player Owner => Main.player[projectile.owner];

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.ranged = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 15;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
        }

        public override void AI()
        {
            Timer++;

            if (State != 2) //persist
                projectile.timeLeft = 15;

            var flippedOffset = offset;
            flippedOffset.X = offset.X * -Owner.direction;

            posTarget = Owner.Center + flippedOffset + Vector2.UnitY * Owner.gfxOffY;
            projectile.Center += (posTarget - projectile.Center) * 0.18f;

            if (Helper.CompareAngle(projectile.rotation, rotTarget) > 0.1f)
                projectile.rotation += Helper.CompareAngle(projectile.rotation, rotTarget) * 0.1f;
            else
                projectile.rotation = rotTarget;

            if(Owner.armor[0].type != ModContent.ItemType<VitricHead>())
			{
                State = 2;
                Timer = 0;
			}

            if(State == 0)
			{
                rotTarget = (float)Math.Sin(Timer * 0.02f) * 0.2f + rotOffset * -Owner.direction;

                if(Timer <= 30)
                    projectile.scale = Timer / 30f * maxSize;

                if (parent != null && parent.loaded)
				{
                    if (Timer < 30)
                        projectile.active = false;

                    State = 1;
                    Timer = 0;
				}
            }

            if(State == 1) //loaded
			{
                rotTarget = Helpers.Helper.LerpFloat(projectile.rotation, (Owner.Center - Main.MouseWorld).ToRotation() + 1.57f, Math.Min(1, Timer / 30f));

                if (parent != null && !parent.loaded)
                {
                    State = 0;
                    Timer = 30;
                }

                if (parent != null && parent.shardCount <= index)
				{
                    State = 2;
                    Timer = 0;
				}
			}

            if(State == 2)
			{
                projectile.scale = projectile.timeLeft / 15f * maxSize;
			}
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
            var tex = ModContent.GetTexture(Texture);
            var texGlow = ModContent.GetTexture(Texture + "Glow");
            var texHot = ModContent.GetTexture(Texture + "Hot");

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, tex.Size() / 2, projectile.scale, 0, 0);

            if(State == 0)
                spriteBatch.Draw(texGlow, projectile.Center - Main.screenPosition, null, VitricSummonOrb.MoltenGlow(Timer / 30f * 110), projectile.rotation, texGlow.Size() / 2, projectile.scale, 0, 0);

            if (State == 1)
                spriteBatch.Draw(texHot, projectile.Center - Main.screenPosition, null, Color.White * Math.Min(1, Timer / 30f), projectile.rotation, texHot.Size() / 2, projectile.scale, 0, 0);

            return false;
		}
	}

    internal class VitricArmorProjectile : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        public ref float state => ref projectile.ai[0];

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 4;
            projectile.height = 4;
            projectile.ranged = true;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 180;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.extraUpdates = 5;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + 1.57f;

            if (Main.rand.Next(5) == 0)
                Dust.NewDustPerfect(projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * Main.rand.NextFloat(-2, -1), 0, new Color(255, 150, 50), 0.6f);

            ManageCaches();
            ManageTrail();
        }

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
            projectile.velocity *= 0;
            projectile.friendly = false;
            return false;
		}

		private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 90; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            cache.Add(projectile.Center);

            while (cache.Count > 90)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 90, new TriangularTip(20 * 4), factor => factor * 20, factor =>
            {
                if (factor.X > 0.95f)
                    return Color.White * 0;

                float alpha = 1;

                if (projectile.timeLeft < 20)
                    alpha = projectile.timeLeft / 20f;

                return new Color(255, 175 + (int)((float)Math.Sin(factor.X * 3.14f * 5) * 25), 100) * (float)Math.Sin(factor.X * 3.14f) * alpha;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/EnergyTrail"));

            trail?.Render(effect);
        }
    }
}