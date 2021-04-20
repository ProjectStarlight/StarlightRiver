using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs
{
    public class SpikeImmuneBuff : SmartBuff
    {
        public SpikeImmuneBuff() : base("Aurora's blessing", "Immunity to spikes\nImproved movement speed", false) { }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed += 0.5f;
        }
    }
}