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
		public int rage = 0;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public BuffChalice() : base("Plexus Chalice", "Inflicting debuffs grants innoculation") { }

		public override bool Autoload(ref string name)
		{
			StatusTrackingNPC.buffCompareEffects += ChaliceEffects;
			return base.Autoload(ref name);
		}

		private void ChaliceEffects(Player player, NPC npc, int[] oldTypes, int[] newTypes, int[] oldTimes, int[] newTimes)
		{
			if (Equipped(player))
			{
				for(int k = 0; k < 5; k++)
				{
					if (oldTypes[k] != newTypes[k] || newTimes[k] > oldTimes[k])
						player.AddBuff(ModContent.BuffType<Potions.InnoculationPotionBuff>(), 120);
				}
			}
		}
	}
}
