using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Lightsaber
{
	public class LightsaberGProj : GlobalProjectile
	{
		public Entity parent = default;

		public override bool InstancePerEntity => true;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_Parent spawnSource)
				parent = spawnSource.Entity;
		}
	}

	public class LightsaberPlayer : ModPlayer
	{
		public int whiteCooldown = -1;
		public bool dashing = false;

		public bool jumping = false;
		public Vector2 jumpVelocity = Vector2.Zero;

		public float storedBodyRotation = 0f;

		public override void ResetEffects()
		{
			if (whiteCooldown > 1 || Player.itemAnimation == 0)
				whiteCooldown--;
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (dashing)
				base.ModifyHurt(ref modifiers);
		}

		public override void PreUpdate()
		{
			if (dashing || jumping)
				Player.maxFallSpeed = 2000f;

			if (whiteCooldown == 0)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item9 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Player.Center);
				var dust = Dust.NewDustPerfect(Player.Center, ModContent.DustType<LightsaberStar>(), Vector2.Zero, 0, new Color(200, 200, 255, 0), 0.3f);
				dust.customData = Player.whoAmI;
			}
		}

		public override void PostUpdate()
		{
			if (jumping)
			{
				Player.mount?.Dismount(Player);
				storedBodyRotation += 0.3f * Player.direction;
				Player.fullRotation = storedBodyRotation;
				Player.fullRotationOrigin = Player.Size / 2;
			}

			if (Player.velocity.X == 0 || Player.velocity.Y == 0)
				dashing = false;

			if (Player.velocity.Y == 0)
			{
				storedBodyRotation = 0;
				Player.fullRotation = 0;
				jumping = false;
			}
			else
			{
				jumpVelocity = Player.velocity;
			}
		}
	}

	public class LightsaberGlow : Dusts.Glow
	{
		public override bool Update(Dust dust)
		{
			dust.scale *= 0.95f;
			dust.velocity *= 0.98f;
			dust.color *= 1.05f;
			return base.Update(dust);
		}
	}

	public class LightsaberGlowSoft : LightsaberGlow
	{
		public override string Texture => AssetDirectory.Keys + "GlowVerySoft";

		public override bool Update(Dust dust)
		{
			dust.scale *= 0.95f;
			dust.velocity = Vector2.Zero;
			return base.Update(dust);
		}
	}

	public class LightsaberLight : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			dust.scale *= 0.96f;
			if (dust.scale < 0.05f)
				dust.active = false;
			Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale * 2);
			return false;
		}
	}

	public class LightsaberStar : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Keys/GlowStar";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 74, 74);
			dust.noLight = true;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color * (1 - dust.alpha / 255f);
		}

		public override bool Update(Dust dust)
		{
			Player owner = Main.player[(int)dust.customData];

			dust.position = owner.position + new Vector2(9 * owner.direction, 19);

			dust.alpha += 10;

			if (!dust.noLight)
				Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.2f);

			if (dust.alpha > 255)
				dust.active = false;
			return false;
		}
	}

	public class LightsaberImpactRing : ModProjectile
	{
		public Color outerColor = Color.Orange;
		public int ringWidth = 28;
		public bool additive = false;

		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		public int timeLeftStart = 10;

		private float Progress => 1 - Projectile.timeLeft / (float)timeLeftStart;

		private float Radius => Projectile.ai[0] * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override string Texture => AssetDirectory.Assets + "Invisible";

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = timeLeftStart;
			Projectile.extraUpdates = 1;
			Projectile.hide = true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lightsaber");
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}

		public override void AI()
		{
			Projectile.velocity *= 0.95f;

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			DrawPrimitives();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			return false;
		}

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			float radius = Radius;

			for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = i / 32f * 6.28f;
				var offset = new Vector2((float)Math.Sin(rad) * 0.4f, (float)Math.Cos(rad));
				offset *= radius;
				offset = offset.RotatedBy(Projectile.ai[1]);
				cache.Add(Projectile.Center + offset);
			}

			while (cache.Count > 33)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{

			trail ??= new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => ringWidth * (1 - Progress), factor => outerColor);

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => ringWidth * 0.36f * (1 - Progress), factor => Color.White);
			float nextplace = 33f / 32f;
			var offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
			offset *= Radius;

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + offset;

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + offset;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(1);

			BlendState oldState = Main.graphics.GraphicsDevice.BlendState;
			if (additive)
				Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;
			trail?.Render(effect);
			trail2?.Render(effect);

			Main.graphics.GraphicsDevice.BlendState = oldState;
		}
	}
}