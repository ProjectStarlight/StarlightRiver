using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs
{
    public class RidersPotionBuff : SmartBuff
    {
        public RidersPotionBuff() : base("Rider's Blessing", "Increased critical strike chance while mounted", false) { }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.mount.Active)
            {
                player.thrownCrit += 25;
                player.rangedCrit += 25;
                player.meleeCrit += 25;
                player.magicCrit += 25;
            }
        }
    }
}