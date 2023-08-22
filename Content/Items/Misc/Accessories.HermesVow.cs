﻿using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class HermesVow : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public HermesVow() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "HermesVow").Value) { }

		public override void Load()
		{
			StarlightPlayer.PostUpdateRunSpeedsEvent += AddRunSpeeds;
			StarlightItem.CanEquipAccessoryEvent += PreventWingUse;
		}

		public override void Unload()
		{
			StarlightPlayer.PostUpdateRunSpeedsEvent -= AddRunSpeeds;
			StarlightItem.CanEquipAccessoryEvent -= PreventWingUse;
		}

		private bool PreventWingUse(Item item, Player player, int slot, bool modded)
		{
			if (Equipped(player))
			{
				if (item.wingSlot > 0)
					return false;
			}

			return true;
		}

		private void AddRunSpeeds(Player player)
		{
			if (Equipped(player))
			{
				player.accRunSpeed += 3.5f;
				player.moveSpeed += 0.3f;
				player.maxRunSpeed += 0.75f;
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hermes' Vow");
			Tooltip.SetDefault("Cursed\nMassively increased acceleration and movement speed\nIncreased jump height and max movement speed\nWorks with boots\nYou are unable to use wings");
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(silver: 75);
		}

		public override void OnEquip(Player Player, Item item)
		{
			for (int i = 3; i < 10; i++)
			{
				if (Player.IsItemSlotUnlockedAndUsable(i))
				{
					Item wingItem = Player.armor[i];
					if (wingItem.wingSlot > 0)
					{
						Player.QuickSpawnItem(Player.GetSource_Accessory(Item), wingItem);
						wingItem.wingSlot = 0;
						wingItem.TurnToAir();
					}
				}
			}
		}

		public override void SafeUpdateEquip(Player player)
		{
			player.jumpSpeedBoost += 2f;
			player.extraFall += 10;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HermesBoots);
			recipe.AddIngredient(ItemID.FrogLeg);
			recipe.AddIngredient(ItemID.TungstenBar, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HermesBoots);
			recipe.AddIngredient(ItemID.FrogLeg);
			recipe.AddIngredient(ItemID.SilverBar, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HermesBoots);
			recipe.AddIngredient(ItemID.CreativeWings);
			recipe.AddIngredient(ItemID.TungstenBar, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HermesBoots);
			recipe.AddIngredient(ItemID.CreativeWings);
			recipe.AddIngredient(ItemID.SilverBar, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}