namespace StarlightRiver.Content.Buffs.Summon
{
	class StarwoodSummonBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + Name;

		public StarwoodSummonBuff() : base("Starwood Minion", "The sentient stars shall protect you!", false, true) { }

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<Items.Starwood.StarwoodScepterSummonSplit>()] > 0)
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