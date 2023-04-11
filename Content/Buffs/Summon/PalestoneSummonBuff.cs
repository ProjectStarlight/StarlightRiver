namespace StarlightRiver.Content.Buffs.Summon
{
	class PalestoneSummonBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + Name;

		public PalestoneSummonBuff() : base("Pale Knight", "The Pale Knight Will Protect You!", false, true) { }

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<Items.Palestone.PaleKnight>()] > 0)
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