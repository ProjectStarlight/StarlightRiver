using Microsoft.Xna.Framework;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.IO;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld : ModSystem
    {
        public static void BookAltarGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Hiding Codex...";

            Vector2 spawn = FindSand();
            StructureHelper.Generator.GenerateStructure("Structures/CodexTemple", spawn.ToPoint16(), StarlightRiver.Instance);
        }

        private static Vector2 FindSand()
        {
            for (int i = WorldGen.UndergroundDesertLocation.X; i < Main.maxTilesX; i++)
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    if (Main.tile[i, j].type == TileID.Sand)
                    {
                        if (Helper.AirScanUp(new Vector2(i, j), 40)) return new Vector2(i, j - 30);
                        else break;
                    }
                }

            return new Vector2(WorldGen.UndergroundDesertLocation.X, 400);
        }
    }
}