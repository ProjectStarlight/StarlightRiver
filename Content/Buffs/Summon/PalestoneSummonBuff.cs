using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Buffs.Summon
{
	class PalestoneSummonBuff : SmartBuff
    {
        public PalestoneSummonBuff() : base("Little lad", "His goal: to kill.", false, true) { }

        public override void Update(Player Player, ref int buffIndex)
        {
            if (Player.ownedProjectileCounts[ModContent.ProjectileType<Items.Palestone.PaleKnight>()] > 0)
            {
                Player.buffTime[buffIndex] = 18000;
            }
            else
            {
                Player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
