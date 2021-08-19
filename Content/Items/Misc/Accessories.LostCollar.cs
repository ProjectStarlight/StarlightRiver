using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	class LostCollar : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public LostCollar() : base(ModContent.GetTexture(AssetDirectory.MiscItem + "BloodlessAmuletGlow")) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lost Collar");
			Tooltip.SetDefault("+40% DoT Resistance\nDebuffs you inflict are inflicted on you\n+5% movement and attack speed per debuff affecting you\nLose all debuff immunities");
		}

		public override bool Autoload(ref string name)
		{
			StatusTrackingNPC.buffCompareEffects += CollarEffects;
			return base.Autoload(ref name);
		}

		private void CollarEffects(Player player, NPC npc, int[] oldTypes, int[] newTypes, int[] oldTimes, int[] newTimes)
		{
			if (Equipped(player))
			{
				for (int k = 0; k < 5; k++)
				{
					if ((oldTypes[k] != newTypes[k] || newTimes[k] > oldTimes[k]) && Main.debuff[player.buffType[k]])
						player.AddBuff(newTypes[k], newTimes[k]);
				}
			}
		}

		public override void SafeUpdateEquip(Player player)
		{
			player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.4f;

			for (int k = 0; k < player.buffImmune.Length; k++)
			{
				player.buffImmune[k] = false;
			}

			for(int k = 0; k < player.buffType.Length; k++)
				if(player.buffType[k] > 0 && Main.debuff[player.buffType[k]])
				{
					player.maxRunSpeed += 0.05f;
					player.GetModPlayer<StarlightPlayer>().itemSpeed += 0.05f;
				}
		}
	}
}
