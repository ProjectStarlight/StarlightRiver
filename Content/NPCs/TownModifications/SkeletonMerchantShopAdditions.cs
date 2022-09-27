using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Content.Items.Misc;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using System.IO;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace StarlightRiver.Content.NPCs.TownModifications
{
    class SkeletonMerchantShopAdditions : GlobalNPC
    {
		public override void SetupShop(int type, Chest shop, ref int nextSlot)
		{
			if (type == NPCID.SkeletonMerchant && Main.moonPhase > 2 && Main.moonPhase < 5)
			{
				shop.item[nextSlot].SetDefaults(ModContent.ItemType<BizarrePotion>()); nextSlot++;
			}
		}
	}
}