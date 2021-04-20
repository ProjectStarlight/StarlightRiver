using Terraria;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Buffs
{
    public class CoughDropsBuff : SmartBuff
    {
        public CoughDropsBuff() : base("Cough Drops", "Your speed and damage are boosted", false) { }

        public override void Update(Player player, ref int buffIndex)
        {
            player.maxRunSpeed += 2;
            player.BoostAllDamage(0.15f);
        }
    }
}
