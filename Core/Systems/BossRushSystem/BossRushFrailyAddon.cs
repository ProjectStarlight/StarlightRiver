using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class BossRushFrailtyAddon : ModSystem
	{
		public static bool active;
	}

	internal class BossRushFrailtyPlayer : ModPlayer
	{
		public override void OnHurt(Player.HurtInfo info)
		{
			if (BossRushSystem.isBossRush && BossRushFrailtyAddon.active)
				Player.KillMe(PlayerDeathReason.ByCustomReason(Player.name + " shattered..."), double.PositiveInfinity, 0);
		}
	}
}
