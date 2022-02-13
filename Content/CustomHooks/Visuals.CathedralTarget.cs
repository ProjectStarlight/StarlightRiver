using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
	class CathedralTarget : HookGroup
    {
        //Creates a RenderTarget for the cathedral water. Nothing unsafe.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On.Terraria.Main.CheckMonoliths += WaterTarget;
        }

        public override void Unload()
        {
            CatherdalWaterTarget = null;
        }

        public static RenderTarget2D CatherdalWaterTarget;

        private void WaterTarget(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            orig();

            if (Main.dedServ)
                return;

            var graphics = Main.graphics.GraphicsDevice;

            if (CatherdalWaterTarget is null || CatherdalWaterTarget.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
                CatherdalWaterTarget = new RenderTarget2D(graphics, Main.screenWidth, Main.screenHeight, default, default, default, default, RenderTargetUsage.PreserveContents);

            graphics.SetRenderTarget(CatherdalWaterTarget);

            graphics.Clear(Color.Transparent);
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.End();

            graphics.SetRenderTarget(null);
        }
    }
}