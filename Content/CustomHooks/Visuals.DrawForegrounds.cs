using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

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

            foreach (var fg in StarlightRiver.Instance.foregrounds)
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
