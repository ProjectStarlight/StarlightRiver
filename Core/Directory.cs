using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;


namespace StarlightRiver.Core
{
    public static class Directory
    {
        public const string Assets =               "StarlightRiver/Assets/";

        public const string Invisible =            Assets + "Invisible";
        public const string Debug =                Assets + "MarioCumming";

        public const string Dust =                 Assets + "Dusts/";

        public const string AluminumItemDir =      Assets + "Items/AstralMeteor/";
        public const string CaveTempleItemDir = Assets + "Items/UndergroundTemple/";

        public const string OvergrowTileDir =      Assets + "Tiles/Overgrow/";
        public const string OvergrowItemDir =      Assets + "Items/Overgrow/";
        public const string OvergrowBossDir =      Assets + "Bosses/OvergrowBoss/";

        public const string VitricTileDir =        Assets + "Tiles/Vitric/";
        public const string VitricItemDir =        Assets + "Items/Vitric/";
        public const string GlassBossDir =         Assets + "Bosses/GlassBoss/";
        public const string GlassMinibossDir =     Assets + "Bosses/GlassMiniboss/";

        public const string PermafrostTileDir =    Assets + "Tiles/Permafrost/";
        public const string PermafrostItemDir =    Assets + "Items/Permafrost/";
        public const string SquidBossDir =         Assets + "Bosses/SquidBoss/";

        public const string SlimeItemDir =         Assets + "Items/Slime/";
    }
}