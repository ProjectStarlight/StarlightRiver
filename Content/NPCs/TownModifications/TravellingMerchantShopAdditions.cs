using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Content.Items.Vanity;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using System.IO;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace StarlightRiver.Content.NPCs.TownModifications
{
    class TravellingMerchantShopAdditions : GlobalNPC
    {
        public override void SetupTravelShop(int[] shop, ref int nextSlot)
        {
            if (Main.rand.NextBool(15))
            {
                shop[nextSlot] = ModContent.ItemType<WardenHat>(); nextSlot++;
                shop[nextSlot] = ModContent.ItemType<WardenRobe>(); nextSlot++;
            }
        }
    }
}