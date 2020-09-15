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

            SquidBossArena = new Rectangle(center - 40, iceBottom - 100, 80, 200);
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
