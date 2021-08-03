using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	class BloodlessAmulet : CursedAccessory
	{
		public int rage = 0;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public BloodlessAmulet() : base(ModContent.GetTexture(AssetDirectory.MiscItem + "BloodlessAmuletGlow")) { }

		public override bool Autoload(ref string name)
		{
			On.Terraria.Player.HealEffect += GrantRage;
			return true;
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
				"\nYou cannot have life" +
				"\nSlightly reduced barrier recharge" +
				"\nHealing grants a decaying damage boost instead of life" +
				"\n'Leave your flesh behind, for your rage is all you need'");
		}

		public override void SafeUpdateEquip(Player player)
		{
			player.allDamageMult += rage / 2000f;

			if(rage > 0)
				rage--;

			if (rage > 800)
				rage = 800;

			player.GetModPlayer<ShieldPlayer>().MaxShield += 100;
			player.GetModPlayer<ShieldPlayer>().ShieldResistance = 1;
			player.GetModPlayer<ShieldPlayer>().LiveOnOnlyShield = true;
			player.GetModPlayer<ShieldPlayer>().RechargeRate -= 10;
			player.statLife = 0;
			player.lifeRegen = 0;
			player.lifeRegenCount = 0;
		}
	}
}
