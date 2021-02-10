using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Content.GUI;
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


namespace StarlightRiver.Helpers
{
    public static partial class Helper
    {
        public static bool IsValidDebuff(Player player, int buffindex)
        {
            int bufftype = player.buffType[buffindex];
            bool vitalbuff = (bufftype == BuffID.PotionSickness || bufftype == BuffID.ManaSickness || bufftype == BuffID.ChaosState);
            return player.buffTime[buffindex] > 2 && Main.debuff[bufftype] && !Main.buffNoTimeDisplay[bufftype] && !Main.vanityPet[bufftype] && !vitalbuff;
        }
    }
}

