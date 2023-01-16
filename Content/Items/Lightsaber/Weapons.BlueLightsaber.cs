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
}