using StarlightRiver.Content.Items.Beach;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModWorld
    {
        private void SurfaceItemPass(GenerationProgress progress)
        {
            progress.Message = "Placing treasures";
            //Seaglass ring
            PlaceSeaglassRing(0, 500);
            PlaceSeaglassRing(Main.maxTilesX - 500, Main.maxTilesX);
        }

        private bool PlaceSeaglassRing(int xStart, int xEnd)
		{
            for (int x = xStart; x < xEnd; x++)
                for (int y = 100; y < Main.maxTilesY; y++)
                {
                    var tile = Framing.GetTileSafely(x, y);

                    if (tile.active() && tile.type != TileID.Sand || tile.liquid > 0)
                        break;
                    else if (tile.active() && tile.slope() == 0 && !tile.halfBrick() && tile.type == TileID.Sand && Helper.AirScanUp(new Microsoft.Xna.Framework.Vector2(x, y - 1), 10) && WorldGen.genRand.Next(20) == 0)
                    {
                        var newTile = Framing.GetTileSafely(x, y - 1);
                        newTile.ClearEverything();
                        newTile.active(true);
                        newTile.type = (ushort)TileType<SeaglassRingTile>();

                        return true;
                    }
                }
            return false;
        }
    }
}