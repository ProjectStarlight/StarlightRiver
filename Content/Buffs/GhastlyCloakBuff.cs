using Terraria;

namespace StarlightRiver.Content.Buffs
{
	public class GhastlyCloakBuff : SmartBuff
    {
        public GhastlyCloakBuff() : base("Cloaked", "Increases most stats", false) { }

        public override void Update(Player Player, ref int buffIndex)
        {
            Player.immuneAlpha += 80;
            Player.lifeRegen++;
            Player.statDefense += 2;
            Player.allDamage += 0.05f;
            Player.moveSpeed += 0.4f;
            if (Main.rand.NextBool())
            {
                Dust.NewDust(Player.position, Player.width, Player.height, 62);
            }
        }
    }
}