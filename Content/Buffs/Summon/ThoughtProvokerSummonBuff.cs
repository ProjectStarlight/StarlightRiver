using StarlightRiver.Content.Items.Crimson;
using StarlightRiver.Content.Items.Haunted;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Buffs.Summon
{
	class ThoughtProvokerSummonBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + Name;

		public ThoughtProvokerSummonBuff() : base("Thinky Juniors", "1000 IQ", false, true) { }

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<ThoughtProvokerProjectile>()] > 0)
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
