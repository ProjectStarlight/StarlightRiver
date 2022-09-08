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
            
            WeightedRandom<Artifact> desertPool = GetArtifactPool<DesertArtifact>(StarlightRiver.Instance);
            int[] desertTiles = new int[] { TileID.HardenedSand, TileID.Sandstone };
            Rectangle desertRange = new Rectangle(WorldGen.UndergroundDesertLocation.X - 500, WorldGen.UndergroundDesertLocation.Y + (WorldGen.UndergroundDesertLocation.Height / 2) - 500, 1000, 1000);
            PlaceArtifactPool(desertRange, desertPool, desertTiles, 50, 999);

            WeightedRandom<Artifact> oceanPool = GetArtifactPool<OceanArtifact>(StarlightRiver.Instance);
            int[] oceanTiles = new int[] { TileID.Sand, TileID.ShellPile };
            Rectangle leftOceanRange = new Rectangle(0, 0, 300, (int)Main.rockLayer);
            Rectangle rightOceanRange = new Rectangle(Main.maxTilesX - 300, 0, 300, (int)Main.rockLayer);
            PlaceArtifactPool(leftOceanRange, oceanPool, oceanTiles, 10, 999);
        }

        private void PlaceArtifactPool(Rectangle range, WeightedRandom<Artifact> pool, int[] validTiles, int toPlace, int maxTries)
        {
            int tries = 0;
            for (int i = 0; i < toPlace; i++)
            {
                if (!PlaceArtifact(range, pool, validTiles))
                {
                    i--;
                    tries++;

                    if (tries > maxTries)
                        break;
                }
            }
        }

        private bool PlaceArtifact(Rectangle range, WeightedRandom<Artifact> pool, int[] validTiles)
        {
            int i = range.Left + Main.rand.Next(range.Width);
            int j = range.Top + Main.rand.Next(range.Height);
            if (!WorldGen.InWorld(i, j)) return false;

            Tile testTile = Framing.GetTileSafely(i, j);
            if (!testTile.HasTile || !validTiles.Contains(testTile.TileType)) return false;

            pool.Get().Place(i, j);
            return true;
        }

        private WeightedRandom<Artifact> GetArtifactPool<T>(Mod mod) where T : Artifact 
        {
            WeightedRandom<Artifact> pool = new(Main.rand);
            foreach (T artifact in mod.GetContent<T>())
                pool.Add(artifact, artifact.SpawnChance);
            return pool;
        }
    }
}