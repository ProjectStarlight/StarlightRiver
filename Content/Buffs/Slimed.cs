namespace StarlightRiver.Content.Buffs
{
	public class Slimed : SmartBuff
	{
		public Slimed() : base("Slimed", "eww", true) { }

		public override string Texture => AssetDirectory.Buffs + "Slimed";

		public override void Update(Player Player, ref int buffIndex)
		{
			Player.lifeRegen -= 5;
			Player.slow = true;
		}

		public override void Update(NPC NPC, ref int buffIndex)
		{
			NPC.lifeRegen -= 5;//life per second
			NPC.velocity *= NPC.noGravity ? 0.96f : 0.92f;//4% & 8%
		}
	}
}
