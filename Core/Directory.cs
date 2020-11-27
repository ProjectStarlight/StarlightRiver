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
        public static string Assets =               "StarlightRiver/Assets/";

        public static string Invisible =            Assets + "Invisible";
        public static string Debug =                Assets + "MarioCumming";

        public static string Dust =                 Assets + "Dusts";

        public static string OvergrowTileDir =      Assets + "Tiles/Overgrow";
        public static string OvergrowItemDir =      Assets + "Items/Overgrow";
        public static string OvergrowBossDir =      Assets + "Bosses/OvergrowBoss";

        public static string VitricTileDir =        Assets + "Tiles/Vitric";
        public static string VitricItemDir =        Assets + "Items/Vitric";
        public static string GlassBossDir =         Assets + "Bosses/GlassBoss";
        public static string GlassMinibossDir =     Assets + "Bosses/GlassMiniboss";

        public static string PermafrostTileDir =    Assets + "Tiles/Permafrost";
        public static string PermafrostItemDir =    Assets + "Items/Permafrost";
        public static string SquidBossDir =         Assets + "Bosses/SquidBoss";

        public static string SlimeItemDir =         Assets + "Items/Slime";
    }
}