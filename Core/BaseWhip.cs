using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.GameContent;

namespace StarlightRiver.Core
{
	public abstract class BaseWhip : ModProjectile
	{
		protected string _name;
		protected int _segments;
		protected float _rangeMultiplier;
		protected float _flyTime;
		protected Color _stringColor;
		protected int _handleOffset;

		public float MiddleOfArc => _flyTime / 1.5f;

		public Vector2 EndPoint => Projectile.WhipPointsForCollision[_segments - 1] + new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;

		public BaseWhip(string name, int segments = 20, float rangeMultiplier = 1f, Color? stringColor = null, int handleOffset = 2)
		{
			_name = name;
			_segments = Math.Max(3, segments);
			_rangeMultiplier = rangeMultiplier;

			if (stringColor != null)
				_stringColor = stringColor.Value;
			else
				_stringColor = Color.White;

			_handleOffset = handleOffset;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(_name);
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.DefaultToWhip();
		}

        public override bool PreAI()
		{
			Player player = Main.player[Projectile.owner];
			_flyTime = player.itemAnimationMax * Projectile.MaxUpdates;

			Projectile.ai[0]++;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[0] - 1f);
			Projectile.spriteDirection = ((!(Vector2.Dot(Projectile.velocity, Vector2.UnitX) < 0f)) ? 1 : -1);

			if (Projectile.ai[0] >= _flyTime || player.itemAnimation == 0)
			{
				Projectile.Kill();
				return false;
			}

			player.heldProj = Projectile.whoAmI;
			player.itemAnimation = player.itemAnimationMax - (int)(Projectile.ai[0] / Projectile.MaxUpdates);
			player.itemTime = player.itemAnimation;

			if (Projectile.ai[0] == (int)(_flyTime / 2f))
			{
				Projectile.WhipPointsForCollision.Clear();
				SetPoints(Projectile.WhipPointsForCollision);
				Vector2 position = Projectile.WhipPointsForCollision[Projectile.WhipPointsForCollision.Count - 1];
				SoundEngine.PlaySound(SoundID.Item153, position);
			}

			if (Utils.GetLerpValue(0.1f, 0.7f, Projectile.ai[0] / _flyTime, true) * Utils.GetLerpValue(0.9f, 0.7f, Projectile.ai[0] / _flyTime, true) > 0.5f)
			{
				Projectile.WhipPointsForCollision.Clear();
				SetPoints(Projectile.WhipPointsForCollision);
			}

			ArcAI();

			return false;
		}
		
		public virtual void ArcAI() { }

		public override void CutTiles()
		{
			Vector2 midPoint = new Vector2(Projectile.width * Projectile.scale * 0.5f, 0f);

			for (int i = 0; i < Projectile.WhipPointsForCollision.Count; i++)
			{
				DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
				Utils.PlotTileLine(Projectile.WhipPointsForCollision[i] - midPoint, Projectile.WhipPointsForCollision[i] + midPoint, Projectile.height * Projectile.scale, DelegateMethods.CutTiles);
			}
		}

		public virtual void SetPoints(List<Vector2> controlPoints)
		{
			float time = Projectile.ai[0] / _flyTime;
			float timeModified = time * 1.5f;
			float segmentOffset = MathHelper.Pi * 10f * (1f - timeModified) * (-Projectile.spriteDirection) / _segments;
			float tLerp = 0f;

			if (timeModified > 1f)
			{
				tLerp = (timeModified - 1f) / 0.5f;
				timeModified = MathHelper.Lerp(1f, 0f, tLerp);
			}

			//vanilla code
			float realRange = Projectile.ai[0] - 1f;
			Player player = Main.player[Projectile.owner];
			Item heldItem = player.HeldItem;
			realRange = (ContentSamples.ItemsByType[heldItem.type].useAnimation * 2) * time * player.whipRangeMultiplier;
			float segmentLength = Projectile.velocity.Length() * realRange * timeModified * _rangeMultiplier / (float)_segments;
			Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);
			Vector2 firstPos = playerArmPosition;
			float negativeAngle = -MathHelper.PiOver2;
			Vector2 midPos = firstPos;
			float directedAngle = 0f + MathHelper.PiOver2 + (MathHelper.PiOver2 * Projectile.spriteDirection);
			Vector2 lastPos = firstPos;
			float positiveAngle = MathHelper.PiOver2;
			controlPoints.Add(playerArmPosition);

