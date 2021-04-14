using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.CustomHooks
{
    class LightingBuffer : HookGroup
    {
        //Creates a RenderTarget for the lighting buffer. Could potentially be performance havy but shouldn't be dangerous.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            Main.OnPreDraw += LightingTarget;
            On.Terraria.Main.SetDisplayMode += RefreshLightingTarget;
        }

        private void RefreshLightingTarget(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            if (width != Main.screenWidth || height != Main.screenHeight)
                StarlightRiver.lightingTest.ResizeBuffers();

            orig(width, height, fullscreen);
        }

        private void LightingTarget(GameTime obj)
        {
            if (Main.dedServ)
                return;

            if (!Main.gameMenu) 
                StarlightRiver.lightingTest.DebugDraw();

            Main.instance.GraphicsDevice.SetRenderTarget(null);
        }
    }
}