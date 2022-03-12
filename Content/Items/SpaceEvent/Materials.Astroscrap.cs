using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.SpaceEvent
{
    class Astroscrap : ModItem
    {
        public override string Texture => "StarlightRiver/Assets/Items/SpaceEvent/Astroscrap";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astroscrap");
            Tooltip.SetDefault("‘Alloy salvaged from the wreckage of your world’s invaders’");
        }

        public override void SetDefaults()
        {
            item.rare = ItemRarityID.Blue;
            item.value = Item.sellPrice(0,0,5,0);
            item.width = 32;
            item.height = 32;
            item.maxStack = 999;
        }
    }
}
