using System;
using Terraria;
using Terraria.DataStructures;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs
{
    public class Claustrophobia : SmartBuff
    {
        public Claustrophobia() : base("Claustrophobia", "Stuck in wisp form!", true) { }

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
