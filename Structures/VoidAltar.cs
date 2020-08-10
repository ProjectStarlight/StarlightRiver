using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Tiles.Void;
using Terraria;
using Terraria.ID;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Structures
{
    public partial class GenHelper
    {
        public static void VoidAltarGen(GenerationProgress progress)
        {
            progress.Message = "Opening the Gates...";

            Texture2D Courtyard = GetTexture("StarlightRiver/Structures/VoidAltar");
            Vector2 spawn = new Vector2(Main.maxTilesX / 4, Main.maxTilesY - 100);
            StarlightWorld.RiftLocation = (spawn + new Vector2(25.5f, 3.5f)) * 16;

            for (int y = 0; y < Courtyard.Height; y++) // for every row
            {
                Color[] rawData = new Color[Courtyard.Width]; //array of colors
                Rectangle row = new Rectangle(0, y, Courtyard.Width, 1); //one row of the image
                Courtyard.GetData<Color>(0, row, rawData, 0, Courtyard.Width); //put the color data from the image into the array

                for (int x = 0; x < Courtyard.Width; x++) //every entry in the row
                {
                    Main.tile[(int)spawn.X + x, (int)spawn.Y + y].ClearEverything(); //clear the tile out
                    Main.tile[(int)spawn.X + x, (int)spawn.Y + y].liquidType(0); // clear liquids

                    ushort placeType = 0;
                    ushort wallType = 0;
                    switch (rawData[x].R) //select block
                    {
                        case 10: placeType = TileID.Ash; break;
                        case 20: placeType = (ushort)TileType<VoidBrick>(); break;
                        case 30: placeType = (ushort)TileType<VoidStone>(); break;
                        case 40: placeType = TileID.Platforms; break;
                            //case 50: placeType = (ushort)ModContent.TileType<Tiles.Rift.MainRift>(); break;
                    }
                    switch (rawData[x].B) //select wall
                    {
                        case 10: wallType = (ushort)WallType<VoidWall>(); break;
                    }

                    if (placeType != 0) { WorldGen.PlaceTile((int)spawn.X + x, (int)spawn.Y + y, placeType, true, true); } //place block
                    if (wallType != 0) { WorldGen.PlaceWall((int)spawn.X + x, (int)spawn.Y + y, wallType, true); } //place wall
                }
            }
        }
    }
}