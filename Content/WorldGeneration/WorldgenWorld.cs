using StarlightRiver.Structures;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.World.Generation;

using StarlightRiver.Core;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld : ModWorld
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
            int SurfaceIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Sunflowers"));
            int HellIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Lakes"));
            int DesertIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
            int TrapsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Traps"));

            if (ShiniesIndex != -1)
            {
                tasks.Insert(DesertIndex + 1, new PassLegacy("Starlight River Temples", UndergroundTempleGen));
                tasks.Insert(DesertIndex + 2, new PassLegacy("Starlight River Permafrost", PermafrostGen));
                tasks.Insert(DesertIndex + 3, new PassLegacy("Starlight River Ash Hell", AshHellGen));
                tasks.Insert(DesertIndex + 4, new PassLegacy("Starlight River Vitric Desert", VitricGen));
                tasks.Insert(DesertIndex + 5, new PassLegacy("Starlight River Overgrowth", OvergrowGen));
                tasks.Insert(DesertIndex + 6, new PassLegacy("Starlight River Codex", GenHelper.BookAltarGen));

                //tasks.Insert(DesertIndex + 7, new PassLegacy("Starlight River Vines", VineGen));

                tasks.Insert(ShiniesIndex + 1, new PassLegacy("Starlight River Ores", EbonyGen));

                tasks.Insert(SurfaceIndex + 1, new PassLegacy("Starlight River Ruins", GenHelper.RuinsGen));
                tasks.Insert(SurfaceIndex + 2, new PassLegacy("Starlight River Ivy", ForestHerbGen));
            }
        }

        public override void PostWorldGen()
        {
            if (WorldGen.genRand.NextBool())
                Flag(WorldFlags.AluminumMeteors);

            ModContent.GetInstance<StarlightRiver>().PopulateChests();
        }
    }
}