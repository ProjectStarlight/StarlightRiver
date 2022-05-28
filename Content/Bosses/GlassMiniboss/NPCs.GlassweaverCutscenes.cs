using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	public partial class Glassweaver : ModNPC
    {
        private void JumpBackAnimation()
        {
            AttackTimer++;

            if (AttackTimer == 40)
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake = 8;

            if (AttackTimer > 38 && AttackTimer < 160)
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 2;

            if (AttackTimer > 410)
            {
                SetPhase(PhaseEnum.DirectPhase);
                ResetAttack();
                NPC.dontTakeDamage = false;
                AttackPhase = -1;
            }

        }
    }
}
