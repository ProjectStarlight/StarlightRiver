using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Alchemy
{
    public class MixingStick : ModItem
    {
		public override string Texture => AssetDirectory.Alchemy + Name;

		public override bool AltFunctionUse(Player player) => true;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Mixing Stick\nUse this to finalize alchemy recipes to craft them.");
		}

		public override void SetDefaults()
		{
			item.width = 26;
			item.height = 22;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 15;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.consumable = true;
			item.value = 500;
		}
	}
}
