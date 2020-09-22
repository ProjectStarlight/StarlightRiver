using Microsoft.Xna.Framework;
using StarlightRiver.Tiles.Permafrost;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System;
using System.Linq;

namespace StarlightRiver
{
    public partial class StarlightWorld : ModWorld
    {
        private void PermafrostGen(GenerationProgress progress)
        {
            progress.Message = "Permafrost generation";

            int iceLeft = 0;
            int iceRight = 0;
            int iceBottom = 0;

            List<Circle> circles = new List<Circle>();

            for (int x = 0; x < Main.maxTilesX; x++) //Find the ice biome since vanilla cant fucking keep track of its own shit
            {
                if (iceLeft != 0) break;

                for (int y = 0; y < Main.maxTilesY; y++)
                    if (Main.tile[x, y].type == TileID.IceBlock)
                    {
                        iceLeft = x;
                        break;
                    }
            }

            for (int x = Main.maxTilesX - 1; x > 0; x--)
            {
                if (iceRight != 0) break;

                for (int y = 0; y < Main.maxTilesY; y++)
                    if (Main.tile[x, y].type == TileID.IceBlock)
                    {
                        iceRight = x;
                        break;
                    }
            }

            for (int y = Main.maxTilesY - 1; y > 0; y--)
                if (Main.tile[iceLeft, y].type == TileID.IceBlock)
                {
                    iceBottom = y;
                    break;
                }

            int center = iceLeft + (iceRight - iceLeft) / 2;

            Circle bigCircle = new Circle(new Point16(center + 15, iceBottom - 70), 110);
            circles.Add(bigCircle);
            DigCircle(bigCircle, 30, 2400);

            for (int k = 0; k < 150; k++)
            {
                Point16 pos = new Point16(WorldGen.genRand.Next(center - 400, center + 400), WorldGen.genRand.Next(iceBottom - 150, iceBottom + 50));
                int dist = (int)((800 - Vector2.Distance(pos.ToVector2(), bigCircle.position.ToVector2())) / 800f * 30);
                int rad = WorldGen.genRand.Next(3 + dist, 9 + dist);
                Circle circle = new Circle(pos, rad);

                if (!circles.Any(n => n.Colliding(circle)))
                {
                    DigCircle(circle);
                    circles.Add(circle);
                }
            }

            var caves = GenerateLines(circles);
            caves.ForEach(n => DigTunnel(n));

            SquidBossArena = new Rectangle(center - 40, iceBottom - 150, 109, 180);
            StructureHelper.StructureHelper.GenerateStructure("Structures/SquidBossArena", new Point16(center - 40, iceBottom - 150), mod);
        }

        private void DigCircle(Circle circle, int maxOff = 0, int speed = 1800)
        {
            Point16 pos = circle.position;
            int rad = circle.radius + 7;
            int row = WorldGen.genRand.Next(1000);

            for (int x = -rad; x < rad; x++)
                for (int y = -rad; y < rad; y++)
                {
                    float angleOff = new Vector2(x, y).ToRotation();
                    float off = genNoise.GetPerlin(angleOff / 6.28f * speed, row) * (maxOff == 0 ? rad / 30f * 5 : maxOff);
                    float distSquared = Vector2.DistanceSquared(pos.ToVector2() + new Vector2(x, y), pos.ToVector2());

                    if (distSquared <= (int)Math.Pow(rad - Math.Abs(off), 2))
                    {
                        if (distSquared <= (int)Math.Pow(rad - Math.Abs(off) - 7, 2))
                            Framing.GetTileSafely(pos.X + x, pos.Y + y).ClearEverything();
                        else
                            WorldGen.PlaceTile(pos.X + x, pos.Y + y, TileType<PermafrostIce>(), true, true);
                    }
                }
        }

        private List<Vector4> GenerateLines(List<Circle> circles)
        {
            List<Vector4> output = new List<Vector4>();

            for (int k = 0; k < circles.Count; k++)
            {
                for (int j = k + 1; j < circles.Count; j++)
                {
                    var line = new Vector4(circles[k].position.X, circles[k].position.Y, circles[j].position.X, circles[j].position.Y);
                    var lineVector = new Vector2(line.X - line.Z, line.Y - line.W);
                    var normal = lineVector.RotatedBy(Math.PI / 2);

                    var colliding = false;

                    for (int n = 0; n < circles.Count; n++)
                    {
                        Circle check = circles[n];
                        if (n == k || n == j) continue;

                        var off = check.position.ToVector2() - line.XY();

                        var dot = Math.Abs(Vector2.Dot(off, normal));
                        var angle = Math.Acos(dot / (off.Length() * normal.Length()));

                        var perp = Math.Cos(angle) * off.Length();

                        if (Math.Abs(perp) < check.radius) //we've passed the check against the infinite line
                        {
                            var distAlong = off.Length() * Math.Sin(angle);
                            //var closestPoint = Vector2.Lerp(line.XY(), line.ZW(), distAlong / lineVector.Length()); -- this would find the exact point, which is useless to us. We jsut need to know if its on, so if the fractino is between 0 and 1
                            if (distAlong > 0 || distAlong < 1)
                            {
                                colliding = true;
                                break;
                            }
                        }
                    }

                    if (!colliding)
                        output.Add(line);
                }
            }

            return output;
        }

        private void DigTunnel(Vector4 line, int type = TileID.EmeraldGemspark)
        {
            var length = (line.XY() - line.ZW()).Length();
            for (float k = 0; k < 1; k += 1 / length)
            {
                Point16 pos = Vector2.Lerp(line.XY(), line.ZW(), k).ToPoint16();
                WorldGen.PlaceTile(pos.X, pos.Y, type, false, true);
            }
        }
    }

    public struct Circle
    {
        public Point16 position;
        public int radius;

        public Circle(Point16 position, int radius)
        {
            this.position = position;
            this.radius = radius;
        }

        public bool Colliding(Circle compare)
        {
            if (Vector2.DistanceSquared(position.ToVector2(), compare.position.ToVector2()) <= (float)Math.Pow(radius + compare.radius + 14, 2))
                return true;
            else return false;
        }
    }
}
