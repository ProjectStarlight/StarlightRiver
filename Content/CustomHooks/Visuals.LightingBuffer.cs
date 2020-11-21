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
    class LightingBuffer : HookGroup
    {
        //Creates a RenderTarget for the lighting buffer. Could potentially be performance havy but shouldn't be dangerous.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            Main.OnPreDraw += LightingTarget;
        }

        private void LightingTarget(GameTime obj)
        {
            if (Main.dedServ) return;
            if (!Main.gameMenu) StarlightRiver.lightingTest.DebugDraw();

            GraphicsDevice graphics = Main.instance.GraphicsDevice;

            //TODO: move all below


            Main.spriteBatch.End();

            graphics.SetRenderTarget(null);
        }
    }
}