using StarlightRiver.Content.Items.Breacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Buffs.Summon
{
	class MediRemoteSummonBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + "HauntedDaggerBuff";

		public MediRemoteSummonBuff() : base("M.E.D.I Remote", "Always good to see a healing drone", false, true) { }

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<MediRemoteProjectile>()] > 0)
			{
				Player.buffTime[buffIndex] = 18000;
			}
			else
			{
				Player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
