using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets
{
	class Dart : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

        private Vector2 startPoint;
        public Vector2 endPoint;
        public Vector2 midPoint;
        public int duration;

		public override string Texture => AssetDirectory.Assets + "Tiles/Underground/" + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cursed Dart");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
        }

		public override void SetDefaults()
		{
			projectile.width = 16;
			projectile.height = 16;
			projectile.hostile = true;
            projectile.timeLeft = 120;
		}

		public override void AI()
		{
            projectile.rotation = projectile.velocity.ToRotation();

            if (startPoint == Vector2.Zero)
                startPoint = projectile.Center;

            if (endPoint != Vector2.Zero)
            {
                float timer = (duration - projectile.timeLeft) / (float)duration;
                float dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, endPoint - startPoint);
                float dist2 = ApproximateSplineLength(30, midPoint, endPoint - startPoint, endPoint, endPoint - midPoint);
                float factor = dist1 / (dist1 + dist2);

                if(timer < factor)
                    projectile.Center = Vector2.Hermite(startPoint, midPoint - startPoint, midPoint, endPoint - startPoint, timer * (1 / factor));
                if (timer >= factor)
                    projectile.Center = Vector2.Hermite(midPoint, endPoint - startPoint, endPoint, endPoint - midPoint, (timer - factor) * (1 / (1 - factor)));
            }

            projectile.rotation = (projectile.position - projectile.oldPos[0]).ToRotation();

            ManageCaches();
            ManageTrail();
            //Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), ModContent.DustType<Dusts.Shadow>(), Vector2.UnitY * -1, 0, Color.Black);
        }

        private float ApproximateSplineLength(int steps, Vector2 start, Vector2 startTan, Vector2 end, Vector2 endTan)
		{
            float total = 0;
            Vector2 prevPoint = start;

            for(int k = 0; k < steps; k++)
			{
                Vector2 testPoint = Vector2.Hermite(start, startTan, end, endTan, k / (float)steps);
                total += Vector2.Distance(prevPoint, testPoint);

                prevPoint = testPoint;
			}

            return total;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var glowTex = ModContent.GetTexture(Texture + "Glow");
			spriteBatch.Draw(glowTex, projectile.Center - Main.screenPosition, null, new Color(100, 0, 255), projectile.rotation, glowTex.Size() / 2, 1, 0, 0);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{

			return true;
		}

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 30; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            cache.Add(projectile.Center);

            while (cache.Count > 30)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 30, factor =>
            {
                float alpha = 1;

                if (projectile.timeLeft < 20)
                    alpha = projectile.timeLeft / 20f;

                return new Color(50 + (int)(factor.X * 150), 80, 255) * (float)Math.Sin(factor.X * 3.14f) * alpha;
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
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/ShadowTrail"));

            trail?.Render(effect);
        }
    }
}
