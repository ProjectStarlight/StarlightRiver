using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Lightsaber
{
	public class LightsaberProj_Red : LightsaberProj
	{
		bool releasedRight = false;

		int pullTimer = 0;
		NPC pullTarget;

		private bool targetNoGrav = false;

		private Vector2 pullDirection = Vector2.Zero;
		private Vector2 launchVector = Vector2.Zero;

		private int pauseTime = 0;

		protected override Vector3 BladeColor => Color.DarkRed.ToVector3() * 1.3f;

		protected override void RightClickBehavior()
		{
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			Owner.heldProj = Projectile.whoAmI;

			if (!releasedRight && Main.mouseRight)
			{
				Projectile.timeLeft = 30;
				hide = true;
				canHit = false;

				if (pullTimer == 0)
					pullTarget = Main.npc.Where(x => x.active && x.knockBackResist > 0 && !x.boss && !x.townNPC && x.Distance(Main.MouseWorld) < 200 && x.Distance(Owner.Center) < 500).OrderBy(x => x.Distance(Main.MouseWorld)).FirstOrDefault();

				if (pullTarget != default)
				{
					if (pullTimer == 0)
					{
						targetNoGrav = pullTarget.noGravity;
						pullTarget.noGravity = true;
					}

					pullDirection = Owner.DirectionTo(pullTarget.Center);
					pullTarget.velocity = -pullDirection * EaseFunction.EaseQuinticIn.Ease(MathHelper.Clamp(pullTimer / 150f, 0, 1)) * 12;
					Projectile.rotation = pullDirection.ToRotation();

					if (pullTarget.Distance(Owner.Center) < 5)
						releasedRight = true;

					Vector2 dustVel = pullDirection.RotatedByRandom(0.8f) * Main.rand.NextFloat();
					Dust.NewDustPerfect(pullTarget.Center - dustVel * 45, ModContent.DustType<Dusts.Glow>(), dustVel * 3, 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(0.25f, 0.45f));
				}
				else
				{
					Projectile.rotation = Owner.DirectionTo(Main.MouseWorld).ToRotation();
				}

				pullTimer++;
			}
			else
			{
				if (pullTarget != default)
					pullTarget.noGravity = targetNoGrav;

				if (!releasedRight)
				{
					float rot = Projectile.rotation;

					if (Owner.direction == 1)
						facingRight = true;
					else
						facingRight = false;

					midRotation = rot;
					canHit = true;
					releasedRight = true;
					hide = false;

					anchorPoint = Vector2.Zero;
					endRotation = rot - 2f * Owner.direction;

					oldRotation = new List<float>();
					oldPositionDrawing = new List<Vector2>();
					oldSquish = new List<float>();
					oldPositionCollision = new List<Vector2>();

					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Owner.Center);

					startRotation = endRotation;
					startSquish = endSquish;
					endMidRotation = rot + Main.rand.NextFloat(-0.45f, 0.45f);
					startMidRotation = midRotation;
					endSquish = 0.3f;
					endRotation = rot + 3f * Owner.direction;
					attackDuration = 65;
				}

				if (Projectile.ai[0] < 1)
				{
					Projectile.timeLeft = 50;

					if (pauseTime-- <= 0)
						Projectile.ai[0] += 1f / attackDuration;

					rotVel = Math.Abs(EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]) - EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0] - 1f / attackDuration)) * 2;
				}
				else
				{
					rotVel = 0f;
				}

				float progress = EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]);

				Projectile.scale = MathHelper.Min(MathHelper.Min(growCounter++ / 30f, 1 + rotVel * 4), 1.3f);

				Projectile.rotation = MathHelper.Lerp(startRotation, endRotation, progress);
				midRotation = MathHelper.Lerp(startMidRotation, endMidRotation, progress);
				squish = MathHelper.Lerp(startSquish, endSquish, progress) + 0.35f * (float)Math.Sin(3.14f * progress);
				anchorPoint = Projectile.Center - Main.screenPosition;

				Owner.ChangeDir(facingRight ? 1 : -1);

				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
				Owner.itemAnimation = Owner.itemTime = 5;

				if (Owner.direction != 1)
					Projectile.rotation += 0.78f;

				updatePoints = pauseTime <= 0;

				if (pullTarget != null && pullTarget.active)
				{
					if (pauseTime > 0)
						pullTarget.velocity = Vector2.Zero;
					else if (pauseTime == 0)
						pullTarget.velocity = launchVector * 8 * pullTarget.knockBackResist;
				}
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (target == pullTarget)
			{
				CameraSystem.shake += 5;
				launchVector = pullTarget.DirectionTo(Main.MouseWorld);
				modifiers.FinalDamage *= 2.5f;
				target.velocity = Vector2.Zero;
				pauseTime = 40;
			}
		}
	}
}