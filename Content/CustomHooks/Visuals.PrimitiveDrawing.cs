using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
	public class PrimitiveDrawing : HookGroup
    {
        // Should not interfere with anything.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On.Terraria.Main.DrawDust += DrawPrimitives;
        }

		private void DrawPrimitives(On.Terraria.Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (Main.gameMenu)
                return;

            for (int k = 0; k < Main.maxProjectiles; k++) // Projectiles.
                if (Main.projectile[k].active && Main.projectile[k].modProjectile is IDrawPrimitive)
                    (Main.projectile[k].modProjectile as IDrawPrimitive).DrawPrimitives();

			for (int k = 0; k < Main.maxNPCs; k++) // NPCs.
				if (Main.npc[k].active && Main.npc[k].modNPC is IDrawPrimitive)
					(Main.npc[k].modNPC as IDrawPrimitive).DrawPrimitives();
		}
    }
}
