using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	class BuffChalice : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public BuffChalice() : base("Plexus Chalice", "Inflicting debuffs temporarily increases debuff resistance") { }

		public override void Load()
		{
			StatusTrackingNPC.buffCompareEffects += ChaliceEffects;			
		}

		public override void Unload()
		{
			StatusTrackingNPC.buffCompareEffects -= ChaliceEffects;
		}

		private void ChaliceEffects(Player player, NPC NPC, int[] oldTypes, int[] newTypes, int[] oldTimes, int[] newTimes)
		{
			if (Equipped(player))
			{
				for (int k = 0; k < 5; k++)
				{
					if (oldTypes[k] != newTypes[k] || newTimes[k] > oldTimes[k])
						player.AddBuff(ModContent.BuffType<PlexusChaliceBuff>(), 120);
				}
			}
		}
		class PlexusChaliceBuff : ModBuff
		{

            public override string Texture => AssetDirectory.PotionsItem + Name;

			public override void SetStaticDefaults()
			{
				DisplayName.SetDefault("Plexus Resistance");
				Description.SetDefault("+30% Inoculation");
			}

			public override void Update(Player Player, ref int buffIndex)
			{
				Player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.3f;
			}
		}
	}
}
