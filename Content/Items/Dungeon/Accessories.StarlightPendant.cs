﻿using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Systems.BarrierSystem;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Dungeon
{
	public class StarlightPendant : ModItem
	{
		private int currentMana = -1;

		private readonly List<int> manaConsumed = new();

		public override string Texture => AssetDirectory.DungeonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starlight Pendant");
			Tooltip.SetDefault("Boosts mana regen based on your current barrier\n" +
				"Consuming mana boosts barrier regen\n" +
				"-10 barrier");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(0, 5, 0, 0);
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier = (int)MathHelper.Max(0, Player.GetModPlayer<BarrierPlayer>().maxBarrier - 10);

			if (currentMana != Player.statMana || Player.itemTime >= 2)
			{
				if (currentMana > Player.statMana)
					manaConsumed.Add(currentMana - Player.statMana);
				else if (Player.itemTime >= 2)
					manaConsumed.Add(0);
				currentMana = Player.statMana;
			}

			while (manaConsumed.Count > 300)
				manaConsumed.RemoveAt(0);

			int manaConsumedTotal = 0;
			foreach (int i in manaConsumed)
				manaConsumedTotal += i;
			float manaBonus = 1 + Player.GetModPlayer<BarrierPlayer>().barrier * 0.01f;

			Player.manaRegen = (int)(Player.manaRegen * manaBonus);

			Player.GetModPlayer<BarrierPlayer>().rechargeRate = (int)(Player.GetModPlayer<BarrierPlayer>().rechargeRate * 1 + manaConsumedTotal * 0.025f);
			Player.GetModPlayer<BarrierPlayer>().rechargeDelay = (int)(Player.GetModPlayer<BarrierPlayer>().rechargeDelay / (1 + manaConsumedTotal * 0.025f));
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<SandstoneChunk>(), 4);
			recipe.AddIngredient(ModContent.ItemType<AquaSapphire>(), 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}