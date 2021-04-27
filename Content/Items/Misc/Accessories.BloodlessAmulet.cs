using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
				int rageToAdd = healAmount / 2;

				if (rageToAdd > 0)
				{
					rage += rageToAdd;
					CombatText.NewText(self.Hitbox, Color.Orange, rageToAdd);
				}
			}
			else orig(self, healAmount, broadcast);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Amulet of the Bloodless Warrior");
			Tooltip.SetDefault("+100 Maximum Barrier\nBarrier absorbs ALL damage\nYou cannot have life\nSlightly reduced barrier recharge\nHealing grant a decaying rage effect instead of healing\n'Leave your flesh behind, for your rage is all you need to live'");
		}

		public override void SafeUpdateEquip(Player player)
		{
			player.meleeDamageMult += rage / 100f;

			if(rage > 0)
				rage--;

			player.GetModPlayer<ShieldPlayer>().MaxShield += 100;
			player.GetModPlayer<ShieldPlayer>().ShieldResistance = 1;
			player.statLifeMax2 = 0;
		}
	}
}
