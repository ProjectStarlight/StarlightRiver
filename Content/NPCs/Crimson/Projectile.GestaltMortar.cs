using StarlightRiver.Content.Buffs;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Crimson
{
	class GestaltMortar : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;

		private bool initialized = false;

		public override string Texture => AssetDirectory.Invisible;

		public Color BaseColor => Projectile.ai[0] == 0 ? new Color(0.65f, 0.64f, 0.87f) : Projectile.ai[0] == 1 ? new Color(0.5f, 0.87f, 0.87f) : new Color(0.95f, 0.59f, 0.78f);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Mind Mortar");
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 600;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			if (!initialized)
			{
				initialized = true;
				Projectile.netUpdate = true;

				Helpers.SoundHelper.PlayPitched("Magic/Shadow1", 0.1f, 0.5f, Projectile.Center);
			}

			Projectile.velocity.Y += 0.6f;

			Color color = BaseColor * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);

			if (Main.rand.NextBool(2))
			{
				color.A = 0;
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Projectile.velocity.RotatedByRandom(0.1f) * -Main.rand.NextFloat(0.3f, 0.9f), 0, color, Main.rand.NextFloat(0.1f, 0.2f));
			}

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			BuffInflictor.Inflict<Neurosis>(target, Main.masterMode ? 4000 : Main.expertMode ? 2000 : 1000);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D glow = Assets.Masks.GlowAlpha.Value;
			Texture2D star = Assets.StarTexture.Value;

			Color color = BaseColor * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);
			color.A = 0;

			Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, color * 0.5f, 0, glow.Size() / 2f, 0.5f, 0, 0);
			Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, color, Main.GameUpdateCount * 0.1f, star.Size() / 2f, 0.25f, 0, 0);
			Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, color * 1.8f, Main.GameUpdateCount * -0.2f, star.Size() / 2f, 0.15f, 0, 0);

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.025f);
					effect.Parameters["repeats"].SetValue(1f);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);
					trail?.Render(effect);

					effect.Parameters["sampleTexture"].SetValue(Assets.LightningTrail.Value);
					trail?.Render(effect);
				}
			});

			return false;
		}

		protected void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 30; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 30)
			{
				cache.RemoveAt(0);
			}
		}

		protected void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 30, new NoTip(), factor => factor * 8, factor =>
				{
					float alpha = factor.X;

					if (factor.X == 1)
						alpha = 0;

					if (Projectile.timeLeft < 20)
						alpha *= Projectile.timeLeft / 20f;

					alpha *= factor.X;

					Color color = BaseColor * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);
					return color * alpha;
				});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}
	}
}