using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
    public class Datsuzei : ModItem
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public override void SetStaticDefaults()
		{
            DisplayName.SetDefault("Datsuzei");
            Tooltip.SetDefault("Unleash the fucking moon");
		}

		public override void SetDefaults()
        {
            item.damage = 110;
            item.width = 16;
            item.height = 16;
            item.useStyle = ItemUseStyleID.Stabbing;
            item.useTime = 12;
            item.useAnimation = 12;
            item.crit = 10;
        }
    }
}
