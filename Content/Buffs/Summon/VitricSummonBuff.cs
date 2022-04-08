using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Buffs.Summon
{
	class VitricSummonBuff : SmartBuff
    {
        public override string Texture => AssetDirectory.Debug;

        public VitricSummonBuff() : base("Glassweaver's Arsonal", "Strike your foes with glass-forged weapons!", false, true) { }

        public override void Update(Player Player, ref int buffIndex)
        {
            if (Player.ownedProjectileCounts[ModContent.ProjectileType<VitricSummonOrb>()] > 0)
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
