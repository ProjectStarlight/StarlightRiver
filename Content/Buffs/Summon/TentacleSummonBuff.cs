using Terraria;

namespace StarlightRiver.Content.Buffs.Summon
{
	class TentacleSummonBuff : SmartBuff
    {
        public TentacleSummonBuff() : base("Tentacles", "Now you'll be able to get that damn spiderman for sure!", false, true) { }

        public override void Update(Player Player, ref int buffIndex)
        {
            if (Player.ownedProjectileCounts[Mod.ProjectileType("TentacleSummonHead")] > 0)
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
