using Terraria;

namespace StarlightRiver.Buffs
{
    public class DarkSlow : SmartBuff
    {
        public DarkSlow() : base("Grasping Darkness", "Slowed by shadowy tendrils!", true) { }
        public override void Update(Player player, ref int buffIndex)
        {
            player.velocity.X *= 0.2f;
        }
    }
}