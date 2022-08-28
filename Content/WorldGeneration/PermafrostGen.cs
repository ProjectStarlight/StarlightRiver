using Microsoft.Xna.Framework;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld : ModSystem
    {
        public static int permafrostCenter;

        public void PermafrostGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Permafrost generation";

            int iceLeft = 0;
            int iceRight = 0;
            int iceBottom = 0;

            for (int x = 0; x < Main.maxTilesX; x++) //Find the ice biome since vanilla dosent track it
            {
                if (iceLeft != 0) break;

                for (int y = 0; y < Main.maxTilesY; y++)
                    if (Main.tile[x, y].TileType == TileID.IceBlock)
                    {
                        iceLeft = x;
                        break;
                    }
            }

            for (int x = Main.maxTilesX - 1; x > 0; x--)
            {
                if (iceRight != 0) break;

                for (int y = 0; y < Main.maxTilesY; y++)
                    if (Main.tile[x, y].TileType == TileID.IceBlock)
                    {
                        iceRight = x;
                        break;
                    }
            }

            for (int y = Main.maxTilesY - 1; y > 0; y--)
                if (Main.tile[iceLeft, y].TileType == TileID.IceBlock)
                {
                    iceBottom = y;
                    break;
                }

            int center = iceLeft + (iceRight - iceLeft) / 2;
            int centerY = (int)WorldGen.worldSurfaceHigh + (iceBottom - (int)WorldGen.worldSurfaceHigh) / 2;

            TryToGenerateArena:

            if(center < iceLeft || center > iceRight - 109)
                center = iceLeft + (iceRight - iceLeft) / 2;

            for (int x1 = 0; x1 < 109; x1++)
                for(int y1 = 0; y1 < 180; y1++)
				{
                    Tile tile = Framing.GetTileSafely(center - 40 + x1, centerY + 100 + y1);
                    if(tile.TileType == TileID.BlueDungeonBrick || tile.TileType == TileID.GreenDungeonBrick || tile.TileType == TileID.PinkDungeonBrick)
					{
                        center += Main.rand.Next(-1, 2) * 109;
                        goto TryToGenerateArena;
                    }
				}

            SquidBossArena = new Rectangle(center - 40, centerY + 100, 109, 180);
            StructureHelper.Generator.GenerateStructure("Structures/SquidBossArena", new Point16(center - 40, centerY + 100), Mod);

            Vector2 oldPos = new Vector2(SquidBossArena.Center.X, SquidBossArena.Y) * 16;

            //Find locations for and place the touchstone altars which lead to the boss' arena
            for(int k = 1; k <= 2; k++) 
			{
                float fraction = k / 3f;
                int yTarget = (int)Helper.LerpFloat(SquidBossArena.Y, (float)WorldGen.worldSurfaceHigh, fraction);

                for (int x = 0; x < Main.maxTilesX; x++) 
                {
                    if (Main.tile[x, yTarget].TileType == TileID.IceBlock)
                    {
                        iceLeft = x;
                        break;
                    }
                }

                for (int x = Main.maxTilesX - 1; x > 0; x--)
                {
                    if (Main.tile[x, yTarget].TileType == TileID.IceBlock)
                    {
                        iceRight = x;
                        break;
                    }
                }

                int iceCenter = iceLeft + (iceRight - iceLeft) / 2;
                int xTarget = iceCenter + WorldGen.genRand.Next(-100, 100);

                StructureHelper.Generator.GenerateStructure("Structures/TouchstoneAltar", new Point16(xTarget, yTarget), Mod);
                (TileEntity.ByPosition[new Point16(xTarget + 9, yTarget + 6)] as TouchstoneTileEntity).targetPoint = oldPos;
                oldPos = new Vector2(xTarget + 11, yTarget + 9) * 16;
            }

            for (int y = 14; y < Main.maxTilesY - 200; y++)
            {             
                if (Main.tile[center, y].TileType == TileID.SnowBlock || Main.tile[center, y].TileType == TileID.IceBlock)
                {
                    StructureHelper.Generator.GenerateStructure("Structures/TouchstoneAltar", new Point16(center, y - 12), Mod);
                    (TileEntity.ByPosition[new Point16(center + 9, y - 12 + 6)] as TouchstoneTileEntity).targetPoint = oldPos;
                    break;
                }

                if (Main.tile[center, y].HasTile && Main.tileSolid[Main.tile[center, y].TileType])
                {                  
                    center += center > (iceLeft + (iceRight - iceLeft) / 2) ? -10 : 10;
                    continue;
                }
            }
        }

        private void PlaceOre(Point16 center)
		{
            var radius = Main.rand.Next(2, 5);

            int frameStartX = radius == 4 ? 5 : radius == 3 ? 2 : 0;
            int frameStartY = radius == 4 ? 0 : radius == 3 ? 1 : 2;

            for (int x = center.X; x < center.X + radius; x++)
                for (int y = center.Y; y < center.Y + radius; y++)
                {
                    int xRel = x - center.X;
                    int yRel = y - center.Y;

                    Tile tile = Framing.GetTileSafely(x, y);
                    tile.HasTile = true;
                    tile.TileType = (ushort)TileType<AuroraIce>();
                    tile.TileFrameX = (short)((frameStartX + xRel) * 18);
                    tile.TileFrameY = (short)((frameStartY + yRel) * 18);

                    int r = radius - 1;
                    if (xRel == 0 && yRel == 0) tile.Slope = SlopeType.SlopeDownRight;
                    if (xRel == 0 && yRel == r) tile.Slope = SlopeType.SlopeUpRight;
                    if (xRel == r && yRel == 0) tile.Slope = SlopeType.SlopeDownLeft;
                    if (xRel == r && yRel == r) tile.Slope = SlopeType.SlopeUpLeft;

                    var dum = false;
                    GetInstance<AuroraIce>().TileFrame(x, y, ref dum, ref dum);
                }
        }
    }
}
