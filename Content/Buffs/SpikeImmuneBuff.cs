using Terraria;

namespace StarlightRiver.Content.Buffs
{
	public class SpikeImmuneBuff : SmartBuff
    {
        public SpikeImmuneBuff() : base("Aurora's blessing", "Immunity to spikes\nImproved movement speed", false) { }

        public override void Update(Player Player, ref int buffIndex)
        {
            Player.moveSpeed += 0.5f;
        }
    }
}