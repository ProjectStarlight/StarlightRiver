using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Permafrost
{
	public class AuroraThroneMountWhip : BaseWhip
	{
		public override string Texture => AssetDirectory.PermafrostItem + Name;

		public AuroraThroneMountWhip() : base("Tentacle", 15, 0.57f, new Color(153, 255, 255)) { }

		public override void Load()
		{
			On.Terraria.Projectile.Colliding += SpecialWhipColission; //We need to use a custom hook here since vanilla oofs whip colission before tmods colliding hook can run.
		}

		public override void SetDefaults()
		{
			Projectile.DefaultToWhip();
			Projectile.ownerHitCheck = false;
		}

		public override int SegmentVariant(int segment)
		{
			int variant = segment switch
			{
				5 or 6 or 7 or 8 => 2,
				9 or 10 or 11 or 12 or 13 => 3,
				_ => 1,
			};
			return variant;
		}

		private bool SpecialWhipColission(On.Terraria.Projectile.orig_Colliding orig, Projectile self, Rectangle myRect, Rectangle targetRect)
		{
			if (self.type == ModContent.ProjectileType<AuroraThroneMountWhip>())
			{
				self.WhipPointsForCollision.Clear();
				(self.ModProjectile as AuroraThroneMountWhip).SetPoints(self.WhipPointsForCollision);

				List<Vector2> points = self.WhipPointsForCollision;

				for (int i = 0; i < points.Count - 1; i++)
				{
					float collisionPoint = 0f;
					if (Collision.CheckAABBvLineCollision(targetRect.TopLeft(), targetRect.Size(), points[i], points[i + 1], 8, ref collisionPoint))
						return true;
				}

				return false;
			}

			return orig(self, myRect, targetRect);
		}

		public override bool PreAI()
		{
			if (flyTime == 0)
			{
				flyTime = Projectile.ai[0];
				Projectile.ai[0] = 0;
			}

			Projectile.ai[0]++;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[0] - 1f);
			Projectile.spriteDirection = (!(Vector2.Dot(Projectile.velocity, Vector2.UnitX) < 0f)) ? 1 : -1;

			if (Projectile.ai[0] >= flyTime)
			{
				Projectile.Kill();
				return false;
			}

			if (Projectile.ai[0] == (int)(flyTime / 2f))
			{
				Vector2 position = Projectile.WhipPointsForCollision[^1];
				SoundEngine.PlaySound(SoundID.Item153, position);
			}

			ArcAI();

			return false;
		}

		public override void SetPoints(List<Vector2> controlPoints)
		{
			float time = Projectile.ai[0] / flyTime;

			if (Projectile.ai[1] == -1)
				time = 1 - time;

			float timeModified = time * 1.5f;
			float segmentOffset = MathHelper.Pi * 10f * (1f - timeModified) * -Projectile.spriteDirection / segments;
			float tLerp = 0f;

			if (timeModified > 1f)
			{
				tLerp = (timeModified - 1f) / 0.5f;
				timeModified = MathHelper.Lerp(1f, 0f, tLerp);
			}

			//vanilla code
			Player player = Main.player[Projectile.owner];
			float realRange = 50 * time * player.whipRangeMultiplier;
			float segmentLength = Projectile.velocity.Length() * realRange * timeModified * rangeMultiplier / segments;
			Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile) + new Vector2(0, 12);
			Vector2 firstPos = playerArmPosition;
			float negativeAngle = -MathHelper.PiOver2;
			Vector2 midPos = firstPos;
			float directedAngle = 0f + MathHelper.PiOver2 + MathHelper.PiOver2 * Projectile.spriteDirection;
			Vector2 lastPos = firstPos;
			float positiveAngle = MathHelper.PiOver2;
			controlPoints.Add(playerArmPosition);

			for (int i = 0; i < segments; i++)
			{
				float thisOffset = segmentOffset * (i / (float)segments);
				Vector2 nextFirst = firstPos + negativeAngle.ToRotationVector2() * segmentLength;
				Vector2 nextLast = lastPos + positiveAngle.ToRotationVector2() * (segmentLength * 2f);
				Vector2 nextMid = midPos + directedAngle.ToRotationVector2() * (segmentLength * 2f);
				float progressModifier = 1f - (float)Math.Pow(1f - timeModified, 2);
				var lerpPoint1 = Vector2.Lerp(nextLast, nextFirst, progressModifier * 0.7f + 0.3f);
				var lerpPoint2 = Vector2.Lerp(nextMid, lerpPoint1, progressModifier * 0.9f + 0.1f);
				Vector2 spinningpoint = playerArmPosition + (lerpPoint2 - playerArmPosition) * new Vector2(1f, 1.5f);
				Vector2 item = spinningpoint.RotatedBy(Projectile.rotation + 4.712389f * (float)Math.Pow(tLerp, 2) * Projectile.spriteDirection, playerArmPosition);
				controlPoints.Add(item);
				negativeAngle += thisOffset;
				positiveAngle += thisOffset;
				directedAngle += thisOffset;
				firstPos = nextFirst;
				lastPos = nextLast;
				midPos = nextMid;
			}
		}

		public override bool ShouldDrawSegment(int segment)
		{
			return true;// segment % 2 == 0;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			Color minLight = lightColor;
			var minColor = new Color(10, 25, 33);

			if (minLight.R < minColor.R)
				minLight.R = minColor.R;
			if (minLight.G < minColor.G)
				minLight.G = minColor.G;
			if (minLight.B < minColor.B)
				minLight.B = minColor.B;

			return minLight;
		}
	}
}
