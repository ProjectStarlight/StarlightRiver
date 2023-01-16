using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
	class DrawForegrounds : HookGroup
    {
        //just drawing, nothing to see here.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On.Terraria.Main.DrawInterface += DrawForeground;
            On.Terraria.Main.DoUpdate += ResetForeground;
        }

        public void DrawForeground(On.Terraria.Main.orig_DrawInterface orig, Main self, GameTime gameTime)
        {
            Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);//Main.spriteBatch.Begin()

            foreach (var fg in ForegroundLoader.Foregrounds) //TODO: Perhaps create some sort of ActiveForeground list later? especially since we iterate twice for the over UI ones
            {
                if (fg != null && !fg.OverUI)
                    fg.Render(Main.spriteBatch);
            }

            Main.spriteBatch.End();

            orig(self, gameTime);

            Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);

            foreach (var fg in ForegroundLoader.Foregrounds)
            {
                if (fg != null && fg.OverUI)
                    fg.Render(Main.spriteBatch);
            }

            Main.spriteBatch.End();
        }

        private void ResetForeground(On.Terraria.Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
        {
            if (Main.gameMenu)
            {
                orig(self, ref gameTime);
                return;
            }
            
            foreach (var fg in ForegroundLoader.Foregrounds)
            {
                if (fg != null)
                    fg.Reset();
            }

            orig(self, ref gameTime);
        }
    }
}
