using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
    class VitricBannerDrawing : HookGroup
    {
        //Creates a RenderTarget and then renders the banners in the vitric desert. Should be fairly safe, as its just drawing.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            On.Terraria.Main.DrawProjectiles += DrawVerletBanners;
            Main.OnPreDraw += DrawVerletBannerTarget;
        }

        public void DrawVerletBannerTarget(GameTime obj)
        {
            GraphicsDevice graphics = Main.instance.GraphicsDevice;

            graphics.SetRenderTarget(VerletChainInstance.target);
            graphics.Clear(Color.Transparent);

            foreach (var i in VerletChainInstance.toDraw)
                i.DrawStrip();
        }

        private void DrawVerletBanners(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
        {
            Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            VerletChainInstance.DrawStripsPixelated(Main.spriteBatch);
            Main.spriteBatch.End();
            orig(self);
        }
    }
}