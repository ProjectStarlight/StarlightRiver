using Microsoft.Xna.Framework.Graphics;
using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.CustomHooks
{
    class AdditiveDrawing : HookGroup
    {
        //Just draw calls and a SB reset, nothing dangerous.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            On.Terraria.Main.DrawDust += DrawAdditive;
        }

        private void DrawAdditive(On.Terraria.Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            for (int k = 0; k < Main.maxProjectiles; k++) //projectiles
                if (Main.projectile[k].active && Main.projectile[k].modProjectile is IDrawAdditive)
                    (Main.projectile[k].modProjectile as IDrawAdditive).DrawAdditive(Main.spriteBatch);

            for (int k = 0; k < Main.maxNPCs; k++) //NPCs
                if (Main.npc[k].active && Main.npc[k].modNPC is IDrawAdditive)
                    (Main.npc[k].modNPC as IDrawAdditive).DrawAdditive(Main.spriteBatch);

            Main.spriteBatch.End();
        }
    }
}