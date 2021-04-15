using Microsoft.Xna.Framework;
using Terraria;

using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;

namespace StarlightRiver.Content.CustomHooks
{
    class DrawForegrounds : HookGroup
    {
        //just drawing, nothing to see here.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            On.Terraria.Main.DrawInterface += DrawForeground;
        }

        public void DrawForeground(On.Terraria.Main.orig_DrawInterface orig, Main self, GameTime gameTime)
        {
            Main.spriteBatch.Begin();

            foreach (var fg in ForegroundLoader.Foregrounds)
                fg.Render(Main.spriteBatch);

            Main.spriteBatch.End();

            try //I dont know why this is ehre but it was in the old one so im keeping it to be safe
            {
                orig(self, gameTime);
            }
            catch { }
        }
    }
}
