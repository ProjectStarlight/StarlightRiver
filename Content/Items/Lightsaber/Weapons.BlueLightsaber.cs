using System;
using System.Linq;

namespace StarlightRiver.Content.Items.Lightsaber
{
	public class BlueLightsaberProjectile : LightsaberProj
	{
		bool rightClicking = false;

		bool parried = false;
		int parries = 0;

		bool startedSwing = false;

		protected override Vector3 BladeColor => new(0, 0.1f, 0.255f);

		protected override void RightClickBehavior()
		{
			Projectile.extraUpdates = 9;
			Projectile.velocity = Vector2.Zero;
			Owner.heldProj = Projectile.whoAmI;
			hide = false;
			canHit = false;
			afterImageLength = 20;

			if (!initialized)
			{
				initialized = true;
				startRotation = endRotation = Owner.DirectionTo(Main.MouseWorld).ToRotation() + 2f * Owner.direction;
				startSquish = endSquish = 0.3f;
			}

			if (!startedSwing)
			{
				if (Owner.DirectionTo(Main.MouseWorld).X > 0)
					facingRight = true;
				else
					facingRight = false;

				rightClicking = true;
				Projectile.timeLeft = 200;
				Vector2 hitboxCenter = Owner.Center + Owner.DirectionTo(Main.MouseWorld) * 40;
				var hitbox = new Rectangle((int)hitboxCenter.X, (int)hitboxCenter.Y, 0, 0);
				hitbox.Inflate(55, 55);

				Projectile deflection = Main.projectile.Where(n => n.active && n.hostile && n.Hitbox.Intersects(hitbox)).FirstOrDefault();

				float rot = Owner.DirectionTo(Main.MouseWorld).ToRotation();
				startRotation = endRotation;
				startSquish = endSquish;
				endMidRotation = rot + Main.rand.NextFloat(-0.45f, 0.45f);
				startMidRotation = midRotation;
				endSquish = 0.3f;
				endRotation = rot + 2f * Owner.direction * (((parries++ % 2) == 1) ? 1 : -1);
				attackDuration = 90;
				Projectile.ai[0] = 0;

				startedSwing = true;

				if (deflection != default)
				{
					parried = true;
					deflection.penetrate--;

					if (deflection.penetrate == 0)
					{
						deflection.active = false;

						Vector2 laserVel = deflection.DirectionTo(Main.MouseWorld);

						if (deflection.GetGlobalProjectile<LightsaberGProj>().parent != default)
							laserVel = deflection.DirectionTo(deflection.GetGlobalProjectile<LightsaberGProj>().parent.Center);

						Projectile.NewProjectile(Projectile.GetSource_FromThis(), deflection.Center, laserVel * 15, ModContent.ProjectileType<BlueLightsaberLaser>(), Projectile.damage, 0, Owner.whoAmI);
					}
				}
			}
			else if (parried)
			{
				if (rightClicking && !Main.mouseRight)
					rightClicking = false;

				if (!rightClicking && Main.mouseRight)
				{
					Vector2 hitboxCenter = Owner.Center + Owner.DirectionTo(Main.MouseWorld) * 40;
					var hitbox = new Rectangle((int)hitboxCenter.X, (int)hitboxCenter.Y, 0, 0);
					hitbox.Inflate(55, 55);
					Projectile deflection = Main.projectile.Where(n => n.active && n.hostile && n.Hitbox.Intersects(hitbox)).FirstOrDefault();

					if (Projectile.ai[0] >= 1)
					{
						if (Owner.DirectionTo(Main.MouseWorld).X > 0)
							facingRight = true;
						else
							facingRight = false;

						float rot = Owner.DirectionTo(Main.MouseWorld).ToRotation();
						startRotation = endRotation;
						startSquish = endSquish;
						endMidRotation = rot + Main.rand.NextFloat(-0.45f, 0.45f);
						startMidRotation = midRotation;
						endSquish = 0.3f;
						endRotation = rot + 2f * Owner.direction * (((parries++ % 2) == 1) ? 1 : -1);
						attackDuration = 90;
						Projectile.ai[0] = 0;
						parried = true;
					}

					if (deflection != default)
					{
						deflection.penetrate--;

						if (deflection.penetrate == 0)
							deflection.active = false;

						Vector2 laserVel = deflection.DirectionTo(Main.MouseWorld);

						if (deflection.GetGlobalProjectile<LightsaberGProj>().parent != default)
							laserVel = deflection.DirectionTo(deflection.GetGlobalProjectile<LightsaberGProj>().parent.Center);

						Projectile.NewProjectile(Projectile.GetSource_FromThis(), deflection.Center, laserVel * 15, ModContent.ProjectileType<BlueLightsaberLaser>(), Projectile.damage, 0, Owner.whoAmI);
					}
					else
					{
						parried = false;
					}

					rightClicking = true;
				}
			}

			if (Projectile.ai[0] < 1)
			{
				Projectile.timeLeft = 400;
				Projectile.ai[0] += 1f / attackDuration;
				rotVel = Math.Abs(EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]) - EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0] - 1f / attackDuration)) * 2;
			}

			float progress = EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]);

			Projectile.scale = MathHelper.Min(MathHelper.Min(growCounter++ / 30f, 1 + rotVel * 4), 1.3f);

			Projectile.rotation = MathHelper.Lerp(startRotation, endRotation, progress);
			midRotation = MathHelper.Lerp(startMidRotation, endMidRotation, progress);
			squish = MathHelper.Lerp(startSquish, endSquish, progress) + 0.35f * (float)Math.Sin(3.14f * progress);
			anchorPoint = Projectile.Center - Main.screenPosition;

			Owner.ChangeDir(facingRight ? 1 : -1);

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			Owner.itemAnimation = Owner.itemTime = 2;

			if (Owner.direction != 1)
				Projectile.rotation += 0.78f;

			Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
		}
	}

	class BlueLightsaberLensFlare : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.Keys + "Glow";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Laser");
		}

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 60;
			Projectile.tileCollide = false;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 4;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			float scale = MathHelper.Min(1 - (60 - Projectile.timeLeft) / 60f, 1);

			for (int k = 0; k < 9; k++)
			{
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), 0, tex.Size() / 2, Projectile.scale * scale * 0.7f, SpriteEffects.None, 0f);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, Projectile.scale * scale * 0.5f, SpriteEffects.None, 0f);
			}
		}
	}

	class BlueLightsaberLaser : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.VitricBoss + "RoarLine";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Laser");
		}

		private bool initialized = false;

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 1060;
			Projectile.tileCollide = true;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public override void AI()
		{
			Lighting.AddLight(Projectile.Center, new Vector3(0, 0.1f, 0.255f));
			Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

			if (!initialized)
			{
				initialized = true;
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlueLightsaberLensFlare>(), 0, 0, Projectile.owner);
			}
		}

		public override void Kill(int timeLeft)
		{

		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), Projectile.rotation, tex.Size() / 2, Projectile.scale * new Vector2(1, 0.6f) * 1.5f, SpriteEffects.None, 0f);

			for (int i = 0; i < 5; i++)
			{
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), Projectile.rotation, tex.Size() / 2, Projectile.scale * new Vector2(1, 0.9f), SpriteEffects.None, 0f);
			}

			for (int k = 0; k < 9; k++)
			{
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale * 0.9f * new Vector2(1, 0.9f), SpriteEffects.None, 0f);
			}

			for (int l = 0; l < 2; l++)
			{
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), Projectile.rotation, tex.Size() / 2, Projectile.scale * 2f * new Vector2(1, 0.9f), SpriteEffects.None, 0f);
			}
		}
	}
}