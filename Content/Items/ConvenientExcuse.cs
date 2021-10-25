using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items
{
	class ConvenientExcuse : ModItem
	{
		public static string sike = "Lol you thought you were gonna find something in here";

        public override string Texture => "StarlightRiver/Assets/Items/ConvenientExcuse";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("The rest of the mod's content");
			Tooltip.SetDefault("Tightly packed into a conveniently indecipherable ball");
		}

		public override void SetDefaults()
		{
			item.useStyle = 1;
			item.useTime = 20;
			item.useAnimation = 20;
			item.rare = -1;
		}

		public override bool UseItem(Player player)
		{
			Helpers.Helper.PlayPitched("Yeehaw", 1, 0);
			Main.NewText("It's far too tightly compacted to open without breaking everything inside...", new Color(255, 255, 200));
			return true;
		}
	}
}
