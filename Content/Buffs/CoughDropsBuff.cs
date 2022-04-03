using StarlightRiver.Helpers;
using Terraria;

namespace StarlightRiver.Content.Buffs
{
	public class CoughDropsBuff : SmartBuff
    {
        public CoughDropsBuff() : base("Cough Drops", "Your speed and damage are boosted", false) { }

        public override void Update(Player Player, ref int buffIndex)
        {
            Player.maxRunSpeed += 2;
            Player.BoostAllDamage(0.15f);
        }
    }
}
