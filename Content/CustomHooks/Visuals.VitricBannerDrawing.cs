using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Physics;
using Terraria;

using StarlightRiver.Core;
using System;

namespace StarlightRiver.Content.CustomHooks
{
    class VitricBannerDrawing : HookGroup
    {
        //Creates a RenderTarget and then renders the banners in the vitric desert. Should be fairly safe, as its just drawing.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            On.Terraria.Main.DrawProjectiles += DrawVerletBanners;
            On.Terraria.Main.SetDisplayMode += RefreshBannerTarget;
            On.Terraria.Main.Draw += DrawVerletBannerTarget;
        }

		private void DrawVerletBanners(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
        {
            Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            VerletChainInstance.DrawStripsPixelated(Main.spriteBatch);
            Main.spriteBatch.End();
            orig(self);
        }

        private void RefreshBannerTarget(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            if (width != Main.screenWidth || height != Main.screenHeight)
                VerletChainInstance.target = Main.dedServ ? null : new RenderTarget2D(Main.instance.GraphicsDevice, width / 2, height / 2, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            orig(width, height, fullscreen);
        }

        private void DrawVerletBannerTarget(On.Terraria.Main.orig_Draw orig, Main self, GameTime gameTime)
        {
            GraphicsDevice graphics = self.GraphicsDevice;

            graphics.SetRenderTarget(VerletChainInstance.target);
            graphics.Clear(Color.Transparent);

            foreach (var i in VerletChainInstance.toDraw)
                i.DrawStrip(i.scale);

            graphics.SetRenderTarget(null);

            orig(self, gameTime);
        }
    }
}