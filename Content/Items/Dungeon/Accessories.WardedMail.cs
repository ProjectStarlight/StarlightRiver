﻿using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.BarrierSystem;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Dungeon
{
	public class WardedMail : SmartAccessory
	{
		public override string Texture => AssetDirectory.DungeonItem + Name;

		public WardedMail() : base("Warded Mail", "{{Barrier}} damage is applied to attackers as thorns\n+40 maximum {{barrier}}\n+{{Barrier}} negates 10% more damage") { }

		public override List<int> ChildTypes => new()
		{
			ModContent.ItemType<SpikedMail>(),
		};

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Green;
		}

		public override void SafeUpdateEquip(Player player)
		{
			player.GetModPlayer<BarrierPlayer>().maxBarrier += 20;
			player.GetModPlayer<BarrierPlayer>().barrierDamageReduction += 0.1f;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<SpikedMail>(), 1);
			recipe.AddIngredient(ModContent.ItemType<AquaSapphire>(), 1);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();

		}
	}
}