using StarlightRiver.Core.Systems.ChestLootSystem;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
    public partial class StarlightWorld : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
            int SurfaceIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Sunflowers"));
            int HellIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Lakes"));
            int DesertIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
            int TrapsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Traps"));
            int EndIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Tile Cleanup"));

            if (ShiniesIndex != -1)
            {
                tasks.Insert(DesertIndex + 1, new PassLegacy("Starlight River Temples", UndergroundTempleGen));
                tasks.Insert(DesertIndex + 2, new PassLegacy("Starlight River Permafrost", PermafrostGen));
                tasks.Insert(DesertIndex + 4, new PassLegacy("Starlight River Vitric Desert", VitricGen));
                tasks.Insert(DesertIndex + 6, new PassLegacy("Starlight River Codex", BookAltarGen));

                tasks.Insert(SurfaceIndex + 1, new PassLegacy("Starlight River Ivy", ForestHerbGen));
                tasks.Insert(SurfaceIndex + 1, new PassLegacy("Starlight River Surface Items", SurfaceItemPass));
                tasks.Insert(EndIndex + 1, new PassLegacy("Starlight River Big Trees", BigTreeGen));
            }
        }

        public override void PostWorldGen()
        {
            if (WorldGen.genRand.NextBool())
                Flag(WorldFlags.AluminumMeteors);

            ChestLootSystem.Instance.PopulateAllChests();
        }
    }
}