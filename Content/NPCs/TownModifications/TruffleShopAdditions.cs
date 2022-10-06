using StarlightRiver.Content.Items.Food;
using StarlightRiver.Content.Items.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Tiles.Mushroom;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class TruffleShopAdditions : GlobalNPC
	{
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Truffle)
            {
                shop.item[nextSlot].SetDefaults(ModContent.ItemType<JellyShroomItem>()); nextSlot++;
            }
        }
    }
}
