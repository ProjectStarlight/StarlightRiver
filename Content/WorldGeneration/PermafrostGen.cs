using Microsoft.Xna.Framework;
using StarlightRiver.Content.Tiles.Permafrost;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Tiles.Permafrost;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Permafrost.Decoration;
using StarlightRiver.Content.Tiles.Permafrost.VFX;
using StarlightRiver.Helpers;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld : ModWorld
    {
        public static int permafrostCenter;

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
            int width = iceRight - iceLeft;

            Circle bigCircle = new Circle(new Point16(center + 15, iceBottom - 70), 110);
            circles.Add(bigCircle);

            for (int k = 0; k < 150; k++)
            {
                Point16 pos = new Point16(WorldGen.genRand.Next(center - 400, center + 400), WorldGen.genRand.Next(iceBottom - 150, iceBottom + 50));
                int dist = (int)((800 - Vector2.Distance(pos.ToVector2(), bigCircle.position.ToVector2())) / 800f * 30);
                int rad = WorldGen.genRand.Next(3 + dist, 9 + dist);
                Circle circle = new Circle(pos, rad);

                if (!circles.Any(n => n.Colliding(circle)))
                    circles.Add(circle);
            }

            int row = WorldGen.genRand.Next(1000);

            var caves = GenerateLines(circles);
            caves.ForEach(n => DigTunnel(n, row));

            circles.ForEach(n => DigCircle(n));

            caves.ForEach(n => DigTunnel(n, row, true));
            caves.ForEach(n => DecorateTunnel(n));
            PlaceDisc(circles);
            PlaceTeleporters(circles, center);

            circles.ForEach(n => DecorateCircle(n));

            SquidBossArena = new Rectangle(center - 40, iceBottom - 150, 109, 180);
            StructureHelper.StructureHelper.GenerateStructure("Structures/SquidBossArena", new Point16(center - 40, iceBottom - 150), mod);

            MakeCenterGates(bigCircle, caves);

            for (int x = 0; x < 50; x++)
                for (int y = 0; y < 40; y++)
                {
                    if (Main.rand.Next(2) != 0) continue;

                    Point16 point = new Point16((center - 400) + (int)(x / 50f * 800), iceBottom + 50 - (int)(y / 40f * 200));
                    point += new Point16(WorldGen.genRand.Next(-10, 10), WorldGen.genRand.Next(-10, 10));

                    Tile tile = Framing.GetTileSafely(point.X, point.Y);
                    if (tile.type == TileType<PermafrostIce>()) PlaceOre(point, WorldGen.genRand.Next(2, 5));
                }

            //entrances
            for (int k = 0; k < 4; k++)
            {
                int y = iceBottom - 200;
                int x = center - 400 + (int)(800 * (k / 3f));

                DigCircle(new Circle(new Point16(x, y), 10));
            }

            //uses some perlin noise to place snow/grass where there is air above ice.
            for (int x = center - 400; x < center + 400; x++)
                for (int y = iceBottom - 200; y < iceBottom + 100; y++)
                {
                    var noise = genNoise.GetPerlin(x, y);
                    if (Math.Abs(noise) > 0.5f) continue;

                    Tile floor = Framing.GetTileSafely(x, y);
                    Tile above = Framing.GetTileSafely(x, y - 1);

                    if (floor.type == TileType<PermafrostIce>() && !above.active())
                    {
                        floor.type = (ushort)TileType<PermafrostSnow>();

                        for (int k = 0; k < WorldGen.genRand.Next(4); k++)
                        {
                            WorldGen.PlaceTile(x, y - k, TileType<SnowGrass>());
                        }
                    }
                }

            permafrostCenter = center; //set this for use in the ash hell generation
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

                            if (dist < check.radius)
                            {
                                colliding = true;
                                break;
                            }
                        }
                    }

                    for (int n = 0; n < output.Count; n++) //next, check against the lines we already have to prevent intersections
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

                        if (!digOnly)
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

        private void DecorateTunnel(Vector4 line) //randomly picks a style of tunnel to generate
        {
            var style = Main.rand.Next(1);

            switch (style)
            {
                case 0: DecorateTunnelSpikes(line); break;
            }
        }

        private void DecorateTunnelMist(Vector4 line)
        {

        }

        private void DecorateTunnelSpikes(Vector4 line)
        {
            var length = (line.XY() - line.ZW()).Length();
            var row = WorldGen.genRand.Next(1000);

            int pillarCD = 0;
            int orbCD = 0;

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
                                    if (Framing.GetTileSafely(x, y - n).wall == WallID.SnowWallUnsafe)
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

                //orbs
                if (orbCD > 0) orbCD--;

                if (orbCD == 0)
                {
                    //I have to do this because PlaceTile and PlaceObject crash for some ungodly reason. Dont ask.
                    Tile tile = Framing.GetTileSafely(pos.X, pos.Y);

                    if (!tile.active())
                    {
                        tile.active(true);
                        tile.type = (ushort)TileType<SpikeImmuneOrb>();
                        tile.frameX = 0;
                        tile.frameY = 0;
                    }

                    orbCD = 4;
                }
            }
        }

        private void DecorateCircle(Circle circle)
        {
            if (circle.decorated) return;

            var under = new Rectangle(circle.position.X - circle.radius - 1, circle.position.Y + 18, circle.radius * 2 + 2, circle.radius - 12);
            if (circle.radius > 22 && !CheckForTunnel(under)) //no tunnels under this circle
            {
                WorldGen.PlaceTile(circle.position.X - 15, circle.position.Y - 15, TileType<CaveVFX>()); //aurora VFX actor
                Helper.PlaceMultitile(circle.position - new Point16(8, 0), TileType<BigTree>()); //tree

                for (int x = -4; x <= 4; x++) //island under the tree
                    for (int y = 0; y < 2; y++)
                    {
                        WorldGen.PlaceTile(circle.position.X + x, circle.position.Y + 17 + y, TileType<PermafrostSnow>());
                    }

                FloodRectangle(under); //water under the tree

                for (int x = -circle.radius; x <= circle.radius; x++) //layer of ice over the water
                {
                    WorldGen.PlaceTile(circle.position.X + x, circle.position.Y + 17, TileType<PhotoreactiveIce>());
                }
            }
        }

        private bool CheckForTunnel(Rectangle rect)
        {
            for (int x = rect.X; x < rect.X + rect.Width; x++)
                for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);
                    if (tile.wall == WallID.SnowWallUnsafe) return true;
                }

            return false;
        }

        private void FloodRectangle(Rectangle rect)
        {
            for (int x = rect.X; x < rect.X + rect.Width; x++)
                for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);
                    if (!tile.active())
                    {
                        tile.liquid = 255;
                        tile.liquidType(0);
                    }
                }
        }

        private void MakeCenterGates(Circle circle, List<Vector4> lines)
        {
            List<Vector4> importantLines = new List<Vector4>();

            foreach (Vector4 line in lines.Where(n => n.XY() == circle.position.ToVector2() || n.ZW() == circle.position.ToVector2()))
                importantLines.Add(line);

            foreach (Vector4 line in importantLines)
            {
                int windup = 0;

                if (line.XY() == circle.position.ToVector2())
                {
                    for (float k = 0; k < 1; k += 5 / Vector2.Distance(line.XY(), line.ZW()))
                    {
                        Point16 pos = Vector2.Lerp(line.XY(), line.ZW(), k).ToPoint16();
                        Tile tile = Framing.GetTileSafely(pos.X, pos.Y);
                        if (tile.wall == WallID.SnowWallUnsafe)
                        {
                            windup++;
                            if (windup >= 2)
                            {
                                MakeGate(pos, line);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    for (float k = 1; k > 0; k -= 5 / Vector2.Distance(line.XY(), line.ZW()))
                    {
                        Point16 pos = Vector2.Lerp(line.XY(), line.ZW(), k).ToPoint16();
                        Tile tile = Framing.GetTileSafely(pos.X, pos.Y);
                        if (tile.wall == WallID.SnowWallUnsafe)
                        {
                            windup++;
                            if (windup >= 2)
                            {
                                MakeGate(pos, line);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void MakeGate(Point16 pos, Vector4 line)
        {
            var pillar = Vector2.Normalize(line.XY() - line.ZW()).RotatedBy(Math.PI / 2);

            for (int n = -10; n < 10; n++)
            {
                Point16 point = pos + (pillar * n).ToPoint16();

                for (int x = point.X - 2; x < point.X + 2; x++)
                    for (int y = point.Y - 2; y < point.Y + 2; y++)
                    {
                        Tile tile = Framing.GetTileSafely(x, y);

                        if (tile.wall == WallID.SnowWallUnsafe)
                            WorldGen.PlaceTile(x, y, TileType<DiscGate>());
                    }

                for (int x = point.X - 7; x < point.X + 7; x++)
                    for (int y = point.Y - 7; y < point.Y + 7; y++)
                    {
                        Tile tile = Framing.GetTileSafely(x, y);

                        if (tile.active() && tile.wall == WallID.SnowWallUnsafe && tile.type != TileType<PermafrostIce>() && tile.type != TileType<DiscGate>())
                            tile.active(false);
                    }

                WorldGen.PlaceTile(pos.X, pos.Y, TileType<DiscHole>(), false, true);
            }
        }

        private void PlaceDisc(List<Circle> circles)
        {
            List<Circle> sorted = circles;
            sorted.Sort((i, j) => i.position.X > j.position.X ? 1 : -1);

            var index = WorldGen.genRand.NextBool() ? 0 : sorted.Count - 1;
            var circle = sorted[index];

            WorldGen.PlaceObject(circle.position.X, circle.position.Y, TileType<Content.Tiles.Misc.AuroraDiscTile>()); //TODO: Replace with full structure    
            circle.decorated = true;
        }

        private void PlaceTeleporters(List<Circle> circles, int center)
        {
            List<Circle> lefts = new List<Circle>();
            List<Circle> rights = new List<Circle>();

            foreach (Circle circle in circles)
            {
                if (circle.position.X < center) lefts.Add(circle);
                else rights.Add(circle);
            }

            lefts.Sort((i, j) => i.position.X < j.position.X ? 1 : -1);
            rights.Sort((i, j) => i.position.X > j.position.X ? 1 : -1);

            PlaceTeleporter(lefts[1].position, rights[1].position);
            PlaceTeleporter(rights[1].position, lefts[1].position);

            lefts[1].decorated = true;
            rights[1].decorated = true;
        }

        private void PlaceTeleporter(Point16 pos, Point16 target)
        {
            Helper.PlaceMultitile(pos, TileType<PermafrostTeleporter>());
            TileEntity.PlaceEntityNet(pos.X, pos.Y, TileEntityType<PermafrostTeleporterEntity>());

            var entity = TileEntity.ByPosition[pos] as PermafrostTeleporterEntity;
            entity.target = target.ToVector2() * 16;
        }

        private void PlaceOre(Point16 center, int radius) //this is a total joke of optimization but who cares its worldgen in funny pixel game hahaaaaaa kill me
        {
            bool stayAlive = false;

            for (int x = center.X; x < center.X + radius; x++) //again im just brute forcing this at this point lmao
                for (int y = center.Y; y < center.Y + radius; y++)
                {
                    if (Framing.GetTileSafely(x, y).wall == WallID.SnowWallUnsafe) stayAlive = true;
                    if (!(Framing.GetTileSafely(x, y).type == TileType<PermafrostIce>() || Framing.GetTileSafely(x, y).type == TileID.Dirt)) return; //uh oh were trying to generate over something invalid
                }

            if (!stayAlive) return;

            int frameStartX = radius == 4 ? 5 : radius == 3 ? 2 : 0; //im lazy and really dont feel like doing math right now. And this is probably faster for the extremely limited use case of this. Is it expansibile? no, but it probably wont need to be and if it needs to be tahts a problem for future me
            int frameStartY = radius == 4 ? 0 : radius == 3 ? 1 : 2;

            for (int x = center.X; x < center.X + radius; x++)
                for (int y = center.Y; y < center.Y + radius; y++)
                {
                    int xRel = x - center.X;
                    int yRel = y - center.Y;

                    Tile tile = Framing.GetTileSafely(x, y);
                    tile.active(true);
                    tile.type = (ushort)TileType<AuroraIce>();
                    tile.frameX = (short)((frameStartX + xRel) * 18);
                    tile.frameY = (short)((frameStartY + yRel) * 18);

                    int r = radius - 1;
                    if (xRel == 0 && yRel == 0) tile.slope(2);
                    if (xRel == 0 && yRel == r) tile.slope(4);
                    if (xRel == r && yRel == 0) tile.slope(1);
                    if (xRel == r && yRel == r) tile.slope(3);

                    bool dummy = false; //this really shouldn't matter for anything but if something goes catastrophic here tell me
                    GetInstance<AuroraIce>().TileFrame(x, y, ref dummy, ref dummy);
                }
        }
    }

    public class Circle
    {
        public Point16 position;
        public int radius;
        public bool decorated = false;

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
