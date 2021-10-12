using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    class StarlightShuriken : ModItem
    {
        public int amountToThrow = 3;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starlight Shuriken");
            Tooltip.SetDefault("Toss a volley of magical shurikens\nLanding every shuriken decreases the amount thrown by 1\nThrow a powerful glaive when you would throw only 1 shuriken");
        }

        public override void SetDefaults()
        {
            item.damage = 16;
            item.magic = true;
            item.mana = 13;
            item.width = 18;
            item.height = 34;
            item.useTime = 24;
            item.useAnimation = 24;
            item.useStyle = ItemUseStyleID.SwingThrow;

            item.knockBack = 0f;
            item.shoot = ModContent.ProjectileType<StarShuriken>();
            item.shootSpeed = 15f;
            item.noMelee = true;
            item.autoReuse = true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            var aim = Vector2.Normalize(new Vector2(speedX, speedY));

            if(amountToThrow == 1)
			{
				int i = Projectile.NewProjectile(player.Center, aim * 8f, ModContent.ProjectileType<StarGlaive>(), damage, knockBack, player.whoAmI);
                var proj = Main.projectile[i].modProjectile as StarGlaive;
				proj.color = new Color(240, 100, 255);

                amountToThrow = 3;
                return false;
            }

            for(int k = 0; k < amountToThrow; k++)
			{
                float maxAngle = amountToThrow == 3 ? 0.21f : 0.16f;
                int i = Projectile.NewProjectile(player.Center, aim.RotatedBy(-(maxAngle / 2f) + (k / (float)(amountToThrow - 1)) * maxAngle) * 8f, type, damage, knockBack, player.whoAmI);
                var proj = Main.projectile[i].modProjectile as StarShuriken;
                proj.color = amountToThrow == 3 ? new Color(100, 210, 255) : new Color(150, 150, 255);
                proj.creator = this;
			}

            amountToThrow--;

            if (amountToThrow <= 0)
                amountToThrow = 3;

            return false;
        }
    }

	class StarShuriken : ModProjectile, IDrawAdditive, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public StarlightShuriken creator;

		public Color color = new Color(100, 210, 255);

		public ref float State => ref projectile.ai[0];

		public Player Owner => Main.player[projectile.owner];

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetDefaults()
		{
			projectile.width = 30;
			projectile.height = 30;
			projectile.timeLeft = 150;
			projectile.extraUpdates = 3;
			projectile.magic = true;
			projectile.friendly = true;
			projectile.penetrate = 1;
			projectile.scale = 0.85f;
		}

		public override void AI()
		{
			projectile.rotation += 0.06f;
			projectile.velocity *= 0.997f;

			projectile.velocity.Y += 0.01f;

			if (Main.rand.Next(15) == 0)
			{
				Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(8), ModContent.DustType<Dusts.Glow>(), projectile.velocity * -Main.rand.NextFloat(1), 0, color * 0.8f, 0.3f);

				var d = Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(8), ModContent.DustType<Dusts.AuroraFast>(), projectile.velocity * Main.rand.NextFloat(2, 3), 0, color * 0.4f, 0.5f);
				d.customData = Main.rand.NextFloat(0.8f, 1.1f);
			}

			ManageCaches();
			ManageTrail();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var tex = ModContent.GetTexture(Texture);
			var texOutline = ModContent.GetTexture(Texture + "Outline");
			var drawColor = color;

			if (projectile.timeLeft < 30)
				drawColor = color * (projectile.timeLeft / 30f);

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, drawColor * 0.3f, projectile.rotation, tex.Size() / 2, projectile.scale, 0, 0);
			spriteBatch.Draw(texOutline, projectile.Center - Main.screenPosition, null, drawColor * 1.5f, projectile.rotation, texOutline.Size() / 2, projectile.scale, 0, 0);

			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (State == 0)
			{
				projectile.timeLeft = 30;
				projectile.damage = 0;
				projectile.velocity *= 0;
				projectile.penetrate = -1;
				projectile.tileCollide = false;

				State = 1;
			}
		}

		public override bool PreKill(int timeLeft)
		{
			if (projectile.penetrate == 1)
				creator.amountToThrow = 3;

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			var tex = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color * 0.5f, projectile.rotation, tex.Size() / 2, projectile.scale, 0, 0);
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 100; i++)
				{
					cache.Add(projectile.Center);
				}
			}

			cache.Add(projectile.Center);

			while (cache.Count > 100)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(40 * 4), factor => factor * 16, factor =>
			{
				if (factor.X >= 0.98f)
					return Color.White * 0;

				return new Color(color.R, color.G - 30, color.B) * 0.4f * (float)Math.Pow(factor.X, 2) * (float)Math.Sin(projectile.timeLeft / 150f * 3.14f);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = projectile.Center + projectile.velocity;
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
		}
	}

	class StarGlaive : ModProjectile, IDrawAdditive, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public Color color = new Color(100, 210, 255);

		public ref float State => ref projectile.ai[0];

		public Player Owner => Main.player[projectile.owner];

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetDefaults()
		{
			projectile.width = 62;
			projectile.height = 62;
			projectile.timeLeft = 600;
			projectile.extraUpdates = 3;
			projectile.magic = true;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.scale = 0.85f;
		}

		public override void AI()
		{
			projectile.rotation += 0.06f;

			if (projectile.velocity.Length() < 8)
				projectile.velocity = Vector2.Normalize(projectile.velocity) * 8;

			if (Main.rand.Next(15) == 0)
			{
				Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(30), ModContent.DustType<Dusts.Glow>(), projectile.velocity * -Main.rand.NextFloat(1), 0, color * 0.8f, 0.3f);

				var d = Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(30), ModContent.DustType<Dusts.AuroraFast>(), projectile.velocity * Main.rand.NextFloat(2, 3), 0, color * 0.4f, 0.5f);
				d.customData = Main.rand.NextFloat(0.8f, 1.1f);
			}

			ManageCaches();
			ManageTrail();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Helper.PlayPitched("Yeehaw", 1, 0, projectile.Center);
			projectile.velocity *= -1;
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var tex = ModContent.GetTexture(Texture);
			var texOutline = ModContent.GetTexture(Texture + "Outline");
			var drawColor = color;

			if (projectile.timeLeft < 30)
				drawColor = color * (projectile.timeLeft / 30f);

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, drawColor * 0.3f, projectile.rotation, tex.Size() / 2, projectile.scale, 0, 0);
			spriteBatch.Draw(texOutline, projectile.Center - Main.screenPosition, null, drawColor * 1.5f, projectile.rotation, texOutline.Size() / 2, projectile.scale, 0, 0);

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			var tex = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color * 0.5f, projectile.rotation, tex.Size() / 2, projectile.scale * 1.5f, 0, 0);
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 100; i++)
				{
					cache.Add(projectile.Center);
				}
			}

			cache.Add(projectile.Center);

			while (cache.Count > 100)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(40 * 4), factor => factor * 32, factor =>
			{
				if (factor.X >= 0.98f)
					return Color.White * 0;

				return new Color(color.R, color.G - 30, color.B) * 0.3f * factor.X * (float)Math.Sin(projectile.timeLeft / 600f * 3.14f);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = projectile.Center + projectile.velocity;
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
		}
	}
}
