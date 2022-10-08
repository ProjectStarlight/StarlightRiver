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
                Core.Systems.CameraSystem.Shake = 8;

            if (AttackTimer > 38 && AttackTimer < 160)
                Core.Systems.CameraSystem.Shake += 2;

            if (AttackTimer > 410)
            {
                Phase = (int)Phases.DirectPhase;
                ResetAttack();
                NPC.dontTakeDamage = false;
                AttackPhase = -1;
            }

        }
    }
}
