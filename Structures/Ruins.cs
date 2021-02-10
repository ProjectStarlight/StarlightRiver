using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Structures
{
    public partial class GenHelper
    {
        public static void RuinsGen(GenerationProgress progress)
        {
            progress.Message = "Spicing up Forests...";
            Texture2D Ruins = GetTexture("StarlightRiver/Structures/Ruins");

            for (int x = 0; x + 16 < Main.maxTilesX; x += Main.rand.Next(8, 16))
            {
                if (Main.rand.Next(4) == 0) // 1/5 chance to generate
                {
                    for (int y = 0; y < Main.maxTilesY; y++) // find the highest grass block
                    {
                        if (Main.tile[x, y].type == TileID.Grass && Math.Abs(x - Main.maxTilesX / 2) >= Main.maxTilesX / 6 && Main.tile[x + 4, y].active() && Main.tile[x + 8, y].active())// valid placement
                        {
                            int variant = Main.rand.Next(5);

                            // Generation Block
                            for (int y2 = 0; y2 < Ruins.Height; y2++) // for every row
                            {
                                Color[] rawData = new Color[8]; //array of colors
                                Rectangle row = new Rectangle(8 * variant, y2, 8, 1); //one row of the image
                                Ruins.GetData<Color>(0, row, rawData, 0, 8); //put the color data from the image into the array

                                for (int x2 = 0; x2 < 8; x2++) //every entry in the row
                                {
                                    Main.tile[x + x2, y + y2].slope(0);

                                    ushort placeType = 0;
                                    ushort wallType = 0;
                                    switch (rawData[x2].R) //select block
                                    {
                                        case 10: placeType = TileID.GrayBrick; break;
                                        case 20: placeType = TileID.LeafBlock; break;
                                    }
                                    switch (rawData[x2].B) //select wall
                                    {
                                        case 10: wallType = WallID.GrayBrick; break;
                                    }

                                    if (placeType != 0) { WorldGen.PlaceTile(x + x2, y - 15 + y2, placeType, true, true); } //place block
                                    if (wallType != 0) { WorldGen.PlaceWall(x + x2, y - 15 + y2, wallType, true); } //place wall
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}