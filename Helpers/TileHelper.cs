using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Content.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.UI;
using Terraria;
using static Terraria.ModLoader.ModContent;
using static Terraria.WorldGen;


namespace StarlightRiver.Helpers
{
    public static partial class Helper
    {
        public static double Distribution(int pos, int maxVal, float posOffset = 0.5f, float maxChance = 100f)
        {
            return -Math.Pow((20 * (pos - (posOffset * maxVal))) / maxVal, 2) + maxChance;
        }

        public static void OutlineRect(Rectangle rect, int tileType)
        {
            for (int i = 0; i < rect.Width; i++)
                PlaceTile(rect.X + i, rect.Y, tileType, true, true);

            for (int i = 0; i < rect.Width; i++)
                PlaceTile(rect.X + i, rect.Y + rect.Height, tileType, true, true);

            for (int i = 0; i < rect.Height; i++)
                PlaceTile(rect.X, rect.Y + i, tileType, true, true);

            for (int i = 0; i < rect.Height; i++)
                PlaceTile(rect.X + rect.Width, rect.Y + i, tileType, true, true);
        }

        public static void PlaceMultitile(Point16 position, int type, int style = 0)
        {
            TileObjectData data = TileObjectData.GetTileData(type, style); //magic numbers and uneccisary params begone!

            if (position.X + data.Width > Main.maxTilesX || position.X < 0) return; //make sure we dont spawn outside of the world!
            if (position.Y + data.Height > Main.maxTilesY || position.Y < 0) return;

            int xVariants = 0;
            int yVariants = 0;
            if (data.StyleHorizontal) xVariants = Main.rand.Next(data.RandomStyleRange);
            else yVariants = Main.rand.Next(data.RandomStyleRange);

            for (int x = 0; x < data.Width; x++) //generate each column
            {
                for (int y = 0; y < data.Height; y++) //generate each row
                {
                    Tile tile = Framing.GetTileSafely(position.X + x, position.Y + y); //get the targeted tile
                    tile.type = (ushort)type; //set the type of the tile to our multitile

                    tile.frameX = (short)((x + data.Width * xVariants) * (data.CoordinateWidth + data.CoordinatePadding)); //set the X frame appropriately
                    tile.frameY = (short)((y + data.Height * yVariants) * (data.CoordinateHeights[y] + data.CoordinatePadding)); //set the Y frame appropriately
                    tile.active(true); //activate the tile
                }
            }
        }

        /// <summary>
        /// returns true if every tile in a rectangle is air
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool CheckAirRectangle(Point16 position, Point16 size)
        {
            if (position.X + size.X > Main.maxTilesX || position.X < 0) return false; //make sure we dont check outside of the world!
            if (position.Y + size.Y > Main.maxTilesY || position.Y < 0) return false;

            for (int x = position.X; x < position.X + size.X; x++)
            {
                for (int y = position.Y; y < position.Y + size.Y; y++)
                {
                    if (Main.tile[x, y].active()) return false; //if any tiles there are active, return false!
                }
            }
            return true;
        }
        /// <summary>
        /// returns true if any tile in a rectanlge is air
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool CheckAnyAirRectangle(Point16 position, Point16 size)
        {
            if (position.X + size.X > Main.maxTilesX || position.X < 0) return false; //make sure we dont check outside of the world!
            if (position.Y + size.Y > Main.maxTilesY || position.Y < 0) return false;

            for (int x = position.X; x < position.X + size.X; x++)
            {
                for (int y = position.Y; y < position.Y + size.Y; y++)
                {
                    if (!Main.tile[x, y].active()) return true; //if any tiles there are inactive, return true!
                }
            }
            return true;
        }

        public static bool AirScanUp(Vector2 start, int MaxScan)
        {
            if (start.Y - MaxScan < 0) { return false; }

            bool clear = true;

            for (int k = 1; k <= MaxScan; k++)
            {
                if (Main.tile[(int)start.X, (int)start.Y - k].active()) { clear = false; }
            }
            return clear;
        }
    }
}

