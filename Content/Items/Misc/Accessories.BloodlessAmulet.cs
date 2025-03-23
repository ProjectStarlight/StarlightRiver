﻿using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Core.Systems.BarrierSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	class BloodlessAmulet : CursedAccessory
	{
		public int rage = 0;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void Load()
		{
			On_Player.HealEffect += GrantRage;
		}

		public override void Unload()
		{
			On_Player.HealEffect -= GrantRage;
		}

		private void GrantRage(On_Player.orig_HealEffect orig, Player self, int healAmount, bool broadcast)
		{
			if (Equipped(self))
			{
				int rageToAdd = healAmount * 10;

				if (rageToAdd > 0)
				{
					(GetEquippedInstance(self) as BloodlessAmulet).rage += rageToAdd;
					CombatText.NewText(self.Hitbox, Color.Orange, rageToAdd / 10);
				}
			}
			else
			{
				orig(self, healAmount, broadcast);
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Amulet of the Bloodless Warrior");
			Tooltip.SetDefault("+100 maximum {{Barrier}}" +
				"\n{{Barrier}} absorbs ALL damage, but recharges slower" +
				"\nYou are unaffected by damage over time" +
				"\nYou have 0 life. Healing grants a decaying damage boost instead of life" +
				"\n'Leave your flesh behind, for your rage is all you need'");
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 7, silver: 50);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetDamage(DamageClass.Generic) += rage / 2000f;

			if (rage > 0)
				rage--;

			if (rage > 800)
				rage = 800;

			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 100;
			Player.GetModPlayer<BarrierPlayer>().barrierDamageReduction = 1;
			Player.GetModPlayer<BarrierPlayer>().playerCanLiveWithOnlyBarrier = true;
			Player.GetModPlayer<BarrierPlayer>().rechargeRate -= 2;

			// 0 hp is okay for singleplayer but would require some unholy IL edits for multiplayer. setting it to 1 instead seems fine
			if (Main.netMode == NetmodeID.SinglePlayer)
				Player.statLife = 0;
			else
				Player.statLife = 1;

			Player.lifeRegen = 0;
			Player.lifeRegenCount = 0;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<BloodAmulet>());
			recipe.AddIngredient(ModContent.ItemType<Dungeon.AquaSapphire>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}