using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Structures
{
    public partial class GenHelper
    {
        public static void BoulderSlope(GenerationProgress progress)
        {
            progress.Message = "Setting ancient traps...";

            Texture2D BoulderSlope = GetTexture("StarlightRiver/Assets/Structures/BoulderSlope");
            Vector2 spawn = new Vector2(0, 0);
            List<int> allowedBlocks = new List<int> {
                TileID.Stone,
                TileID.Grass,
                TileID.CorruptGrass,
                TileID.FleshGrass,
                TileID.HardenedSand,
                TileID.Sandstone,
                TileID.SnowBlock,
                TileID.IceBlock,
                TileID.MushroomGrass,
                TileID.JungleGrass,
                TileID.HallowedGrass,
                TileID.BlueMoss,
                TileID.BrownMoss,
                TileID.GreenMoss,
                TileID.LavaMoss,
                TileID.PurpleMoss,
                TileID.RedMoss,
                TileID.CorruptHardenedSand,
                TileID.CorruptIce,
                TileID.CorruptSandstone,
                TileID.Ebonstone,
                TileID.FleshIce,
                TileID.CrimsonHardenedSand,
                TileID.CrimsonSandstone,
                TileID.Crimstone,
                TileID.Granite,
                TileID.Marble
            };//allowed origin blocks

            List<int> stoneMossBlocks = new List<int>
            {
                TileID.BlueMoss,
                TileID.BrownMoss,
                TileID.GreenMoss,
                TileID.LavaMoss,
                TileID.PurpleMoss,
                TileID.RedMoss
            };//blocks that should give a stone wall

            //TODO change generation step, this may generate after the crimson/corruption does, making it never spawn there
            //also: spawn boulder and pressure plate

            for (int k = 0; k < (int)((Main.maxTilesX * Main.maxTilesY) * .000015); k++) //too common atm, add another zero to reduce it to (likely) reasonable levels
            {
                spawn.X = WorldGen.genRand.Next(BoulderSlope.Width, Main.maxTilesX - BoulderSlope.Width);//keeps it 200 blocks away from edge of world, can be decreased if need be
                spawn.Y = WorldGen.genRand.Next((int)WorldGen.rockLayer, Main.maxTilesY - 220);
                ushort spawnTileType = Main.tile[(int)spawn.X + (BoulderSlope.Width / 2), (int)spawn.Y + (BoulderSlope.Height / 2)].type;

                if (allowedBlocks.Contains(spawnTileType))//checks origin for stone, may shift this check to the middle of the structure later, or even check a area for X% or more of stone
                {
                    for (int y = 0; y < BoulderSlope.Height; y++) // for every row
                    {
                        Color[] rawData = new Color[BoulderSlope.Width]; //array of colors
                        Rectangle row = new Rectangle(0, y, BoulderSlope.Width, 1); //one row of the image
                        BoulderSlope.GetData<Color>(0, row, rawData, 0, BoulderSlope.Width); //put the color data from the image into the array

                        for (int x = 0; x < BoulderSlope.Width; x++) //every entry in the row
                        {
                            //ushort currentTileType = Main.tile[(int)spawn.X + x, (int)spawn.Y + y].type;
                            ushort placeType = 0;
                            //ushort wallType = 0;
                            byte slopeType = 0;
                            byte wireType = 0;

                            switch (rawData[x].R) //select block
                            {
                                case 40: placeType = 254; break; //Keeps existing block, Middle                     (currently replaces non-solid blocks) (currently swaps out grass types for base block)
                                case 45: placeType = 253; break; //Keeps existing block, Edges (for grass / moss)   (currently replaces non-solid blocks) (currently uses base block)
                                case 50: placeType = 252; break; //only uses origin, Middle                                                               (currently swaps out grass types for base block)
                                case 55: placeType = 251; break; //only uses origin, Edges                                                                (currently uses base block)
                                case 120: placeType = TileID.PressurePlates; break; //doesn't work atm
                                case 160: placeType = TileID.Boulder; break; //doesn't work atm
                                case 255: placeType = 255; break; //clear tile
                            }

                            /*switch (rawData[x].B) //select wall (disabled because of one wall
                            {
                                //case 80: wallType = 255; break;
                            }*/

                            switch (rawData[x].G) //select slope and wire
                            {
                                case 40: slopeType = 1; break;
                                case 80: slopeType = 5; break;
                                case 120: wireType = 1; break;
                                case 160: wireType = 5; break; //5 and above will be wires + actuators
                            }

                            if (placeType != 0 && placeType <= 250) { WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, placeType, true, true); } //used for tiles that dont change between structures
                            else switch (placeType) //place types
                                {
                                    case 255:
                                        Main.tile[(int)spawn.X + x, (int)spawn.Y + y].ClearEverything();
                                        if (spawnTileType == TileID.Marble)
                                        {
                                            WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.MarbleUnsafe, true);
                                        }
                                        else if (spawnTileType == TileID.Granite)
                                        {
                                            WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.MarbleUnsafe, true);
                                        }
                                        else if (spawnTileType == TileID.Sandstone)
                                        {
                                            WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.Sandstone, true);
                                        }
                                        else if (spawnTileType == TileID.HardenedSand)
                                        {
                                            WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.HardenedSand, true);
                                        }
                                        else if (spawnTileType == TileID.Crimstone)
                                        {
                                            WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.CrimstoneUnsafe, true);
                                        }
                                        else if (spawnTileType == TileID.Ebonstone)
                                        {
                                            WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.EbonstoneUnsafe, true);
                                        }
                                        else if (spawnTileType == TileID.JungleGrass)
                                        {
                                            WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.JungleUnsafe, true);
                                        }
                                        break;

                                    case 254:
                                        if (Main.tile[(int)spawn.X + x, (int)spawn.Y + y].collisionType == 0) //places tiles, only replaces blocks with no collision
                                        {
                                            if (spawnTileType == TileID.MushroomGrass || spawnTileType == TileID.JungleGrass)//blocks to mud
                                            {
                                                WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, TileID.Mud, true, true);
                                            }
                                            else if (stoneMossBlocks.Contains(spawnTileType)) //blocks to stone
                                            {
                                                WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, TileID.Stone, true, true);
                                            }
                                            else if (spawnTileType == TileID.Grass || spawnTileType == TileID.CorruptGrass || spawnTileType == TileID.FleshGrass)//blocks to dirt
                                            {
                                                WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, TileID.Dirt, true, true);
                                            }
                                            else
                                            {
                                                WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, spawnTileType, true, true);
                                            }
                                        }
                                        else
                                        {
                                            Main.tile[(int)spawn.X + x, (int)spawn.Y + y].slope(0); //deslopes
                                        }
                                        break;

                                    case 253:
                                        if (Main.tile[(int)spawn.X + x, (int)spawn.Y + y].collisionType == 0)
                                        {
                                            WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, spawnTileType, true, true);
                                        }
                                        else
                                        {
                                            Main.tile[(int)spawn.X + x, (int)spawn.Y + y].slope(0); //deslopes
                                        }
                                        break;

                                    case 252:
                                        if (spawnTileType == TileID.MushroomGrass || spawnTileType == TileID.JungleGrass) //same case as 254 but overrides everything
                                        {
                                            WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, TileID.Mud, true, true);
                                        }
                                        else if (stoneMossBlocks.Contains(spawnTileType)) //blocks to stone
                                        {
                                            WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, TileID.Stone, true, true);
                                        }
                                        else if (spawnTileType == TileID.Grass || spawnTileType == TileID.CorruptGrass || spawnTileType == TileID.FleshGrass)//blocks to dirt
                                        {
                                            WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, TileID.Dirt, true, true);
                                        }
                                        else
                                        {
                                            WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, spawnTileType, true, true);
                                        }
                                        Main.tile[(int)spawn.X + x, (int)spawn.Y + y].slope(0); //deslopes
                                        break;

                                    case 251:
                                        WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, spawnTileType, true, true);
                                        break;
                                }

                            //if (wallType != 0 && wallType != 255) { WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, wallType, true); } //place wall

                            if (rawData[x].B == 80) //only one wall color, so the just an if instead of a switch statement
                            {
                                switch (spawnTileType)
                                {
                                    case TileID.Grass: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.DirtUnsafe, true); break;
                                    case TileID.Stone: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.Stone, true); break;
                                    case TileID.JungleGrass: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.MudUnsafe, true); break;
                                    case TileID.MushroomGrass: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.MushroomUnsafe, true); break;

                                    case TileID.BlueMoss: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.Stone, true); break;
                                    case TileID.BrownMoss: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.Stone, true); break;
                                    case TileID.GreenMoss: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.Stone, true); break;
                                    case TileID.LavaMoss: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.Stone, true); break;
                                    case TileID.PurpleMoss: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.Stone, true); break;
                                    case TileID.RedMoss: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.Stone, true); break;

                                    case TileID.IceBlock: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.IceUnsafe, true); break;
                                    case TileID.SnowBlock: WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, WallID.SnowWallUnsafe, true); break;
                                }
                            }

                            if (slopeType != 0) { Main.tile[(int)spawn.X + x, (int)spawn.Y + y].slope(slopeType); }

                            switch (wireType) //select block
                            {
                                case 1: Main.tile[(int)spawn.X + x, (int)spawn.Y + y].wire(true); break; //red wire
                                case 5: Main.tile[(int)spawn.X + x, (int)spawn.Y + y].wire(true); Main.tile[(int)spawn.X + x, (int)spawn.Y + y].actuator(true); break; //red wire plus actuator
                            }
                        }
                    }
                }
                // WorldGen.PlaceTile((int)spawn.X, (int)spawn.Y, TileID.DiamondGemspark, true, true); //debug
                //WorldGen.PlaceTile((int)spawn.X + (BoulderSlope.Width / 2), (int)spawn.Y + (BoulderSlope.Height / 2), TileID.RubyGemspark, true, true);
            }
        }
    }
}