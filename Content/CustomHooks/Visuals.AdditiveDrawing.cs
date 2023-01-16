using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
	class AdditiveDrawing : HookGroup
    {
        //Just draw calls and a SB reset, nothing dangerous.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On.Terraria.Main.DrawDust += DrawAdditive;
        }

        private void DrawAdditive(On.Terraria.Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            for (int k = 0; k < Main.maxProjectiles; k++) //Projectiles
                if (Main.projectile[k].active && Main.projectile[k].ModProjectile is IDrawAdditive)
                    (Main.projectile[k].ModProjectile as IDrawAdditive).DrawAdditive(Main.spriteBatch);

            for (int k = 0; k < Main.maxNPCs; k++) //NPCs
                if (Main.npc[k].active && Main.npc[k].ModNPC is IDrawAdditive)
                    (Main.npc[k].ModNPC as IDrawAdditive).DrawAdditive(Main.spriteBatch);

            Main.spriteBatch.End();
        }
    }
}