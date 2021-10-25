/*using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Buffs.Summon
{
	class PalestoneSummonBuff : SmartBuff
    {
        public PalestoneSummonBuff() : base("Little lad", "His goal: to kill.", false, true) { }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Items.Palestone.PaleKnight>()] > 0)
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
}*/
