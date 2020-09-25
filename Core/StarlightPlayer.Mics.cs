using Microsoft.Xna.Framework;
using StarlightRiver.Abilities;
using StarlightRiver.GUI;
using StarlightRiver.Items.Armor;
using StarlightRiver.NPCs.Boss.SquidBoss;
using StarlightRiver.Tiles.Permafrost;
using StarlightRiver.Tiles.Vitric.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    //I wanna put any of the accessory stats into a different file for tidiness-IDG
    public partial class StarlightPlayer : ModPlayer
    {
        public short FiftyFiveLeafClover = 0;
    }
}
