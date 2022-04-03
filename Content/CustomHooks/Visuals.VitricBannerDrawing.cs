using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Physics;
using System;
using Terraria;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.CustomHooks
{
	class VitricBannerDrawing : HookGroup
    {
        //Creates a RenderTarget and then renders the banners in the vitric desert. Should be fairly safe, as its just drawing.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On.Terraria.Main.DrawProjectiles += DrawVerletBanners;
            On.Terraria.Main.SetDisplayMode += RefreshBannerTarget;
            On.Terraria.Main.CheckMonoliths += DrawVerletBannerTarget;
        }

		private void DrawVerletBanners(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
        {
            Filters.Scene["Outline"].GetShader().Shader.Parameters["resolution"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            Filters.Scene["Outline"].GetShader().Shader.Parameters["outlineColor"].SetValue(new Vector3(0, 0, 0));

            Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, Filters.Scene["Outline"].GetShader().Shader, Main.GameViewMatrix.ZoomMatrix);

            VerletChain.DrawStripsPixelated(Main.spriteBatch);

            Main.spriteBatch.End();

            orig(self);
        }

        private void RefreshBannerTarget(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            if (width != Main.screenWidth || height != Main.screenHeight)
                VerletChain.target = Main.dedServ ? null : new RenderTarget2D(Main.instance.GraphicsDevice, width / 2, height / 2, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            orig(width, height, fullscreen);
        }

        private void DrawVerletBannerTarget(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            orig();

            if (Main.gameMenu)
            {
                VerletChain.toDraw.Clear(); // we clear because the toDraw list is static and we need to manually clear when we're not in a world so we don't get ghost freezeframes when rejoining a multiPlayer world (singlePlayer could be cleared on world load potentially)
                return;
            }
                

            GraphicsDevice graphics = Main.instance.GraphicsDevice;

            graphics.SetRenderTarget(VerletChain.target);
            graphics.Clear(Color.Transparent);

            graphics.BlendState = BlendState.Opaque;

            foreach (var i in VerletChain.toDraw)
                i.DrawStrip(i.scale);

            graphics.SetRenderTarget(null);
        }
    }
}