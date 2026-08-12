namespace StarlightRiver.Content.Buffs
{
	public class CoughDropsBuff : SmartBuff
	{
		public CoughDropsBuff() : base("Cough Drops", "Your speed and damage are increased", false) { }

		public override string Texture => AssetDirectory.Buffs + "CoughDropsBuff";

		public override void Update(Player Player, ref int buffIndex)
		{
			Player.maxRunSpeed += 2;
			Player.GetDamage(DamageClass.Generic) += 0.15f;
		}
	}
}