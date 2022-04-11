using Microsoft.Xna.Framework;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Bestiary
{
    public static class SLRSpawnConditions
    {
        public static ModBiomeSpawnCondition VitricDesert = new ModBiomeSpawnCondition("Vitric Desert", AssetDirectory.Biomes + "VitricDesertIcon", AssetDirectory.MapBackgrounds + "GlassMap", Color.Teal);
    }
}
