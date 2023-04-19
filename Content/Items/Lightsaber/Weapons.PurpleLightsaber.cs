using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Lightsaber
{
	public class LightsaberProj_Purple : LightsaberProj, IDrawPrimitive, IDrawAdditive
	{
		protected override Vector3 BladeColor => Color.Purple.ToVector3();

		public List<Vector2> cache;
		public List<Vector2> cache2;
		public Trail trail;
		public Trail trail2;

		public List<Vector2> cache3;
		public List<Vector2> cache4;
		public Trail trail3;
		public Trail trail4;

		public List<Vector2> cache5;
		public List<Vector2> cache6;
		public Trail trail5;
		public Trail trail6;

		private NPC zapTarget;

		private Vector2 midPoint;
		private Vector2 midPointDirection = Vector2.Zero;
		private int trail3Start = 4;
		private int trail5Start = 4;
		private Vector2 midPoint2;
		private Vector2 midPointDirection2 = Vector2.Zero;
		private Vector2 midPoint3;
		private Vector2 midPointDirection3 = Vector2.Zero;

		private int hitCounter = 0;

		private float curveCounter = 0;

		private int originCounter = 0;

		private float backRotation = 0f;

		private Vector2 lightningOrigin = Vector2.Zero;

		protected override void RightClickBehavior()
		{
			if (!initialized)
			{
				initialized = true;
				anchorPoint = Vector2.Zero;

				oldRotation = new List<float>();
				oldPositionDrawing = new List<Vector2>();
				oldSquish = new List<float>();
				oldPositionCollision = new List<Vector2>();
			}

			updatePoints = true;
			Projectile.ownerHitCheck = false;
			Owner.ChangeDir((Main.MouseWorld.X > Owner.Center.X) ? 1 : -1);
			midPoint += midPointDirection;
			midPoint2 += midPointDirection2;
			hitCounter++;
			Owner.itemTime = Owner.itemAnimation = 2;
			hide = false;
			canHit = false;
			backRotation = Owner.DirectionTo(Main.MouseWorld).ToRotation();
			Projectile.velocity = Vector2.Zero;
			lightningOrigin = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, backRotation - 1.57f);
			Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, backRotation - 1.57f);

			float rotationOffset = Math.Sign(Owner.velocity.X) * EaseFunction.EaseQuadIn.Ease(MathHelper.Min(Math.Abs(Owner.velocity.X * 0.1f), 0.65f));
			Projectile.rotation = (Owner.direction == 1 ? 3.4f : 1.2f) + rotationOffset;
			Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Owner.direction + rotationOffset) + new Vector2(Owner.direction * 15, -2 - Owner.direction);
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.direction + rotationOffset);
			Owner.heldProj = Projectile.whoAmI;
			squish = 0.7f;
			zapTarget = Main.npc.Where(x => x.active && !x.townNPC && x.Distance(Owner.Center) < 300).OrderBy(x => x.Distance(Owner.Center)).FirstOrDefault();
			midRotation = 0;
			anchorPoint = Projectile.Center - Main.screenPosition;
			Owner.heldProj = Projectile.whoAmI;
			rotVel = 0f;

			if (zapTarget != default && !Main.dedServ)
			{
				if (originCounter % 20 == 0)
				{
					if (Main.rand.NextBool())
					{
						if (trail3Start > 0)
							trail3Start--;
					}
					else if (trail3Start < 13)
					{
						trail3Start++;
					}

					if (Main.rand.NextBool())
					{
						if (trail5Start > 0)
							trail5Start--;
					}
					else if (trail5Start < 13)
					{
						trail5Start++;
					}
				}

				if (originCounter++ % 50 == 0)
				{
					midPoint = CalculateMidpoint(zapTarget);
					midPointDirection = Main.rand.NextFloat(0.5f) * Projectile.DirectionTo(midPoint);

					midPoint2 = CalculateMidpointBranch(zapTarget);
					midPointDirection2 = Main.rand.NextFloat(0.25f) * Projectile.DirectionTo(midPoint2);

					midPoint3 = CalculateMidpointBranch(zapTarget);
					midPointDirection3 = Main.rand.NextFloat(0.25f) * Projectile.DirectionTo(midPoint3);
				}

				ManageCaches();
				ManageTrails();
			}

			if (!Main.mouseRight)
				Projectile.active = false;
			else
				Projectile.timeLeft = 30;
		}

		private void ManageCaches()
		{
			cache = new List<Vector2>();

			var curve = new BezierCurve(lightningOrigin, midPoint, zapTarget.Center);
			cache = curve.GetPoints(15);

			var curve2 = new BezierCurve(cache[trail3Start], midPoint2, zapTarget.Center);
			cache3 = curve2.GetPoints(7);

			var curve3 = new BezierCurve(cache[trail5Start], midPoint3, zapTarget.Center);
			cache5 = curve3.GetPoints(7);

			cache2 = new List<Vector2>();

			for (int i = 0; i < cache.Count; i++)
			{
				Vector2 point = cache[i];
				Vector2 nextPoint = i == cache.Count - 1 ? zapTarget.Center : cache[i + 1];
				Vector2 dir = Vector2.Normalize(nextPoint - point).RotatedBy(Main.rand.NextBool() ? -1.57f : 1.57f);

				if (i > cache.Count - 3 || dir == Vector2.Zero)
					cache2.Add(point);
				else
					cache2.Add(point + dir * Main.rand.NextFloat(5));
			}

			cache4 = new List<Vector2>();

			for (int i = 0; i < cache3.Count; i++)
			{
				Vector2 point = cache3[i];
				Vector2 nextPoint = i == cache3.Count - 1 ? zapTarget.Center : cache3[i + 1];
				Vector2 dir = Vector2.Normalize(nextPoint - point).RotatedBy(Main.rand.NextBool() ? -1.57f : 1.57f);

				if (i > cache3.Count - 1 || dir == Vector2.Zero)
					cache4.Add(point);
				else
					cache4.Add(point + dir * Main.rand.NextFloat(5));
			}

			cache6 = new List<Vector2>();

			for (int i = 0; i < cache5.Count; i++)
			{
				Vector2 point = cache5[i];
				Vector2 nextPoint = i == cache5.Count - 1 ? zapTarget.Center : cache5[i + 1];
				Vector2 dir = Vector2.Normalize(nextPoint - point).RotatedBy(Main.rand.NextBool() ? -1.57f : 1.57f);

				if (i > cache5.Count - 1 || dir == Vector2.Zero)
					cache6.Add(point);
				else
					cache6.Add(point + dir * Main.rand.NextFloat(5));
			}
		}

		private void ManageTrails()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => Main.rand.NextFloat(0.75f, 1.25f) * 16, factor =>
			{
				if (factor.X > 0.99f)
					return Color.Transparent;

				return Color.Purple * 0.2f * EaseFunction.EaseCubicOut.Ease(1 - factor.X);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = zapTarget.Center;
			trail2 ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 3 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
			{
				float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
				return Color.Lerp(new Color(200, 150, 200), new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), 1 - progress) * progress;
			});

			trail2.Positions = cache2.ToArray();
			trail2.NextPosition = zapTarget.Center;

			trail3 ??= new Trail(Main.instance.GraphicsDevice, 7, new TriangularTip(4), factor => Main.rand.NextFloat(0.75f, 1.25f) * 16, factor =>
			{
				if (factor.X > 0.99f)
					return Color.Transparent;

				return Color.Purple * 0.2f * EaseFunction.EaseCubicOut.Ease(1 - factor.X) * ((100 - originCounter % 50) / 100f);
			});

			trail3.Positions = cache3.ToArray();
			trail3.NextPosition = zapTarget.Center;
			trail4 ??= new Trail(Main.instance.GraphicsDevice, 7, new TriangularTip(4), factor => 3 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
			{
				float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
				return Color.Lerp(new Color(200, 150, 200), new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), 1 - progress) * progress * ((100 - originCounter % 50) / 100f);
			});

			trail4.Positions = cache4.ToArray();
			trail4.NextPosition = zapTarget.Center;

			trail5 ??= new Trail(Main.instance.GraphicsDevice, 7, new TriangularTip(4), factor => Main.rand.NextFloat(0.75f, 1.25f) * 16, factor =>
			{
				if (factor.X > 0.99f)
					return Color.Transparent;

				return Color.Purple * 0.2f * EaseFunction.EaseCubicOut.Ease(1 - factor.X) * ((100 - originCounter % 50) / 100f);
			});

			trail5.Positions = cache5.ToArray();
			trail5.NextPosition = zapTarget.Center;

			trail6 ??= new Trail(Main.instance.GraphicsDevice, 7, new TriangularTip(4), factor => 3 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
			{
				float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
				return Color.Lerp(new Color(200, 150, 200), new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), 1 - progress) * progress * ((100 - originCounter % 50) / 100f);
			});

			trail6.Positions = cache6.ToArray();
			trail6.NextPosition = zapTarget.Center;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (rightClicked)
			{
				hitCounter = 0;
				return true;
			}

			return base.Colliding(projHitbox, targetHitbox);
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (rightClicked)
			{
				if (target == zapTarget && hitCounter > 100)
					return true;

				return false;
			}

			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hitinfo, int damageDone)
		{
			base.OnHitNPC(target, hitinfo, damageDone);

			if (rightClicked)
				CameraSystem.shake -= 2;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (rightClicked)
			{
				modifiers.Knockback *= 0; // previously: knockback = 0;

				for (int i = 0; i < 5; i++)
					Dust.NewDustPerfect(target.Center, ModContent.DustType<LightsaberGlow>(), Main.rand.NextVector2Circular(2, 2), 0, Color.Purple, Main.rand.NextFloat(0.45f, 0.85f));
			}

			base.ModifyHitNPC(target, ref modifiers);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (!rightClicked || zapTarget == default)
				return;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "Glow").Value;

			for (int k = 0; k < 9; k++)
			{
				spriteBatch.Draw(tex, lightningOrigin - Main.screenPosition, null, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), 0, tex.Size() / 2, Projectile.scale * 0.2f, SpriteEffects.None, 0f);
				spriteBatch.Draw(tex, lightningOrigin - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, Projectile.scale * 0.15f, SpriteEffects.None, 0f);
			}
		}

		public void DrawPrimitives()
		{
			if (zapTarget == default)
				return;

			Effect effect = Filters.Scene["LightningTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			trail?.Render(effect);
			trail2?.Render(effect);
			trail3?.Render(effect);
			trail4?.Render(effect);
			trail5?.Render(effect);
			trail6?.Render(effect);
		}

		private Vector2 CalculateMidpoint(NPC target)
		{
			Vector2 directionTo = Projectile.DirectionTo(target.Center);
			return Projectile.Center + directionTo.RotatedBy(-Math.Sign(directionTo.X) * Main.rand.NextFloat(0.5f, 1f)) * Main.rand.NextFloat(0.5f, 1f) * Projectile.Distance(target.Center);
		}

		private Vector2 CalculateMidpointBranch(NPC target)
		{
			Vector2 directionTo = Projectile.DirectionTo(target.Center);
			return Projectile.Center + directionTo.RotatedBy(-Math.Sign(directionTo.X) * -Main.rand.NextFloat(-1.57f, 1.57f)) * Main.rand.NextFloat(0.25f, 0.5f) * Projectile.Distance(target.Center);
		}
	}
}