using StarlightRiver.Content.Items.Beach;
using StarlightRiver.Helpers;

using Terraria;
using Terraria.Utilities;
using Terraria.DataStructures;
using StarlightRiver.Content.Items.BuriedArtifacts;
using StarlightRiver.Content.Archaeology;
using StarlightRiver.Content.Archaeology.BuriedArtifacts;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using System.Linq;
using System.Collections.Generic;
using System;
using static Terraria.ModLoader.ModContent;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
    {
        private void ArtifactGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Hiding ancient secrets";

            PlaceDesertArtifacts();
            PlaceOceanArtifacts();
            PlaceLavaArtifacts();
            PlaceUndergroundArtifacts();
            PlaceJungleArtifacts();

            (ModContent.GetInstance<ArchaeologyMapLayer>()).CalculateDrawables();
        }

        private void PlaceDesertArtifacts()
        {
            var tiles = new int[]
            {
                  TileID.HardenedSand,
                  TileID.Sandstone
            };

            var range = WorldGen.UndergroundDesertLocation;
            range.Inflate(500, 500);

            int amount = Main.maxTilesX / 17;

            PlaceArtifactPool<DesertArtifact>(range, tiles, amount, 999);
        }

        private void PlaceJungleArtifacts()
        {
            var tiles = new int[]
            {
                  TileID.JungleGrass,
                  TileID.Mud
            };
            Rectangle range = new Rectangle(100, 100, Main.maxTilesX - 200, Main.maxTilesY - 400);

            int amount = Main.maxTilesX / 12;

            PlaceArtifactPool<JungleArtifact>(range, tiles, amount, 4999);
        }

        private void PlaceOceanArtifacts()
        {
            var tiles = new int[]
            {
                  TileID.Sand,
                  TileID.ShellPile
            };

            Rectangle leftRange = new Rectangle(0, 0, 300, (int)Main.rockLayer);
            Rectangle rightRange = new Rectangle(0, 0, 300, (int)Main.rockLayer);

            PlaceArtifactPool<OceanArtifact>(leftRange, tiles, 5, 999);
            PlaceArtifactPool<OceanArtifact>(rightRange, tiles, 5, 999);
        }

        private void PlaceLavaArtifacts()
        {
            var tiles = new int[]
            {
                  TileID.Granite,
                  TileID.Marble,
                  TileID.Stone
            };

            Rectangle range = new Rectangle(0, Main.maxTilesY - 500, Main.maxTilesX, 300);

            int amount = Main.maxTilesX / 800;

            PlaceArtifactPool<LavaArtifact>(range, tiles, amount, 999);
        }

        private void PlaceUndergroundArtifacts()
        {
            var tiles = new int[]
            {
                  TileID.Granite,
                  TileID.Marble,
                  TileID.Stone
            };

            Rectangle range = new Rectangle(100, (int)Main.rockLayer, Main.maxTilesX - 200, (Main.maxTilesY - 400) - (int)Main.rockLayer);

            int amount = Main.maxTilesX / 7;

            PlaceArtifactPool<UndergroundArtifact>(range, tiles, amount, 2999);
        }

        private void PlaceArtifactPool<T>(Rectangle range, int[] validTiles, int toPlace, int maxTries) where T : Artifact
        {
            WeightedRandom<Artifact> pool = new(Main.rand);
            foreach (T artifact in StarlightRiver.Instance.GetContent<T>())
                pool.Add(artifact, artifact.SpawnChance);

            int tries = 0;
            for (int i = 0; i < toPlace; i++)
            {
                Artifact artifact = pool.Get();
                if (!PlaceArtifact(range, artifact, validTiles))
                {
                    i--;
                    tries++;

                    if (tries > maxTries)
                        break;
                }
            }
        }

        private bool PlaceArtifact(Rectangle range, Artifact artifact, int[] validTiles)
        {
            int i = range.Left + Main.rand.Next(range.Width);
            int j = range.Top + Main.rand.Next(range.Height);
            if (!WorldGen.InWorld(i, j) || !artifact.CanGenerate(i, j)) return false;

            for (int x = 0; x < artifact.Size.X / 16; x++)
                for (int y = 0; y < artifact.Size.Y / 16; y++)
                {
                    Tile testTile = Main.tile[x + i, y + j];
                    if (!testTile.HasTile || !validTiles.Contains(testTile.TileType))
                        return false;
                }

            artifact.Place(i, j);
            return true;
        }
    }
}