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

            Main.OnPreDraw += LightingTarget;
            On.Terraria.Main.SetDisplayMode += RefreshLightingTarget;
        }

        public override void Unload()
        {
            Main.OnPreDraw -= LightingTarget;
        }

        private void RefreshLightingTarget(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            if (!Main.gameInactive && width != Main.screenWidth || height != Main.screenHeight)
                StarlightRiver.LightingBufferInstance.ResizeBuffers(width, height);

            orig(width, height, fullscreen);
        }

        private void LightingTarget(GameTime obj)
        {
            if (Main.dedServ)
                return;

            if (!Main.gameMenu)
                StarlightRiver.LightingBufferInstance.DebugDraw();

            Main.instance.GraphicsDevice.SetRenderTarget(null);
        }
    }
}