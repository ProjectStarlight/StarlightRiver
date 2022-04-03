using Terraria;

namespace StarlightRiver.Content.Buffs
{
	public class DarkSlow : SmartBuff
    {
        public DarkSlow() : base("Grasping Darkness", "Slowed by shadowy tendrils!", true) { }

        public override void Update(Player Player, ref int buffIndex)
        {
            Player.velocity.X *= 0.2f;
        }
    }
}