using StarlightRiver.Content.Items.Haunted;

namespace StarlightRiver.Content.Buffs.Summon
{
	class HauntedDaggerSummonBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + "HauntedDaggerBuff";

		public HauntedDaggerSummonBuff() : base("Haunted Daggers", "Death by a hundred cuts", false, true) { }

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<HauntedDaggerProjectile>()] > 0)
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