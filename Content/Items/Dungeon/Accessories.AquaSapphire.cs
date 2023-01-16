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
			Item.width = 30;
			Item.height = 28;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(0, 5, 0, 0);
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			Player.GetModPlayer<BarrierPlayer>().MaxBarrier += 10;
			Player.GetModPlayer<BarrierPlayer>().BarrierDamageReduction += 0.05f;
		}
	}
}