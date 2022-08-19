using StarlightRiver.Content.Items.Beach;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
    {
        private void AnkhChestPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Hiding ancient secrets";

            int tries = 0;

            for (int i = 0; i < 5; i++)
            {
                if (!PlaceAnkhChest(WorldGen.UndergroundDesertLocation.X + Main.rand.Next(-500, 500), WorldGen.UndergroundDesertLocation.Y + (WorldGen.UndergroundDesertLocation.Height / 2) + Main.rand.Next(-500, 500)))
                {
                    tries++;
                    i--;
                    if (tries > 999)
                        break;
                }
            }
        }

        private bool PlaceAnkhChest(int i, int j)
        {
            if (i < 0 || i > Main.maxTilesX || j < 0 || j > Main.maxTilesY)
                return false;

            Tile testTile = Framing.GetTileSafely(i, j + 1);

            if (testTile.TileType != TileID.Sandstone)
                return false;

            if (WorldGen.PlaceChest(i, j, (ushort)ModContent.TileType<Content.Tiles.Desert.AnkhChest>(), false, 2) != -1)
                if (Framing.GetTileSafely(i, j).TileType == ModContent.TileType<Content.Tiles.Desert.AnkhChest>())
                    return true;

            return false;
        }
    }
}