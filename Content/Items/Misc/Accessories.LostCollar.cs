using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
			Tooltip.SetDefault("+40% Inoculation\nDebuffs you inflict are inflicted on you\n+5% movement and attack speed per debuff affecting you\nCursed : Lose all debuff immunities");
		}

		public override void Load() //TODO: Make cursedaccessory.Load not hide this
		{
			StatusTrackingNPC.buffCompareEffects += CollarEffects;			
		}

		public override void Unload()
		{
			StatusTrackingNPC.buffCompareEffects -= CollarEffects;
		}

		private void CollarEffects(Player player, NPC NPC, int[] oldTypes, int[] newTypes, int[] oldTimes, int[] newTimes)
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
