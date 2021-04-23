using StarlightRiver.Content.Tiles;
using StarlightRiver.Content.Tiles.Balanced;
using StarlightRiver.Content.Tiles.Interactive;
using StarlightRiver.Content.Tiles.Purified;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities.Purify.TransformationHelpers
{
    class TransformationLoader : ILoadable
    {
        public static List<PurifyTransformation> transformations;

        public float Priority => 1;

        public void Load()
        {
            transformations = new List<PurifyTransformation>();

            transformations.Add(new PurifyTransformation(
                new List<int>()
                {
                    TileID.Stone,
                    TileID.Ebonstone,
                    TileID.Crimstone,
                    TileID.Pearlstone
                },
                TileType<StonePure>()
                ));

            transformations.Add(new PurifyTransformation(
                new List<int>()
                {
                    TileID.Grass,
                    TileID.CorruptGrass,
                    TileID.FleshGrass,
                    TileID.HallowedGrass
                },
                TileType<GrassPure>()
                ));

            transformations.Add(new PurifyTransformation(
                new List<int>()
                {
                    TileID.Sand,
                    TileID.Ebonsand,
                    TileID.Crimsand,
                    TileID.Pearlsand
                },
                TileType<SandPure>()
                ));

            transformations.Add(new PurifyTransformation(
                TileType<OreEbony>(),
                TileType<OreIvory>()
                ));

            transformations.Add(new PurifyTransformation(
                TileType<VoidDoorOn>(),
                TileType<VoidDoorOff>()
                ));
        }

        public void Unload()
        {
            transformations = null;
        }
    }
}
