using Microsoft.Xna.Framework;
using StarlightRiver.Tiles.Permafrost;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver
{
    public partial class StarlightWorld : ModWorld
    {
        private void PermafrostGen(GenerationProgress progress)
        {
            progress.Message = "Titty";

            int iceLeft = 0;
            int iceRight = 0;
            int iceBottom = 0;

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


            for (int x = iceLeft; x < iceRight; x++) //hey look the ice biome!
                for (int y = iceBottom - 150; y < iceBottom + 50; y++)
                {
                    if (Main.tile[x, y].type == TileID.IceBlock) Main.tile[x, y].type = (ushort)TileType<PermafrostIce>();
                    if (!Main.tile[x, y - 1].active()) Main.tile[x, y].type = (ushort)TileType<PermafrostSnow>();
                }

            int center = iceLeft + (iceRight - iceLeft) / 2;

            SquidBossArena = new Rectangle(center - 40, iceBottom - 100, 80, 200);
            StructureHelper.StructureHelper.GenerateStructure("Structures/SquidBossArena", new Point16(center - 40, iceBottom - 150), mod);
        }
    }
}
