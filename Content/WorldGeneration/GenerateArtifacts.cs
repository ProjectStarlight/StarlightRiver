using StarlightRiver.Content.Items.Beach;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using System.Linq;
using System.Collections.Generic;
using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
    {
        private void ArtifactGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Hiding ancient secrets";

            int tries = 0;

            List<ModTileEntity> desertFossils = new List<ModTileEntity>();
            desertFossils.Add(ModContent.GetInstance<Content.Archaeology.DesertArtifact1>());
            desertFossils.Add(ModContent.GetInstance<Content.Archaeology.DesertArtifact2>());
            desertFossils.Add(ModContent.GetInstance<Content.Archaeology.DesertArtifact3>());
            desertFossils.Add(ModContent.GetInstance<Content.Archaeology.DesertArtifact4>());
            desertFossils.Add(ModContent.GetInstance<Content.Archaeology.DesertArtifact5>());
            desertFossils.Add(ModContent.GetInstance<Content.Archaeology.DesertArtifact6>());
            desertFossils.Add(ModContent.GetInstance<Content.Archaeology.DesertArtifact7>());
            for (int i = 0; i < 100; i++)
            {
                if (!PlaceDesertFossil(WorldGen.UndergroundDesertLocation.X + Main.rand.Next(-500, 500), WorldGen.UndergroundDesertLocation.Y + (WorldGen.UndergroundDesertLocation.Height / 2) + Main.rand.Next(-500, 500), desertFossils))
                {
                    tries++;
                    i--;
                    if (tries > 999)
                        break;
                }
            }
        }

        private bool PlaceDesertFossil(int i, int j, List<ModTileEntity> list)
        {
            if (i < 0 || i > Main.maxTilesX || j < 0 || j > Main.maxTilesY)
                return false;

            Tile testTile = Framing.GetTileSafely(i, j);

            if (testTile.TileType != TileID.Sandstone && testTile.TileType != TileID.HardenedSand)
                return false;

            ModTileEntity toPlace = list[Main.rand.Next(list.Count)];

            toPlace.Place(i, j);
            return true;
        }
    }
}