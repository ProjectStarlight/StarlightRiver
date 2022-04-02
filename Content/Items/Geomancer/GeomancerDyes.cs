using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Dyes;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Geomancer
{
    public class RainbowCycleDye : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;
        public override void SetDefaults()
        {
            /*				
			this.name = "Gel Dye";
			this.width = 20;
			this.height = 20;
			this.maxStack = 99;
			this.value = Item.sellPrice(0, 1, 50, 0);
			this.rare = 3;
			*/
            // item.dye is already assigned to this item prior to SetDefaults because of the GameShaders.Armor.BindShader code in ExampleMod.Load. 
            // This code here remembers item.dye so that information isn't lost during CloneDefaults. The above code is the data being cloned by CloneDefaults, for informational purposes.
            byte dye = item.dye;
            item.CloneDefaults(ItemID.GelDye);
            item.dye = dye;
        }
    }

    public class RainbowCycleDye2 : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + "RainbowCycleDye";
        public override void SetDefaults()
        {
            /*				
			this.name = "Gel Dye";
			this.width = 20;
			this.height = 20;
			this.maxStack = 99;
			this.value = Item.sellPrice(0, 1, 50, 0);
			this.rare = 3;
			*/
            // item.dye is already assigned to this item prior to SetDefaults because of the GameShaders.Armor.BindShader code in ExampleMod.Load. 
            // This code here remembers item.dye so that information isn't lost during CloneDefaults. The above code is the data being cloned by CloneDefaults, for informational purposes.
            byte dye = item.dye;
            item.CloneDefaults(ItemID.GelDye);
            item.dye = dye;
        }
    }
}