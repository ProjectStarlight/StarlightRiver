using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs.Summon
{
    class TentacleSummonBuff : SmartBuff
    {
        public TentacleSummonBuff() : base("Tentacles", "Now you'll be able to get that damn spiderman for sure!", false, true) { }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[mod.ProjectileType("TentacleSummonHead")] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