			for (int i = 0; i < _segments; i++)
			{
				float thisOffset = segmentOffset * ((float)i / (float)_segments);
				Vector2 nextFirst = firstPos + negativeAngle.ToRotationVector2() * segmentLength;
				Vector2 nextLast = lastPos + positiveAngle.ToRotationVector2() * (segmentLength * 2f);
				Vector2 nextMid = midPos + directedAngle.ToRotationVector2() * (segmentLength * 2f);
				float progressModifier = 1f - (float)Math.Pow((1f - timeModified), 2);
				Vector2 lerpPoint1 = Vector2.Lerp(nextLast, nextFirst, (progressModifier * 0.9f) + 0.1f);
				Vector2 lerpPoint2 = Vector2.Lerp(nextMid, lerpPoint1, (progressModifier * 0.7f) + 0.3f);
				Vector2 spinningpoint = playerArmPosition + (lerpPoint2 - playerArmPosition) * new Vector2(1f, 1.5f);
				Vector2 item = spinningpoint.RotatedBy(Projectile.rotation + 4.712389f * (float)(Math.Pow(tLerp, 2)) * Projectile.spriteDirection, playerArmPosition);
				controlPoints.Add(item);
				negativeAngle += thisOffset;
				positiveAngle += thisOffset;
				directedAngle += thisOffset;
				firstPos = nextFirst;
				lastPos = nextLast;
				midPos = nextMid;
			}
		}

		public virtual void DrawBehindWhip(ref Color lightColor) { }

		public override bool PreDraw(ref Color lightColor)
        {
			DrawBehindWhip(ref lightColor);
			List<Vector2> points = new List<Vector2>();
			points.Clear();
			SetPoints(points);

			//string
			Vector2 stringPoint = points[0];
			for (int i = 0; i < points.Count - 2; i++)
			{
				Vector2 nextPoint = points[i + 1] - points[i];
				Color color = _stringColor.MultiplyRGBA(Projectile.GetAlpha(Lighting.GetColor(points[i].ToTileCoordinates())));
				Vector2 scale = new Vector2(1f, (nextPoint.Length() + 2f) / (float)TextureAssets.FishingLine.Height());
				Main.EntitySpriteDraw(TextureAssets.FishingLine.Value, stringPoint - Main.screenPosition, null, color, nextPoint.ToRotation() - MathHelper.PiOver2, new Vector2(TextureAssets.FishingLine.Width() * 0.5f, 2f), scale, SpriteEffects.None, 0);
				stringPoint += nextPoint;
			}

			//whip
			Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
			Rectangle whipFrame = texture.Frame(1, 5, 0, 0);
			int height = whipFrame.Height;
			Vector2 firstPoint = points[0];

			for (int i = 0; i < points.Count - 1; i++)
            {
				Vector2 origin = whipFrame.Size() * 0.5f;
				bool draw = true;

				if (i == 0)
					origin.Y += _handleOffset;
				else if (i == points.Count - 2)
					whipFrame.Y = height * 4;
				else
				{
					whipFrame.Y = height * SegmentVariant(i);
					draw = ShouldDrawSegment(i);
				}

				Vector2 difference = points[i + 1] - points[i];

				if (draw)
                {
					Color alpha = Projectile.GetAlpha(Lighting.GetColor(points[i].ToTileCoordinates()));
					float rotation = difference.ToRotation() - MathHelper.PiOver2;
					Main.EntitySpriteDraw(texture.Value, points[i] - Main.screenPosition, whipFrame, alpha, rotation, origin, Projectile.scale, SpriteEffects.None, 0);
                }

				firstPoint += difference;
			}

			return false;
        }
		public virtual int SegmentVariant(int segment) => (1 + (segment % 3));

		public virtual bool ShouldDrawSegment(int segment) => segment % 2 == 0;
    }
}
