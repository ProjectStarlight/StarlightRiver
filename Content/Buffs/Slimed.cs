using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs
{
    public class Slimed : SmartBuff
    {
        public Slimed() : base("Slimed", "eww", true) { }
        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen -= 5;
            player.slow = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen -= 5;//life per second
            npc.velocity *= npc.noGravity ? 0.96f : 0.92f;//4% & 8%
        }
    }
}
