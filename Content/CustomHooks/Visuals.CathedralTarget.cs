using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;


using StarlightRiver.Core;
using StarlightRiver.Content.Bosses.SquidBoss;

namespace StarlightRiver.Content.CustomHooks
{
    class CathedralTarget : HookGroup
    {
        //Creates a RenderTarget for the cathedral water. Nothing unsafe.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            Main.OnPreDraw += WaterTarget;
        }

        public override void Unload()
        {
            CatherdalWaterTarget = null;
        }

        public static RenderTarget2D CatherdalWaterTarget;

        private void WaterTarget(GameTime obj)
        {
            var graphics = Main.graphics.GraphicsDevice;
            if (CatherdalWaterTarget is null || CatherdalWaterTarget.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
                CatherdalWaterTarget = new RenderTarget2D(graphics, Main.screenWidth, Main.screenHeight, default, default, default, default, RenderTargetUsage.PreserveContents);

            graphics.SetRenderTarget(CatherdalWaterTarget);

            graphics.Clear(Color.Transparent);
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC npc = Main.npc[k];
                if (npc.active && npc.modNPC is ArenaActor)
                    (npc.modNPC as ArenaActor).DrawWater(Main.spriteBatch);
            }

            Main.spriteBatch.End();

            graphics.SetRenderTarget(null);
        }
    }
}