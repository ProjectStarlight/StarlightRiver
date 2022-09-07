using StarlightRiver.Content.Items.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.TownModifications
{
	class MechanicShopAdditions : GlobalNPC
	{
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Mechanic)
            {
                shop.item[nextSlot].SetDefaults(ModContent.ItemType<Sorcerwrench>()); nextSlot++;
            }
        }
    }
}
