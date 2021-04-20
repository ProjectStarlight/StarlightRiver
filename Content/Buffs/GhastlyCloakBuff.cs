using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs
{
    public class GhastlyCloakBuff : SmartBuff
    {
        public GhastlyCloakBuff() : base("Cloaked", "Increases most stats", false) { }

        public override void Update(Player player, ref int buffIndex)
        {
            player.immuneAlpha += 80;
            player.lifeRegen++;
            player.statDefense += 2;
            player.allDamage += 0.05f;
            player.moveSpeed += 0.4f;
            if (Main.rand.NextBool())
            {
                Dust.NewDust(player.position, player.width, player.height, 62);
            }
        }
    }
}