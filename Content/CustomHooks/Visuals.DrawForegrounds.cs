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
        }

		public void DrawForeground(On.Terraria.Main.orig_DrawInterface orig, Main self, GameTime gameTime)
        {
            Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);//Main.spriteBatch.Begin()

            foreach (var fg in ForegroundLoader.Foregrounds)
            {
                if(fg != null)
                    fg.Render(Main.spriteBatch);
            }

            Main.spriteBatch.End();

            try //I dont know why this is ehre but it was in the old one so im keeping it to be safe
            {
                orig(self, gameTime);
            }
            catch { }
        }

        public static void ResetForeground()
        {
            foreach (var fg in ForegroundLoader.Foregrounds)
            {
                if (fg != null)
                    fg.Reset();
            }
        }
    }
}
