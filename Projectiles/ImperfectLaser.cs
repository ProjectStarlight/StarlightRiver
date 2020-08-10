using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles
{
    public class ImperfectLaser : ModProjectile
    {
        public const short LaserFocusDist = 80;

        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = true;
            projectile.magic = true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            projectile.position.X = player.Center.X + (40 * player.direction); // 40 should be replaced with the width of the weapon texture
            projectile.timeLeft = 2;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Main.player[projectile.owner];
            Vector2 laserPos2 = new Vector2(projectile.position.X + player.direction * LaserFocusDist, projectile.position.Y);

            return TriangleCollision(player.direction * LaserFocusDist, (float)(Math.PI / 6), projectile.position, targetHitbox, projectile.rotation)
                | TriangleCollision(player.direction * -720, (float)(Math.PI / 6), laserPos2, targetHitbox, projectile.rotation);
        }

        // using a short here because it is more than enough for what we're doing, but is smaller than an int
        // theta would be the angle adjacent to the focal point of the laser, in radians.
        // pos is the upper-leftmost position of the triangle hitbox
        // making w negative will make the hitbox point left, and make pos the upper-rightmost position

        public static bool? TriangleCollision(int w, float theta, Vector2 pos, Rectangle hitbox, float rotation)
        {
            float h = (float)(Math.Abs(w) * Math.Tan(theta / 2));
            Vector2 vertex1 = RotatePoint(new Vector2(pos.X + w, pos.Y + h), pos, rotation);
            Vector2 vertex2 = RotatePoint(new Vector2(pos.X, pos.Y + 2 * h), pos, rotation);

            Vector2 centroid = GetCentroid(pos, vertex1, vertex2);

            Vector2 hitboxCenter = new Vector2(hitbox.Y + hitbox.Height / 2, hitbox.X + hitbox.Width / 2);
            Vector2 intersectionPoint = GetIntersectionPoint(centroid, hitboxCenter, pos, vertex1);

            float angleToHitbox = (float)Math.Atan2(hitbox.X - hitboxCenter.X, hitbox.Y - hitboxCenter.Y);

            float inHitbox = Math.Min((float)((hitbox.Width / 2) / Math.Sin(angleToHitbox)), (float)((hitbox.Height / 2) / Math.Cos(angleToHitbox)));

            return GetDistance(centroid, intersectionPoint) <= GetDistance(centroid, hitboxCenter) - inHitbox ? true : false;
        }

        public static Vector2 GetIntersectionPoint(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            float A1 = p2.Y - p1.Y;
            float B1 = p2.X - p1.X;
            float C1 = A1 * p1.X + B1 * p1.Y;

            float A2 = q2.Y - q1.Y;
            float B2 = q2.X - q1.X;
            float C2 = A2 * q1.X + B2 * q1.Y;

            float delta = A1 * B2 - A2 * B1;

            float x = (B2 * C1 - B1 * C2) / delta;
            float y = (A1 * C2 - A2 * C1) / delta;
            return new Vector2(x, y);
        }

        public static Vector2 GetCentroid(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return new Vector2((p1.X + p2.X + p3.X) / 3f, (p1.Y + p2.Y + p3.Y) / 3f);
        }

        public static float GetDistance(Vector2 p1, Vector2 p2)
        {
            return (float)(Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2)));
        }

        public static Vector2 RotatePoint(Vector2 p1, Vector2 p2, float theta)
        {
            float s = (float)Math.Sin(theta);
            float c = (float)Math.Cos(theta);

            p1.X -= p2.X;
            p1.Y -= p2.Y;

            float xn = p1.X * c - p1.Y * s; // goes counterclockwise
            float yn = p1.X * s + p1.Y * c;

            return new Vector2(xn + p2.X, yn + p2.Y);
        }
    }
}