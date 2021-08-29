using StarlightRiver.Content.Items.Beach;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
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

            PlaceForestStructure(500, Main.maxTilesX / 2 - 100);
            PlaceForestStructure(Main.maxTilesX / 2 + 100, Main.maxTilesX - 500);
        }

        private bool PlaceForestStructure(int xStart, int xEnd)
		{
            int lastForestVariant = -1;

            for (int x = xStart; x < xEnd; x++)
                for (int y = 100; y < Main.maxTilesY; y++)
                {
                    var tile = Framing.GetTileSafely(x, y);

                    if ((tile.collisionType == 1 && tile.type != TileID.Grass) || tile.liquid > 0)
                        break;
                    else if (tile.active() && tile.type == TileID.Grass && Helper.AirScanUp(new Microsoft.Xna.Framework.Vector2(x, y - 1), 10) && WorldGen.genRand.Next(20) == 0)
                    {
                        Point16 dims = new Point16();

                        int selection = WorldGen.genRand.Next(7);

                        while (selection == lastForestVariant)
                            selection = WorldGen.genRand.Next(7);

                        StructureHelper.Generator.GetMultistructureDimensions("Structures/ForestStructures", mod, selection, ref dims);

                        int off = 3;

                        if (selection == 5) off = 12;
                        if (selection == 6) off = 4;

                        if (!Framing.GetTileSafely(x + dims.X, y - 2).active() && Framing.GetTileSafely(x + dims.X, y + 1).active())
                        {
                            StructureHelper.Generator.GenerateMultistructureSpecific("Structures/ForestStructures", new Point16(x, y - dims.Y + off), mod, selection);
                            lastForestVariant = selection;
                        }

                        x += dims.X * (3 + WorldGen.genRand.Next(3));
                        continue;
                    }
                }
            return true;
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