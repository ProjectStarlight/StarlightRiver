using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.CustomHooks;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Dungeon
{
	public class AquaSapphire : ModItem
	{
		public override string Texture => AssetDirectory.DungeonItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aqua sapphire");
			Tooltip.SetDefault("Barrier negates 5% more damage \n+10 Barrier");

		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 28;
			item.rare = 3;
			item.value = Item.buyPrice(0, 5, 0, 0);
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<ShieldPlayer>().MaxShield += 10;
			player.GetModPlayer<ShieldPlayer>().ShieldResistance += 0.05f;

		}
	}
}