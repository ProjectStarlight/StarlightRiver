using StarlightRiver.Helpers;
using StarlightRiver.Noise;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Permafrost
{
	class AuroraBell : ModItem
	{
		public override string Texture => AssetDirectory.PermafrostItem + "AuroraBell";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aurora Bell");
			Tooltip.SetDefault("Summons a bell sentry\nHit the bell with a whip to ring it");
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.QueenSpiderStaff);
			Item.damage = 19;
			Item.mana = 12;
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 0, 80, 0);
			Item.rare = ItemRarityID.Green;
			Item.knockBack = 2.5f;
			Item.UseSound = SoundID.Item25;
			Item.shoot = ModContent.ProjectileType<AuroraBellProj>();
			Item.shootSpeed = 0f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var proj = Projectile.NewProjectileDirect(source, Main.MouseWorld, velocity, type, damage, knockback, player.whoAmI);
			proj.originalDamage = Item.damage;
			player.UpdateMaxTurrets();

			return false;
		}
	}

	public class AuroraBellProj : ModProjectile
	{
		private int chargeCounter = 300;

		private int scaleCounter = 0;

		private float startRotation = 0f;

		private int ringDirection = 1;

		private int counter = 0;

		private Player Owner => Main.player[Projectile.owner];

		private float Opacity => Math.Min(1, Projectile.timeLeft / 40f);

		private float ChargeRatio => chargeCounter / 300f;

		public override string Texture => AssetDirectory.PermafrostItem + "AuroraBellProj";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aurora Bell");
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 28;
			Projectile.timeLeft = Projectile.SentryLifeTime;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.sentry = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.ignoreWater = true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D outlineTex = ModContent.Request<Texture2D>(Texture + "_Outline").Value;

			Vector2 offset = 8 * new Vector2((float)Math.Cos(counter * 0.05f), (float)Math.Sin(counter * 0.05f));

			float pulseProgress = chargeCounter / 40f;
			float transparency = (float)Math.Pow(MathHelper.Clamp(1 - pulseProgress, 0, 1), 2);
			float scale = (float)MathHelper.Clamp(1 + pulseProgress, 0, 2);

			Vector2 centerer = (Projectile.rotation - 1.57f).ToRotationVector2() * (tex.Height / 2);
			Main.spriteBatch.Draw(tex, Projectile.Center + offset - centerer - new Vector2(0, tex.Height / 2) - Main.screenPosition, null, Color.White * transparency * Opacity, Projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), Projectile.scale * scale, SpriteEffects.None, 0f);

			Texture2D backTex = ModContent.Request<Texture2D>(Texture + "_Back").Value;
			Texture2D frontTex = ModContent.Request<Texture2D>(Texture + "_Front").Value;
			Texture2D clapperTex = ModContent.Request<Texture2D>(Texture + "_Clapper").Value;
			centerer = (Projectile.rotation - 1.57f).ToRotationVector2() * (tex.Height * 0.75f);
			Main.spriteBatch.Draw(backTex, Projectile.Center + offset - new Vector2(0, tex.Height / 2) - Main.screenPosition, null, lightColor * Opacity, Projectile.rotation, new Vector2(tex.Width / 2, 0), Projectile.scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(clapperTex, Projectile.Center + offset - centerer - new Vector2(0, tex.Height / 2) - Main.screenPosition, null, lightColor * Opacity, -Projectile.rotation, new Vector2(tex.Width / 2, tex.Height * 0.75f), Projectile.scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(frontTex, Projectile.Center + offset - new Vector2(0, tex.Height / 2) - Main.screenPosition, null, lightColor * Opacity, Projectile.rotation, new Vector2(tex.Width / 2, 0), Projectile.scale, SpriteEffects.None, 0f);

			if (chargeCounter == 300)
				Main.spriteBatch.Draw(outlineTex, Projectile.Center + offset - new Vector2(0, tex.Height / 2) - Main.screenPosition, null, Color.White * Opacity, Projectile.rotation, new Vector2(tex.Width / 2, 0), Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}

		public override void AI()
		{
			if (scaleCounter < 15)
				scaleCounter++;

			Projectile.scale = EaseFunction.EaseCircularOut.Ease(scaleCounter / 15f);

			Projectile.rotation = (float)Math.Sin(Math.Pow(chargeCounter * 0.15f, 0.7f) * ringDirection + startRotation) * (float)Math.Pow(1 - ChargeRatio, 2f);
			Vector2 offset = 8 * new Vector2((float)Math.Cos(counter * 0.05f), (float)Math.Sin(counter * 0.05f));
			Lighting.AddLight(Projectile.Center + offset, Color.White.ToVector3() * 1.5f * (float)Math.Pow(1 - ChargeRatio, 9f));
			counter++;

			if (chargeCounter < 300)
				chargeCounter++;

			if (chargeCounter == 299)
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + offset, Vector2.Zero, ModContent.ProjectileType<AuroraBellRingSmall>(), 0, 0, Owner.whoAmI);

			if (chargeCounter < 20)
				return;

			for (int i = 0; i < Main.projectile.Length; i++)
			{
				Projectile proj = Main.projectile[i];

				if (proj == null || !proj.active || proj.damage == 0 || !(ProjectileID.Sets.IsAWhip[proj.type] || proj.type == ModContent.ProjectileType<AuroraBellRing>()))
					continue;

				ModProjectile modProj = proj.ModProjectile;

				if (modProj is AuroraBellRing ringer && ringer.cantHit.Contains(Projectile))
					continue;

				bool colliding = false;

				if (modProj != null && modProj.Colliding(proj.Hitbox, Projectile.Hitbox) != false)
				{
					if (modProj.Colliding(proj.Hitbox, Projectile.Hitbox) == null && Projectile.Colliding(proj.Hitbox, Projectile.Hitbox))
						colliding = true;

					if (modProj.Colliding(proj.Hitbox, Projectile.Hitbox) == true)
						colliding = true;
				}
				else
				{
					for (int n = 0; n < proj.WhipPointsForCollision.Count; n++)
					{
						var point = proj.WhipPointsForCollision[n].ToPoint();
						var myRect = new Rectangle(0, 0, proj.width, proj.height);
						myRect.Location = new Point(point.X - myRect.Width / 2, point.Y - myRect.Height / 2);

						if (myRect.Intersects(Projectile.Hitbox))
						{
							startRotation = (float)Math.Asin(Projectile.rotation);
							ringDirection = Math.Sign(Owner.Center.X - Projectile.Center.X);
							colliding = true;
							break;
						}
					}
				}

				if (colliding)
				{
					Helper.PlayPitched("Magic/AuroraBell", ChargeRatio, Main.rand.NextFloat(-0.1f, 0.1f) + (1 - ChargeRatio) * 0.8f, Projectile.Center);
					Core.Systems.CameraSystem.CameraSystem.shake += 7;

					DistortionPointHandler.AddPoint(Projectile.Center, (float)Math.Pow(ChargeRatio, 0.7f), 0,
					(intensity, ticksPassed) => intensity,
					(progress, ticksPassed) => (float)Math.Sqrt(ticksPassed / 20f),
					(progress, intensity, ticksPassed) => ticksPassed <= 20);

					var newProj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + offset, Vector2.Zero, ModContent.ProjectileType<AuroraBellRing>(), (int)(proj.damage * ChargeRatio), Projectile.knockBack, Owner.whoAmI, 2);
					newProj.originalDamage = (int)(proj.damage * ChargeRatio);

					if (modProj is not AuroraBellRing)
					{
						newProj.originalDamage *= 2;
						newProj.damage *= 2;
					}

					for (int j = 0; j < 12; j++)
					{
						float angle = Main.rand.NextFloat(6.28f);

						float colorVel = Main.rand.NextFloat(6.28f);
						float sin = 1 + (float)Math.Sin(colorVel);
						float cos = 1 + (float)Math.Cos(colorVel);
						Dust.NewDustPerfect(Projectile.Center + offset + angle.ToRotationVector2() * 20, ModContent.DustType<Dusts.Aurora>(), angle.ToRotationVector2() * Main.rand.NextFloat() * 8, 0, new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f), Main.rand.NextFloat(1.5f, 2.5f));
					}

					var newProjMP = newProj.ModProjectile as AuroraBellRing;

					if (modProj is AuroraBellRing oldRinger)
					{
						oldRinger.cantHit.Add(Projectile);
						newProjMP.cantHit = oldRinger.cantHit;
					}
					else
					{
						newProjMP.cantHit.Add(Projectile);
					}

					newProjMP.radiusMult = (float)Math.Pow(ChargeRatio, 0.7f);
					chargeCounter = 0;
					break;
				}
			}
		}

		public override void Kill(int timeLeft)
		{
			if (Projectile.sentry && timeLeft > 0)
			{
				var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<AuroraBellProj>(), 0, 0, Owner.whoAmI, Projectile.ai[0], Projectile.ai[1]);
				proj.timeLeft = 40;
				proj.sentry = false;

				var mp = proj.ModProjectile as AuroraBellProj;
				mp.counter = counter;
				mp.scaleCounter = scaleCounter;
				mp.chargeCounter = chargeCounter;
				mp.startRotation = startRotation;
				mp.ringDirection = ringDirection;
			}
		}
	}

	internal class AuroraBellRing : ModProjectile, IDrawPrimitive
	{
		public float radiusMult = 1f;

		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		private FastNoise noise;

		public List<Projectile> cantHit = new();

		public float Progress => 1 - Projectile.timeLeft / 20f;

		private float Radius => (150 + 15 * Projectile.ai[0]) * (float)Math.Sqrt(Progress) * radiusMult;

		public override string Texture => AssetDirectory.Assets + "Invisible";

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 20;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aurora Bell");
		}

		public override void AI()
		{
			noise ??= new FastNoise(Main.rand.Next(9999))
			{
				NoiseType = FastNoise.NoiseTypes.Perlin
			};

			noise.Frequency = MathHelper.Lerp(5, 1.5f, Progress);

			if (Projectile.timeLeft == 15 && Projectile.ai[0] > 0)
			{
				var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<AuroraBellRing>(), 0, 0, Projectile.owner, Projectile.ai[0] - 1);
				var mp = proj.ModProjectile as AuroraBellRing;
				mp.radiusMult = radiusMult;
			}

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target.whoAmI == (int)Projectile.ai[0])
				return false;

			return base.CanHitNPC(target);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;

			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
				return true;

			return false;
		}

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			float radius = Radius;

			for (int i = 0; i < 129; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = i / 128f * 6.28f;
				var offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));

				float radiusOffset = 1 + 0.15f * noise.GetNoise(offset.X, offset.Y);
				offset *= radius * radiusOffset;
				cache.Add(Projectile.Center + offset);
			}

			while (cache.Count > 129)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{

			trail ??= new Trail(Main.instance.GraphicsDevice, 129, new TriangularTip(1), factor => 34 * (1 - Progress) * radiusMult, factor => new Color(181, 0, 252));

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 129, new TriangularTip(1), factor => 30 * (1 - Progress) * radiusMult, factor =>
			{
				float sin = 1 + (float)Math.Sin(Projectile.timeLeft * 0.4f + Projectile.ai[0] * 0.6f + factor.X * 6.28f);
				float cos = 1 + (float)Math.Cos(Projectile.timeLeft * 0.4f + Projectile.ai[0] * 0.6f + factor.X * 6.28f);
				return new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
				;
			});

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
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(1);

			trail?.Render(effect);
			trail2?.Render(effect);
		}
	}

	internal class AuroraBellRingSmall : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		private float Progress => 1 - Projectile.timeLeft / 20f;

		private float Radius => 50 * (float)Math.Sqrt(Progress);

		public override string Texture => AssetDirectory.Assets + "Invisible";

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 20;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aurora Bell");
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			float radius = Radius;

			for (int i = 0; i < 129; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = i / 128f * 6.28f;
				var offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));

				offset *= radius;
				cache.Add(Projectile.Center + offset);
			}

			while (cache.Count > 129)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{

			trail ??= new Trail(Main.instance.GraphicsDevice, 129, new TriangularTip(1), factor => 20 * (1 - Progress), factor =>
			{
				float sin = 1 + (float)Math.Sin(Projectile.timeLeft * 0.4f + Projectile.ai[0] * 0.6f + factor.X * 6.28f);
				float cos = 1 + (float)Math.Cos(Projectile.timeLeft * 0.4f + Projectile.ai[0] * 0.6f + factor.X * 6.28f);
				return new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
			});

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 129, new TriangularTip(1), factor => 10 * (1 - Progress), factor =>
			{
				float sin = 1 + (float)Math.Sin(Projectile.timeLeft * 0.4f + Projectile.ai[0] * 0.6f + factor.X * 6.28f);
				float cos = 1 + (float)Math.Cos(Projectile.timeLeft * 0.4f + Projectile.ai[0] * 0.6f + factor.X * 6.28f);
				return new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
				;
			});

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
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(1);

			trail?.Render(effect);
			trail2?.Render(effect);
		}
	}
}