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

namespace StarlightRiver.Content.Items.Breacher
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

	public class LightsaberProj_Green : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Green.ToVector3();

		bool jumped = false;

		private int soundTimer = 0;
        protected override void RightClickBehavior()
        {
			if (!jumped)
            {
				jumped = true;
				owner.velocity = owner.DirectionTo(Main.MouseWorld) * 20;
				owner.GetModPlayer<LightsaberPlayer>().jumping = true;
            }

			if (soundTimer++ % 100 == 0)
			{
				hit = new List<NPC>();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, owner.Center);
			}
			owner.itemTime = owner.itemAnimation = 2;
			Projectile.timeLeft = 200;
			afterImageLength = 30;
			Projectile.rotation += 0.06f * owner.direction;
			rotVel = 0.02f;
			midRotation = owner.velocity.ToRotation();
			squish = 0.7f;
			hide = false;
			canHit = true;
			anchorPoint = owner.Center - Main.screenPosition;
			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			owner.heldProj = Projectile.whoAmI;

			if (!owner.GetModPlayer<LightsaberPlayer>().jumping)
			{
				Core.Systems.CameraSystem.Shake += 10;
				for (int i = 0; i < 30; i++)
					Dust.NewDustPerfect(owner.Bottom, ModContent.DustType<LightsaberGlow>(), Main.rand.NextVector2Circular(10, 10), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(1.95f, 2.35f));
				Projectile.active = false;
				Tile tile = Main.tile[((owner.Bottom / 16) + new Vector2(0, 1)).ToPoint()];
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitX, ModContent.ProjectileType<Lightsaber_GreenShockwave>(), (int)(Projectile.damage * 1.3f), 0, owner.whoAmI,0, 10);
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitX * -1, ModContent.ProjectileType<Lightsaber_GreenShockwave>(), (int)(Projectile.damage * 1.3f), 0, owner.whoAmI, tile.TileType, -10);
				Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), owner.Bottom, Vector2.Zero, ModContent.ProjectileType<LightsaberImpactRing>(), 0, 0, owner.whoAmI, 160, 1.57f);
				(proj.ModProjectile as LightsaberImpactRing).outerColor = new Color(BladeColor.X, BladeColor.Y, BladeColor.Z);
				(proj.ModProjectile as LightsaberImpactRing).ringWidth = 40;
				(proj.ModProjectile as LightsaberImpactRing).timeLeftStart = 50;
				(proj.ModProjectile as LightsaberImpactRing).additive = true;
				proj.timeLeft = 50;
			}
		}
    }
	public class LightsaberProj_Orange : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Orange.ToVector3();

		private bool released = false;

		private int hitTimer = 0;

		private int releaseTimer = 0;

		private Vector2 mouseStart = Vector2.Zero;
		private Vector2 playerStart = Vector2.Zero;
        protected override void ThrownBehavior()
        {

			float progressMult = 0.0075f;

			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, owner.DirectionTo(Projectile.Center).ToRotation() - 1.57f);
			if (hitTimer++ % ((Projectile.extraUpdates + 1) * 6) == 0)
				hit = new List<NPC>();
			if (throwTimer == 0)
            {
				Vector2 mouseOffset = Main.MouseWorld - owner.Center;
				int max = 400 + (int)MathHelper.Max(0, (mouseOffset.Length() - 400) * 0.2f);
				mouseStart = owner.Center + (Vector2.Normalize(mouseOffset) * MathHelper.Min(mouseOffset.Length(), max));
				playerStart = owner.Center;
				midRotation = Projectile.DirectionTo(mouseStart).ToRotation();
			}

			rotVel = 0.04f;
			squish = MathHelper.Lerp(squish, 0.8f - (Projectile.velocity.Length() * 0.04f), 0.1f);
			anchorPoint = Projectile.Center - Main.screenPosition;
			Projectile.timeLeft = 50;
			owner.itemTime = owner.itemAnimation = 2;

			if ((!Main.mouseLeft || releaseTimer++ > 1600)  && !released && Math.Cos((throwTimer + 50) * progressMult) <= 0)
			{
				throwTimer = (int)(1.57f / progressMult);
				mouseStart = Projectile.Center;
				released = true;
			}

			if (released)
			{
				playerStart = owner.Center;
				midRotation = Projectile.DirectionTo(playerStart).ToRotation();
			}
			else
			{
				Vector2 mouseOffset = Main.MouseWorld - owner.Center;
				int max = 400 + (int)MathHelper.Max(0, (mouseOffset.Length() - 400) * 0.2f);
				mouseStart = owner.Center + (Vector2.Normalize(mouseOffset) * MathHelper.Min(mouseOffset.Length(), max));
			}

			float progress = EaseFunction.EaseQuadOut.Ease((float)Math.Sin(throwTimer * progressMult));
			float nextProgress = EaseFunction.EaseQuadOut.Ease((float)Math.Sin((throwTimer + 1) * progressMult));

			Vector2 currentPoint = Vector2.Lerp(playerStart, mouseStart, progress);
			Vector2 nextPoint = Vector2.Lerp(playerStart, mouseStart, nextProgress);
			Projectile.velocity = nextPoint - currentPoint;

			if (Math.Cos((throwTimer + 50) * progressMult) > 0 || released)
				throwTimer++;
			else
			{
				Vector2 mouseOffset = Main.MouseWorld - owner.Center;

				int max = 400 + (int)MathHelper.Max(0, (mouseOffset.Length() - 400) * 0.2f);
				Vector2 positionToBe = owner.Center + (Vector2.Normalize(mouseOffset) * MathHelper.Min(mouseOffset.Length(), max));
				Projectile.velocity = Projectile.DirectionTo(positionToBe) * 0.45f * MathHelper.Clamp((float)Math.Pow((Projectile.Center - positionToBe).Length(), 0.6f), 1, 30);
				midRotation = Projectile.velocity.ToRotation();
			}

			Projectile.extraUpdates = 8;
			Projectile.rotation += 0.06f;

			if ((Projectile.Distance(owner.Center) < 20 || Math.Sin(throwTimer * progressMult) < 0) && released)
				Projectile.active = false;

			updatePoints = true;
			canHit = true;
			hide = false;
		}
    }
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
			owner.ChangeDir((Main.MouseWorld.X > owner.Center.X) ? 1 : -1);
			midPoint += midPointDirection;
			midPoint2 += midPointDirection2;
			hitCounter++;
			owner.itemTime = owner.itemAnimation = 2;
			hide = false;
			canHit = false;
			backRotation = owner.DirectionTo(Main.MouseWorld).ToRotation();
			Projectile.velocity = Vector2.Zero;
			lightningOrigin = owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, backRotation - 1.57f);
			owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, backRotation - 1.57f);

			float rotationOffset = Math.Sign(owner.velocity.X) * EaseFunction.EaseQuadIn.Ease(MathHelper.Min(Math.Abs(owner.velocity.X * 0.1f), 0.65f));
			Projectile.rotation = (owner.direction == 1 ? 3.4f : 1.2f) + rotationOffset;
			Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, owner.direction + rotationOffset) + new Vector2(owner.direction * 15, -2 - owner.direction);
			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, owner.direction + rotationOffset);
			owner.heldProj = Projectile.whoAmI;
			squish = 0.7f;
			zapTarget = Main.npc.Where(x => x.active && !x.townNPC && x.Distance(owner.Center) < 300).OrderBy(x => x.Distance(owner.Center)).FirstOrDefault();
			midRotation = 0;
			anchorPoint = Projectile.Center - Main.screenPosition;
			owner.heldProj = Projectile.whoAmI;
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
						trail3Start++;

					if (Main.rand.NextBool())
					{
						if (trail5Start > 0)
							trail5Start--;
					}
					else if (trail5Start < 13)
						trail5Start++;

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
			BezierCurve curve = new BezierCurve(lightningOrigin, midPoint, zapTarget.Center);
			cache = curve.GetPoints(15);

			BezierCurve curve2 = new BezierCurve(cache[trail3Start], midPoint2, zapTarget.Center);
			cache3 = curve2.GetPoints(7);

			BezierCurve curve3 = new BezierCurve(cache[trail5Start], midPoint3, zapTarget.Center);
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
					cache2.Add(point + (dir * Main.rand.NextFloat(5)));
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
					cache4.Add(point + (dir * Main.rand.NextFloat(5)));
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
					cache6.Add(point + (dir * Main.rand.NextFloat(5)));
			}
		}

		private void ManageTrails()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => Main.rand.NextFloat(0.75f, 1.25f) * 16, factor =>
			{
				if (factor.X > 0.99f)
					return Color.Transparent;

				return Color.Purple * 0.2f * EaseFunction.EaseCubicOut.Ease(1 - factor.X);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = zapTarget.Center;
			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 3 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
			{
				float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
				return Color.Lerp(new Color(200, 150, 200), new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), 1 - progress) * progress;
			});

			trail2.Positions = cache2.ToArray();
			trail2.NextPosition = zapTarget.Center;

			trail3 = trail3 ?? new Trail(Main.instance.GraphicsDevice, 7, new TriangularTip(4), factor => Main.rand.NextFloat(0.75f, 1.25f) * 16, factor =>
			{
				if (factor.X > 0.99f)
					return Color.Transparent;

				return Color.Purple * 0.2f * EaseFunction.EaseCubicOut.Ease(1 - factor.X) * ((100 - (originCounter % 50)) / 100f);
			});

			trail3.Positions = cache3.ToArray();
			trail3.NextPosition = zapTarget.Center;
			trail4 = trail4 ?? new Trail(Main.instance.GraphicsDevice, 7, new TriangularTip(4), factor => 3 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
			{
				float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
				return Color.Lerp(new Color(200, 150, 200), new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), 1 - progress) * progress * ((100 - (originCounter % 50)) / 100f);
			});

			trail4.Positions = cache4.ToArray();
			trail4.NextPosition = zapTarget.Center;

			trail5 = trail5 ?? new Trail(Main.instance.GraphicsDevice, 7, new TriangularTip(4), factor => Main.rand.NextFloat(0.75f, 1.25f) * 16, factor =>
			{
				if (factor.X > 0.99f)
					return Color.Transparent;

				return Color.Purple * 0.2f * EaseFunction.EaseCubicOut.Ease(1 - factor.X) * ((100 - (originCounter % 50)) / 100f);
			});

			trail5.Positions = cache5.ToArray();
			trail5.NextPosition = zapTarget.Center;

			trail6 = trail6 ?? new Trail(Main.instance.GraphicsDevice, 7, new TriangularTip(4), factor => 3 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
			{
				float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
				return Color.Lerp(new Color(200, 150, 200), new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), 1 - progress) * progress * ((100 - (originCounter % 50)) / 100f);
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

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            base.OnHitNPC(target, damage, knockback, crit);
			if (rightClicked)
				Core.Systems.CameraSystem.Shake -= 2;
		}

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			if (rightClicked)
				knockback = 0;
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
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

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

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
			return Projectile.Center + (directionTo.RotatedBy(-Math.Sign(directionTo.X) * Main.rand.NextFloat(0.5f, 1f)) * Main.rand.NextFloat(0.5f, 1f) * Projectile.Distance(target.Center));
		}

		private Vector2 CalculateMidpointBranch(NPC target)
		{
			Vector2 directionTo = Projectile.DirectionTo(target.Center);
			return Projectile.Center + (directionTo.RotatedBy(-Math.Sign(directionTo.X) * -Main.rand.NextFloat(-1.57f, 1.57f)) * Main.rand.NextFloat(0.25f, 0.5f) * Projectile.Distance(target.Center));
		}
	}

	public class LightsaberProj_Red : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.DarkRed.ToVector3() * 1.3f;

		bool releasedRight = false;

		int pullTimer = 0;

		NPC pullTarget;

		private bool targetNoGrav = false;

		private Vector2 pullDirection = Vector2.Zero;

		private int pauseTime = 0;

		private Vector2 launchVector = Vector2.Zero;
		protected override void RightClickBehavior()
        {
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			owner.heldProj = Projectile.whoAmI;
			if (!releasedRight && Main.mouseRight)
			{
				Projectile.timeLeft = 30;
				hide = true;
				canHit = false;
				if (pullTimer == 0)
					pullTarget = Main.npc.Where(x => x.active && x.knockBackResist > 0 && !x.boss && !x.townNPC && x.Distance(Main.MouseWorld) < 200).OrderBy(x => x.Distance(Main.MouseWorld)).FirstOrDefault();

				if (pullTarget != default)
				{
					if (pullTimer == 0)
					{
						targetNoGrav = pullTarget.noGravity;
						pullTarget.noGravity = true;
					}
					pullDirection = owner.DirectionTo(pullTarget.Center);
					pullTarget.velocity = -pullDirection * EaseFunction.EaseQuinticIn.Ease(MathHelper.Clamp(pullTimer / 150f, 0, 1)) * 12;
					Projectile.rotation = pullDirection.ToRotation();

					if (pullTarget.Distance(owner.Center) < 5)
						releasedRight = true;

					Vector2 dustVel = pullDirection.RotatedByRandom(0.8f) * Main.rand.NextFloat();
					Dust.NewDustPerfect(pullTarget.Center - (dustVel * 45), ModContent.DustType<Dusts.Glow>(), dustVel * 3, 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(0.25f, 0.45f));
				}
				else
					Projectile.rotation = owner.DirectionTo(Main.MouseWorld).ToRotation();
				pullTimer++;
			}
			else
			{
				if (pullTarget != default)
                {
					pullTarget.noGravity = targetNoGrav;
                }

				if (!releasedRight)
				{
					float rot = Projectile.rotation;
					if (owner.direction == 1)
						facingRight = true;
					else
						facingRight = false;

					midRotation = rot;
					canHit = true;
					releasedRight = true;
					hide = false;

					anchorPoint = Vector2.Zero;
					endRotation = rot - (2f * owner.direction);

					oldRotation = new List<float>();
					oldPositionDrawing = new List<Vector2>();
					oldSquish = new List<float>();
					oldPositionCollision = new List<Vector2>();

					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, owner.Center);

					startRotation = endRotation;
					startSquish = endSquish;
					endMidRotation = rot + Main.rand.NextFloat(-0.45f, 0.45f);
					startMidRotation = midRotation;
					endSquish = 0.3f;
					endRotation = rot + (3f * owner.direction);
					attackDuration = 65;
					//Projectile.ai[0] += 30f / attackDuration;
				}

				if (Projectile.ai[0] < 1)
				{
					Projectile.timeLeft = 50;
					if (pauseTime-- <= 0)
						Projectile.ai[0] += 1f / attackDuration;
					rotVel = Math.Abs(EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]) - EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0] - (1f / attackDuration))) * 2;
				}
				else
					rotVel = 0f;

				float progress = EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]);

				/*if (Main.netMode != NetmodeID.Server)
				{
					if (trailCounter % 5 == 0 || (progress > 0.1f && progress < 0.9f))
					{
						ManageCaches();
						ManageTrail();
					}
				}*/

				Projectile.scale = MathHelper.Min(MathHelper.Min(growCounter++ / 30f, 1 + (rotVel * 4)), 1.3f);

				Projectile.rotation = MathHelper.Lerp(startRotation, endRotation, progress);
				midRotation = MathHelper.Lerp(startMidRotation, endMidRotation, progress);
				squish = MathHelper.Lerp(startSquish, endSquish, progress) + (0.35f * (float)Math.Sin(3.14f * progress));
				anchorPoint = Projectile.Center - Main.screenPosition;

				owner.ChangeDir(facingRight ? 1 : -1);

				owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
				owner.itemAnimation = owner.itemTime = 5;

				if (owner.direction != 1)
					Projectile.rotation += 0.78f;

				updatePoints = pauseTime <= 0;

				if (pullTarget != null && pullTarget.active)
				{
					if (pauseTime > 0)
					{
						pullTarget.velocity = Vector2.Zero;
					}
					else if (pauseTime == 0)
                    {
						pullTarget.velocity = launchVector * 8 * pullTarget.knockBackResist;
                    }
				}
			}
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (target == pullTarget)
            {
				Core.Systems.CameraSystem.Shake += 5;
				launchVector = pullTarget.DirectionTo(Main.MouseWorld);
				damage = (int)(damage * 2.5f);
				target.velocity = Vector2.Zero;
				pauseTime = 40;
            }
        }
    }

	public class LightsaberProj_White : LightsaberProj
	{
		protected override Vector3 BladeColor => new Color(200, 200, 255).ToVector3();

		private bool spawnedSecond = false;

        protected override void RightClickBehavior()
        {
			owner.GetModPlayer<LightsaberPlayer>().whiteCooldown = 1200;
			if (nonEasedProgress > 0.5f && !spawnedSecond)
			{
				Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), owner.Center, Vector2.Zero, ModContent.ProjectileType<LightsaberProj_White>(), Projectile.damage, Projectile.knockBack, owner.whoAmI);
				(proj.ModProjectile as LightsaberProj_White).frontHand = false;
				(proj.ModProjectile as LightsaberProj_White).spawnedSecond = true;
				(proj.ModProjectile as LightsaberProj_White).rightClicked = true;
				spawnedSecond = true;
			}
			hide = false;
			canHit = true;
			if (thrown)
				ThrownBehavior();
			else
				HeldBehavior();

			if (Projectile.ai[0] >= 1 && Main.mouseRight)
				Projectile.ai[0] = 0;
		}
    }

	public class LightsaberProj_Yellow : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Yellow.ToVector3() * 0.8f * fade;

		private bool dashing = false;

		private bool caughtUp = false;

		private float fade = 1f;

        protected override void RightClickBehavior()
        {
			hide = true;
			canHit = false;
			Projectile.active = false;
        }

        protected override void SafeLeftClickBehavior()
        {
			if (!thrown)
				return;

			if (Main.mouseRight && !dashing)
			{
				dashing = true;
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<LightsaberProj_YellowDash>(), Projectile.damage * 2, 0, owner.whoAmI);
				owner.GetModPlayer<LightsaberPlayer>().dashing = true;
			}

			if (dashing)
            {
				Projectile.velocity = Vector2.Zero;
				if (owner.Distance(Projectile.Center) < 80 || !owner.GetModPlayer<LightsaberPlayer>().dashing && !caughtUp)
                {
					owner.Center = Projectile.Center;
					owner.velocity = Vector2.Zero;
					owner.GetModPlayer<LightsaberPlayer>().dashing = false;
					Projectile.active = true;
					caughtUp = true;
				}

				if (caughtUp)
				{
					Projectile.active = true;
					fade -= 0.01f;
					if (fade <= 0)
						Projectile.active = false;
				}
				else
				{
					owner.velocity = owner.DirectionTo(Projectile.Center) * 60;
					Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(45, 45) + owner.velocity, ModContent.DustType<Dusts.GlowLine>(), owner.DirectionTo(Projectile.Center) * Main.rand.NextFloat(2), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(1f, 1.5f));
					dust.fadeIn = 20;

					Dust.NewDustPerfect(owner.Center + owner.velocity + Main.rand.NextVector2Circular(30, 30), ModContent.DustType<LightsaberGlow>(), Vector2.Normalize(owner.velocity).RotatedBy(Main.rand.NextFloat(2.5f, 3f)) * Main.rand.NextFloat(3), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(0.5f, 0.85f));
					Dust.NewDustPerfect(owner.Center + owner.velocity + Main.rand.NextVector2Circular(30, 30), ModContent.DustType<LightsaberGlow>(), Vector2.Normalize(owner.velocity).RotatedBy(-Main.rand.NextFloat(2.5f, 3f)) * Main.rand.NextFloat(3), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(0.5f, 0.85f));
				}
			}
        }
    }

	public class LightsaberProj_YellowDash : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults() => DisplayName.SetDefault("Lightsaber");

		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 120;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.Center = owner.Center;

			if (!owner.GetModPlayer<LightsaberPlayer>().dashing)
				Projectile.active = false;
		}
	}

	class Lightsaber_BlueLensFlare : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.Keys + "Glow";

		public override void SetStaticDefaults() => DisplayName.SetDefault("Laser");

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

	class Lightsaber_BlueLaser : ModProjectile, IDrawAdditive
    {
		public override string Texture => AssetDirectory.VitricBoss + "RoarLine";

		public override void SetStaticDefaults() => DisplayName.SetDefault("Laser");

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
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Lightsaber_BlueLensFlare>(), 0, 0, Projectile.owner);
            }
        }

        public override void Kill(int timeLeft)
        {

        }

		public void DrawAdditive(SpriteBatch spriteBatch)
        {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value; spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), Projectile.rotation, tex.Size() / 2, Projectile.scale * new Vector2(1, 0.6f) * 1.5f, SpriteEffects.None, 0f);
			for (int i = 0; i < 5; i++)
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), Projectile.rotation, tex.Size() / 2, Projectile.scale * new Vector2(1, 0.9f), SpriteEffects.None, 0f);
			for (int k = 0; k < 9; k++)
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale * 0.9f * new Vector2(1, 0.9f), SpriteEffects.None, 0f);

			for (int l = 0; l < 2; l++)
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), Projectile.rotation, tex.Size() / 2, Projectile.scale * 2f * new Vector2(1, 0.9f), SpriteEffects.None, 0f);
		}
    }

    class Lightsaber_GreenShockwave : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults() => DisplayName.SetDefault("Shockwave");
		private int TileType => (int)Projectile.ai[0];
		private int ShockwavesLeft => (int)Projectile.ai[1];//Positive and Negitive

		private bool createdLight = false;

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 1060;
			Projectile.tileCollide = true;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.idStaticNPCHitCooldown = 20;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.extraUpdates = 5;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			if (Projectile.timeLeft > 1000)
			{
				if (Projectile.timeLeft < 1002 && Projectile.timeLeft > 80)
					Projectile.Kill();

				Projectile.velocity.Y = 4f;
			}
			else
			{
				Projectile.velocity.Y = Projectile.timeLeft <= 10 ? 1f : -1f;

				if (Projectile.timeLeft == 19 && Math.Abs(ShockwavesLeft) > 0)
				{
					Projectile proj = Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), new Vector2((int)Projectile.Center.X / 16 * 16 + 16 * Math.Sign(ShockwavesLeft)
					, (int)Projectile.Center.Y / 16 * 16 - 32),
					Vector2.Zero, Projectile.type, Projectile.damage, 0, Main.myPlayer, TileType, Projectile.ai[1] - Math.Sign(ShockwavesLeft));
					proj.extraUpdates = (int)(Math.Abs(ShockwavesLeft) / 3f);
				}

			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft < 21)
				Main.spriteBatch.Draw(TextureAssets.Tile[TileType].Value, Projectile.position - Main.screenPosition, new Rectangle(18, 0, 16, 16), lightColor);

			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.timeLeft > 800)
			{
				Point16 point = new Point16((int)((Projectile.Center.X + Projectile.width / 3f * Projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, (int)(Projectile.Center.Y / 16) + 1));
				Tile tile = Framing.GetTileSafely(point.X, point.Y);

				if (!createdLight)
                {
					createdLight = true;
					Dust.NewDustPerfect(point.ToVector2() * 16, ModContent.DustType<LightsaberLight>(), Vector2.Zero, 0, Color.Green, 1);
                }
				if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.HasTile && Main.tileSolid[tile.TileType])
				{
					Projectile.timeLeft = 20;
					Projectile.ai[0] = tile.TileType;
					Projectile.tileCollide = false;
					Projectile.position.Y += 16;

					for (float num315 = 0.50f; num315 < 3; num315 += 0.25f)
					{
						float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
						Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 2f;
						int dustID = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, (int)(Projectile.height / 2f), ModContent.DustType<LightsaberGlow>(), 0f, 0f, 50, Color.Green, Main.rand.NextFloat(0.45f,0.95f));
						Main.dust[dustID].velocity = vecangle;
					}
				}
			}
			return false;
		}
	}

	public class LightsaberGProj : GlobalProjectile
    {
		public override bool InstancePerEntity => true;

        public Entity parent = default;
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

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
			if (dashing)
				return false;
            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }

        public override void PreUpdate()
        {
            if (dashing || jumping)
				Player.maxFallSpeed = 2000f;

			if (whiteCooldown == 0)
            {
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item9 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Player.Center);
				Dust dust = Dust.NewDustPerfect(Player.Center, ModContent.DustType<LightsaberStar>(), Vector2.Zero, 0, new Color(200, 200, 255, 0), 0.3f);
				dust.customData = Player.whoAmI;
			}
		}

        public override void PostUpdate()
        {
			if (jumping)
            {
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
				jumpVelocity = Player.velocity;
        }
    }
}