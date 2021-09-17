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

		public override bool Autoload(ref string name)
		{
			StatusTrackingNPC.buffCompareEffects += ChaliceEffects;
			return base.Autoload(ref name);
		}

		private void ChaliceEffects(Player player, NPC npc, int[] oldTypes, int[] newTypes, int[] oldTimes, int[] newTimes)
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
			public override bool Autoload(ref string name, ref string texture)
			{
				texture = AssetDirectory.PotionsItem + name;
				return base.Autoload(ref name, ref texture);
			}

			public override void SetDefaults()
			{
				DisplayName.SetDefault("Plexus Resistance");
				Description.SetDefault("+30% to DoT Resistance");
			}

			public override void Update(Player player, ref int buffIndex)
			{
				player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.3f;
			}
		}
	}
}
