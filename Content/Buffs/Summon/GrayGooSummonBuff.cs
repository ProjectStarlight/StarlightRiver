namespace StarlightRiver.Content.Buffs.Summon
{
	class GrayGooSummonBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + Name;

		public GrayGooSummonBuff() : base("Gray Goo", "Nanomachines, son!", false, true) { }

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<Items.Magnet.GrayGooProj>()] > 0)
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