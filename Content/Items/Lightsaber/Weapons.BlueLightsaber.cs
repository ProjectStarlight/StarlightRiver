using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Lightsaber
{
	public class LightsaberProj_Blue : LightsaberProj
	{
		protected override Vector3 BladeColor => new Vector3(0, 0.1f, 0.255f);

		bool rightClicking = false;

		bool parried = false;

		int parries = 0;

		bool startedSwing = false;

		protected override void RightClickBehavior()
		{
			Projectile.extraUpdates = 9;
			Projectile.velocity = Vector2.Zero;
			owner.heldProj = Projectile.whoAmI;
			hide = false;
			canHit = false;
			afterImageLength = 20;

			if (!initialized)
			{
				initialized = true;
				startRotation = endRotation = owner.DirectionTo(Main.MouseWorld).ToRotation() + (2f * owner.direction);
				startSquish = endSquish = 0.3f;
			}
			if (!startedSwing)
			{
				if (owner.DirectionTo(Main.MouseWorld).X > 0)
					facingRight = true;
				else
					facingRight = false;
				rightClicking = true;
				Projectile.timeLeft = 200;
				Vector2 hitboxCenter = owner.Center + (owner.DirectionTo(Main.MouseWorld) * 40);
				var hitbox = new Rectangle((int)hitboxCenter.X, (int)hitboxCenter.Y, 0, 0);
				hitbox.Inflate(55, 55);

				Projectile deflection = Main.projectile.Where(n => n.active && n.hostile && n.Hitbox.Intersects(hitbox)).FirstOrDefault();

				float rot = owner.DirectionTo(Main.MouseWorld).ToRotation();
				startRotation = endRotation;
				startSquish = endSquish;
				endMidRotation = rot + Main.rand.NextFloat(-0.45f, 0.45f);
				startMidRotation = midRotation;
				endSquish = 0.3f;
				endRotation = rot + ((2f * owner.direction) * (((parries++ % 2) == 1) ? 1 : -1));
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
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), deflection.Center, laserVel * 15, ModContent.ProjectileType<Lightsaber_BlueLaser>(), Projectile.damage, 0, owner.whoAmI);
					}
				}
			}

			else if (parried)
			{
				if (rightClicking && !Main.mouseRight)
					rightClicking = false;

				if (!rightClicking && Main.mouseRight)
				{
					Vector2 hitboxCenter = owner.Center + (owner.DirectionTo(Main.MouseWorld) * 40);
					Rectangle hitbox = new Rectangle((int)hitboxCenter.X, (int)hitboxCenter.Y, 0, 0);
					hitbox.Inflate(55, 55);
					Projectile deflection = Main.projectile.Where(n => n.active && n.hostile && n.Hitbox.Intersects(hitbox)).FirstOrDefault();
					if (Projectile.ai[0] >= 1)
					{
						if (owner.DirectionTo(Main.MouseWorld).X > 0)
							facingRight = true;
						else
							facingRight = false;

						float rot = owner.DirectionTo(Main.MouseWorld).ToRotation();
						startRotation = endRotation;
						startSquish = endSquish;
						endMidRotation = rot + Main.rand.NextFloat(-0.45f, 0.45f);
						startMidRotation = midRotation;
						endSquish = 0.3f;
						endRotation = rot + ((2f * owner.direction) * (((parries++ % 2) == 1) ? 1 : -1));
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
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), deflection.Center, laserVel * 15, ModContent.ProjectileType<Lightsaber_BlueLaser>(), Projectile.damage, 0, owner.whoAmI);
					}
					else
						parried = false;

					rightClicking = true;
				}
			}

			if (Projectile.ai[0] < 1)
			{
				Projectile.timeLeft = 400;
				Projectile.ai[0] += 1f / attackDuration;
				rotVel = Math.Abs(EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]) - EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0] - (1f / attackDuration))) * 2;
			}
			float progress = EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]);

			Projectile.scale = MathHelper.Min(MathHelper.Min(growCounter++ / 30f, 1 + (rotVel * 4)), 1.3f);

			Projectile.rotation = MathHelper.Lerp(startRotation, endRotation, progress);
			midRotation = MathHelper.Lerp(startMidRotation, endMidRotation, progress);
			squish = MathHelper.Lerp(startSquish, endSquish, progress) + (0.35f * (float)Math.Sin(3.14f * progress));
			anchorPoint = Projectile.Center - Main.screenPosition;

			owner.ChangeDir(facingRight ? 1 : -1);

			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			owner.itemAnimation = owner.itemTime = 2;

			if (owner.direction != 1)
				Projectile.rotation += 0.78f;

			Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
		}
	}
}