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

		public LostCollar() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "BloodlessAmuletGlow").Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lost Collar");
			Tooltip.SetDefault("+40% DoT Resistance\nDebuffs you inflict are inflicted on you\n+5% movement and attack speed per debuff affecting you\nLose all debuff immunities");
		}

		public override void Load()
		{
			StatusTrackingNPC.buffCompareEffects += CollarEffects;
			return base.Autoload(ref name);
		}

		private void CollarEffects(Player Player, NPC NPC, int[] oldTypes, int[] newTypes, int[] oldTimes, int[] newTimes)
		{
			if (Equipped(Player))
			{
				for (int k = 0; k < 5; k++)
				{
					if ((oldTypes[k] != newTypes[k] || newTimes[k] > oldTimes[k]) && Main.debuff[Player.buffType[k]])
						Player.AddBuff(newTypes[k], newTimes[k]);
				}
			}
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.4f;

			for (int k = 0; k < Player.buffImmune.Length; k++)
			{
				Player.buffImmune[k] = false;
			}

			for(int k = 0; k < Player.buffType.Length; k++)
				if(Player.buffType[k] > 0 && Main.debuff[Player.buffType[k]])
				{
					Player.maxRunSpeed += 0.05f;
					Player.GetModPlayer<StarlightPlayer>().ItemSpeed += 0.05f;
				}
		}
	}
}
