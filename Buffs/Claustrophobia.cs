using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Buffs
{
    public class Claustrophobia : SmartBuff
    {
        public Claustrophobia() : base("Claustrophobia", "Stuck in wisp form!", true)
        {
        }

        private int timer;

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.whoAmI == Main.myPlayer && timer++ % 15 == 0)
            {
                var dps = player.statLifeMax2 / 10;
                var dmg = dps / 4;
                player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " couldn't maintain their form."), dmg, 0);
                player.immune = false;
            }

            player.lifeRegen = Math.Min(player.lifeRegen, 0);
            player.lifeRegenTime = 0;
        }
    }
}
