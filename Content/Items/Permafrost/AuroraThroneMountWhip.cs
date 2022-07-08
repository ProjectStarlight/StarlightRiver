using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Permafrost
{
    public class AuroraThroneMountWhip : BaseWhip
    {
        public override string Texture => AssetDirectory.PermafrostItem + Name;

        public AuroraThroneMountWhip() : base("Tentacle", 15, 0.57f, new Color(153, 255, 255)) { }

        public override int SegmentVariant(int segment)
        {
            int variant;
            switch (segment)
            {
                default:
                    variant = 1;
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    variant = 2;
                    break;
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                    variant = 3;
                    break;
            }
            return variant;
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];

            if (_flyTime == 0)
            {
                _flyTime = Projectile.ai[0];
                Projectile.ai[0] = 0;
            }

            Projectile.ai[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[0] - 1f);
            Projectile.spriteDirection = ((!(Vector2.Dot(Projectile.velocity, Vector2.UnitX) < 0f)) ? 1 : -1);

            if (Projectile.ai[0] >= _flyTime)
            {
                Projectile.Kill();
                return false;
            }

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

		public override void SetPoints(List<Vector2> controlPoints)
		{
            float time = Projectile.ai[0] / _flyTime;

            if (Projectile.ai[1] == -1)
                time = 1 - time;

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
            realRange = (50) * time * player.whipRangeMultiplier;
            float segmentLength = Projectile.velocity.Length() * realRange * timeModified * _rangeMultiplier / (float)_segments;
            Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile) + new Vector2(0, 12);
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
                Vector2 lerpPoint1 = Vector2.Lerp(nextLast, nextFirst, (progressModifier * 0.7f) + 0.3f);
                Vector2 lerpPoint2 = Vector2.Lerp(nextMid, lerpPoint1, (progressModifier * 0.9f) + 0.1f);
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

		public override bool ShouldDrawSegment(int segment) => true;// segment % 2 == 0;


        public override Color? GetAlpha(Color lightColor)
        {
            Color minLight = lightColor;
            Color minColor = new Color(10, 25, 33);

            if (minLight.R < minColor.R) minLight.R = minColor.R;
            if (minLight.G < minColor.G) minLight.G = minColor.G;
            if (minLight.B < minColor.B) minLight.B = minColor.B;

            return minLight;
        }
    }
}
