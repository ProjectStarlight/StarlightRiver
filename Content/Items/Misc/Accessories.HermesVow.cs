using StarlightRiver.Content.Items.BaseTypes;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class HermesVow : CursedAccessory
	{
		public Vector2 lastFXSpeed;

		public override string Texture => AssetDirectory.MiscItem + Name;

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
				player.moveSpeed += 1.6f;
				player.maxRunSpeed += 4f;
				player.runAcceleration *= 12f;
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

			if (player.velocity.Length() > 5 && Main.rand.NextBool(4))
			{
				Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(24, 32), ModContent.DustType<Dusts.PixelatedEmber>(), player.velocity * Main.rand.NextFloat(-1f, -0.2f), 0, new Color(200, 50, 255, 0), 0.08f);
			}

			if ((lastFXSpeed.X == 0 && player.velocity.X != 0) || (lastFXSpeed.X > 0 != player.velocity.X > 0 && player.velocity.X != 0))
			{
				for (int k = 0; k < 10; k++)
				{
					Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(24, 32), ModContent.DustType<Dusts.PixelatedImpactLineDust>(), new Vector2(player.velocity.X * -30, 0), 0, new Color(200, 50, 255, 0), 0.1f);
				}

				Helpers.SoundHelper.PlayPitched("Magic/Shadow2", 0.3f, 1f + Main.rand.NextFloat(0.1f), player.Center);
			}

			lastFXSpeed = player.velocity;
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