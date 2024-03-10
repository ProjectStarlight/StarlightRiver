using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc.SoilgunFiles
{
	// not sure if its worth making this inherit SoilProjectile, most things have to be changed from it.
	public abstract class SoilClumpProjectile : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private Vector2[] clumpPositions = new Vector2[5];

		public int dustID;

		public bool gravity = true;

		public bool drawTrail = true;

		public Dictionary<string, Color> Colors = new()
		{
			{ "SmokeColor", Color.White },
			{ "TrailColor", Color.White },
			{ "TrailInsideColor", Color.White },
			{ "RingOutsideColor", Color.White },
			{ "RingInsideColor", Color.White },
		};

		public float AmmoType => Projectile.ai[0];

		public ref float Time => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible; //using the item id for texture was dumb when it can just be requested

		protected SoilClumpProjectile(Color trailColor, Color trailInsideColor, Color ringOutsideColor, Color ringInsideColor, Color smokeColor, int dustID)
		{
			Colors["TrailColor"] = trailColor;
			Colors["TrailInsideColor"] = trailInsideColor;
			Colors["RingOutsideColor"] = ringOutsideColor;
			Colors["RingInsideColor"] = ringInsideColor;
			Colors["SmokeColor"] = smokeColor;

			this.dustID = dustID;
		}

		public sealed override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Soil");
		}

		public virtual void SafeSetDefaults() { }

		public sealed override void SetDefaults()
		{
			Projectile.penetrate = 1;
			SafeSetDefaults();

			Projectile.DamageType = DamageClass.Ranged;
			Projectile.Size = new Vector2(32);
			Projectile.friendly = true;
			Projectile.timeLeft = 500;

			for (int i = 0; i < clumpPositions.Length; i++)
			{
				clumpPositions[i] = new Vector2(Main.rand.Next(-9, 9), Main.rand.Next(-9, 9));
			}
		}

		public virtual void SafeAI() { }

		public sealed override void AI()
		{
			SafeAI();

			Time++;

			Projectile.rotation += Projectile.velocity.Length() * 0.01f;

			if (Projectile.timeLeft < 480 && gravity)
			{
				if (Projectile.velocity.Y < 18f)
				{
					Projectile.velocity.Y += 0.35f;

					if (Projectile.velocity.Y > 0)
					{
						if (Projectile.velocity.Y < 12f)
							Projectile.velocity.Y *= 1.060f;
						else
							Projectile.velocity.Y *= 1.03f;
					}
				}
			}

			for (int i = 0; i < 2; i++)
			{
				Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<SoilgunSmoke>(),
				 Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(180, 220), Colors["SmokeColor"], Main.rand.NextFloat(0.03f, 0.05f));

				if (Main.rand.NextBool(6))
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), dustID, Vector2.Zero, 100).noGravity = true;
			}

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public virtual void SafeOnKill()
		{

		}

		public sealed override void OnKill(int timeLeft)
		{
			SafeOnKill();

			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

			for (int i = 0; i < 12; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, dustID, -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.5f),
					Main.rand.Next(100), default, Main.rand.NextFloat(1f, 1.5f)).noGravity = true;

				Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelSmokeColor>(), -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.5f),
					Main.rand.Next(125, 180), Colors["TrailColor"], Main.rand.NextFloat(0.03f, 0.05f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = Colors["TrailInsideColor"];
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Colors["RingInsideColor"] with { A = 0 }, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.75f, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Colors["RingOutsideColor"] with { A = 0 }, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.55f, 0f, 0f);

			for (int i = 0; i < clumpPositions.Length; i++)
			{
				Main.spriteBatch.Draw(tex, Projectile.Center + clumpPositions[i].RotatedBy(Projectile.rotation) - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0f, 0f);
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0f, 0f);

			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 18; i++)
				{
					cache.Add(Projectile.Center + Projectile.velocity);
				}
			}

			cache.Add(Projectile.Center + Projectile.velocity);

			while (cache.Count > 18)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 18, new TriangularTip(4), factor => 14, factor => Colors["TrailColor"] * factor.X);

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 18, new TriangularTip(4), factor => 7, factor => Colors["TrailInsideColor"] * factor.X * 0.5f);

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			if (!drawTrail)
				return;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

				var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
				Matrix view = Main.GameViewMatrix.EffectMatrix;
				var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
				effect.Parameters["repeats"].SetValue(1);
				effect.Parameters["transformMatrix"].SetValue(world * view * projection);
				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value);

				trail?.Render(effect);
				trail2?.Render(effect);
			});
		}
	}
	public class SoilgunDirtClump : SoilClumpProjectile
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.DirtBlock;
		public SoilgunDirtClump() : base(new Color(30, 19, 12), new Color(60, 35, 20), new Color(81, 47, 27), new Color(105, 67, 44), new Color(82, 45, 22), DustID.Dirt) { }
		public override void SafeOnKill()
		{

		}
	}
}
