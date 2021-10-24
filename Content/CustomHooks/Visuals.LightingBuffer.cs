using Microsoft.Xna.Framework;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
	class LightingBuffer : HookGroup
    {
        //Creates a RenderTarget for the lighting buffer. Could potentially be performance havy but shouldn't be dangerous.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On.Terraria.Main.CheckMonoliths += LightingTarget;
            On.Terraria.Main.SetDisplayMode += RefreshLightingTarget;
        }

		private void RefreshLightingTarget(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            if (width != Main.screenWidth || height != Main.screenHeight)
                StarlightRiver.LightingBufferInstance.ResizeBuffers(width, height);

            orig(width, height, fullscreen);
        }

        private void LightingTarget(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            orig();

            if (Main.dedServ)
                return;

            if (!Main.gameMenu) 
                StarlightRiver.LightingBufferInstance.DebugDraw();

            Main.instance.GraphicsDevice.SetRenderTarget(null);
        }
    }
}