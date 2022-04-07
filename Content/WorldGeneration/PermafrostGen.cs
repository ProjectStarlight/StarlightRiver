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
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld : ModSystem
    {
        public static int permafrostCenter;

        private void PermafrostGen(GenerationProgress progress)
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
            int width = iceRight - iceLeft;


            SquidBossArena = new Rectangle(center - 40, iceBottom - 150, 109, 180);
            StructureHelper.Generator.GenerateStructure("Structures/SquidBossArena", new Point16(center - 40, iceBottom - 150), Mod);
        }
    }
}
