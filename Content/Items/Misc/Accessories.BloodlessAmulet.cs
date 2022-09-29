using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	class BloodlessAmulet : CursedAccessory
	{
		public int rage = 0;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public BloodlessAmulet() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "BloodlessAmuletGlow").Value) { }

		public override void Load()
		{
			On.Terraria.Player.HealEffect += GrantRage;
		}

		public override void Unload()
		{
			On.Terraria.Player.HealEffect -= GrantRage;
		}

		private void GrantRage(On.Terraria.Player.orig_HealEffect orig, Player self, int healAmount, bool broadcast)
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
			else orig(self, healAmount, broadcast);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Amulet of the Bloodless Warrior");
			Tooltip.SetDefault("+100 Maximum Barrier" +
				"\nUnaffected by damage over time" +
				"\nBarrier absorbs ALL damage" +
				"\nYou can survive without life"+
				"\nCursed : You cannot have life" +
				"\n Slightly reduced barrier recharge" +
				"\n Healing grants a decaying damage boost instead of life" +
				"\n'Leave your flesh behind, for your rage is all you need'");
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetDamage(DamageClass.Generic) += rage / 2000f;

			if(rage > 0)
				rage--;

			if (rage > 800)
				rage = 800;

			Player.GetModPlayer<BarrierPlayer>().MaxBarrier += 100;
			Player.GetModPlayer<BarrierPlayer>().BarrierDamageReduction = 1;
			Player.GetModPlayer<BarrierPlayer>().PlayerCanLiveWithOnlyBarrier = true;
			Player.GetModPlayer<BarrierPlayer>().RechargeRate -= 10;
			Player.statLife = 0;
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
