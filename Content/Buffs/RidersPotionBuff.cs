using Terraria;

namespace StarlightRiver.Content.Buffs
{
	public class RidersPotionBuff : SmartBuff
    {
        public RidersPotionBuff() : base("Rider's Blessing", "Increased critical strike chance while mounted", false) { }
        public override void Update(Player Player, ref int buffIndex)
        {
            if (Player.mount.Active)
            {
                Player.thrownCrit += 25;
                Player.rangedCrit += 25;
                Player.meleeCrit += 25;
                Player.magicCrit += 25;
            }
        }
    }
}