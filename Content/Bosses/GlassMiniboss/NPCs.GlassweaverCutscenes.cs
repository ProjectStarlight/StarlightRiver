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
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake = 15;

            if (AttackTimer > 38 && AttackTimer < 140)
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 2;

            if (AttackTimer > 210)
            {
                SetPhase(PhaseEnum.DirectPhase);
                ResetAttack();
                AttackPhase = -1;
            }

        }
    }
}
