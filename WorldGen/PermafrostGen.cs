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
            //DigCircle(bigCircle, 30, 2400);

            for (int k = 0; k < 150; k++)
            {
                Point16 pos = new Point16(WorldGen.genRand.Next(center - 400, center + 400), WorldGen.genRand.Next(iceBottom - 150, iceBottom + 50));
                int dist = (int)((800 - Vector2.Distance(pos.ToVector2(), bigCircle.position.ToVector2())) / 800f * 30);
                int rad = WorldGen.genRand.Next(3 + dist, 9 + dist);
                Circle circle = new Circle(pos, rad);

                if (!circles.Any(n => n.Colliding(circle)))
                {
                    circles.Add(circle);
                }
            }

            int row = WorldGen.genRand.Next(1000);

            var caves = GenerateLines(circles);
            caves.ForEach(n => DigTunnel(n, row));

            circles.ForEach(n => DigCircle(n));

            caves.ForEach(n => DigTunnel(n, row, true));
            caves.ForEach(n => DecorateTunnel(n));

            SquidBossArena = new Rectangle(center - 40, iceBottom - 150, 109, 180);
            StructureHelper.StructureHelper.GenerateStructure("Structures/SquidBossArena", new Point16(center - 40, iceBottom - 150), mod);

            //uses some perlin noise to place snow/grass where there is air above ice.
            for(int x = center - 400; x < center + 400; x++)
                for(int y = iceBottom - 200; y < iceBottom + 100; y++)
                {
                    var noise = genNoise.GetPerlin(x, y);
                    if (Math.Abs(noise) > 0.5f) continue;

                    Tile floor = Framing.GetTileSafely(x, y);
                    Tile above = Framing.GetTileSafely(x, y - 1);

                    if (floor.type == TileType<PermafrostIce>() && !above.active())
                    {
                        floor.type = (ushort)TileType<PermafrostSnow>();

                        for(int k  = 0; k <  WorldGen.genRand.Next(4); k++)
                        {
                            WorldGen.PlaceTile(x, y - k, TileType<Tiles.Permafrost.Decoration.SnowGrass>());
                        }
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
                    var lineVector = new Vector2(line.Z - line.X, line.W - line.Y);

                    var colliding = false;

                    for (int n = 0; n < circles.Count; n++) //first, check against the circles
                    {
                        Circle check = circles[n].Inflate(11);
                        if (n == k || n == j) continue;

                        var off = check.position.ToVector2() - line.XY();

                        var dot = Vector2.Dot(off, lineVector);
                        var distAlong = dot / lineVector.LengthSquared();

                        if (distAlong > 0 && distAlong < 1) //we've passed the check against the infinite line
                        {
                            float dist = Vector2.Distance(line.XY() + lineVector * distAlong, check.position.ToVector2());

                            if(dist < check.radius)
                            {
                                colliding = true;
                                break;
                            }
                        }
                    }

                    for(int n = 0; n < output.Count; n++) //next, check against the lines we already have to prevent intersections
                    {
                        var enemyLine = output[n];

                        if (line == enemyLine) continue;
                        if (line.XY() == enemyLine.XY() || line.XY() == enemyLine.ZW()) continue; //if lines share a point it's fine, because the intersection will be at a sphere
                        if (line.ZW() == enemyLine.XY() || line.ZW() == enemyLine.ZW()) continue;

                        if (SegmentsColliding(line, enemyLine))
                        {
                            colliding = true;
                            break;
                        }
                    }

                    var caveLength = lineVector.Length() - circles[k].radius - circles[j].radius;
                    if (!colliding && caveLength < 80) //final check to add a max length
                        output.Add(line);
                }
            }

            return output;
        }

        //Zoinked from https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/

        private bool SegmentsColliding(Vector4 line1, Vector4 line2)
        {
            var p1 = line1.XY();
            var q1 = line1.ZW();
            var p2 = line2.XY();
            var q2 = line2.ZW();

            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            if (o1 != o2 && o3 != o4)
                return true;

            if (o1 == 0 && onSegment(p1, p2, q1)) return true;
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; 
        }

        static bool onSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        static int orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            int val = (int)((q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y));
            if (val == 0) return 0;  

            return (val > 0) ? 1 : 2; 
        }

        private void DigTunnel(Vector4 line, int row, bool digOnly = false)
        {
            var length = (line.XY() - line.ZW()).Length();

            if (!digOnly)
            {
                for (float k = 0; k < 1; k += 3 / length)
                {
                    Point16 pos = Vector2.Lerp(line.XY(), line.ZW(), k).ToPoint16();
                    int off = 7 + (int)Math.Abs(genNoise.GetPerlin(k * 1000, row) * 5);

                    for (int x = pos.X - off; x < pos.X + off; x++)
                    {
                        for (int y = pos.Y - off; y < pos.Y + off; y++)
                        {
                            WorldGen.PlaceTile(x, y, TileType<PermafrostIce>(), true, true);
                            WorldGen.SlopeTile(x, y);
                        }
                    }
                }
            }

            for (float k = 0; k < 1; k += 1 / length)
            {
                Point16 pos = Vector2.Lerp(line.XY(), line.ZW(), k).ToPoint16();
                int off = 4 + (int)Math.Abs(genNoise.GetPerlin(k * 1000, row) * 5);

                for (int x = pos.X - off; x < pos.X + off; x++)
                {
                    for (int y = pos.Y - off; y < pos.Y + off; y++)
                    {
                        WorldGen.KillTile(x, y);
                        Framing.GetTileSafely(x, y).liquid = 0;

                        if(!digOnly)
                            Framing.GetTileSafely(x, y).wall = WallID.SnowWallUnsafe;
                    }
                }
            }
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
                        WorldGen.SlopeTile(pos.X + x, pos.Y + y);
                    }
                }
        }

        private void DecorateTunnel(Vector4 line)
        {
            var length = (line.XY() - line.ZW()).Length();
            var row = WorldGen.genRand.Next(1000);

            int pillarCD = 0;

            for (float k = 30 / length; k < 1 - 30 / length; k += 5 / length)
            {
                Point16 pos = Vector2.Lerp(line.XY(), line.ZW(), k).ToPoint16();
                int off = 10;

                //spike banks and walls
                for (int x = pos.X - off; x < pos.X + off; x++)
                {
                    for (int y = pos.Y - off; y < pos.Y + off; y++)
                    {
                        Tile floor = Framing.GetTileSafely(x, y);
                        Tile above = Framing.GetTileSafely(x, y - 1);
                        Tile under = Framing.GetTileSafely(x, y + 1);

                        if (floor.type == TileType<PermafrostIce>() && !above.active())
                        {
                            var up = Math.Abs(genNoise.GetPerlin(x * 20, 100) * 6);

                            if (up >= 2)
                                for (int n = 0; n < up; n++)
                                {
                                    if(Framing.GetTileSafely(x, y - n).wall == WallID.SnowWallUnsafe)
                                        WorldGen.PlaceTile(x, y - n, TileType<IceSpike>());
                                }
                        }

                        if (floor.type == TileType<PermafrostIce>() && !under.active())
                        {
                            var up = Math.Abs(genNoise.GetPerlin(x * 20, 230) * 6);

                            if (up >= 2)
                                for (int n = 0; n < up; n++)
                                {
                                    if (Framing.GetTileSafely(x, y + n).wall == WallID.SnowWallUnsafe)
                                        WorldGen.PlaceTile(x, y + n, TileType<IceSpike>());
                                }
                        }
                    }
                }

                //photoreactive pillars
                if (pillarCD > 0) pillarCD--;

                if (pillarCD == 0 && WorldGen.genRand.Next(7) == 0)
                {
                    pillarCD = 4;

                    var pillar = Vector2.Normalize(line.XY() - line.ZW()).RotatedBy(Math.PI / 2 + WorldGen.genRand.NextFloat(-0.5f, 0.5f));

                    for (int n = -10; n < 10; n++)
                    {
                        Point16 point = (pos.ToVector2() + pillar * n).ToPoint16();

                        for (int x = point.X - 1; x < point.X + 1; x++)
                            for (int y = point.Y - 1; y < point.Y + 1; y++)
                            {
                                if (Framing.GetTileSafely(x, y).wall == WallID.SnowWallUnsafe)
                                    WorldGen.PlaceTile(x, y, TileType<PhotoreactiveIce>());
                            }
                    }
                }
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

        public Circle Inflate(int amount) => new Circle(position, radius + amount);
    }
}
