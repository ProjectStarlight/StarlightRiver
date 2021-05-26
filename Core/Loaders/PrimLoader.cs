using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;


namespace StarlightRiver.Core.Loaders
{
    class PrimLoader : ILoadable
    {
        public float Priority { get => 1.09f; }

        public void Load()
        {
            StarlightRiver.primitives = new PrimTrailManager();
            StarlightRiver.primitives.LoadContent(Main.graphics.GraphicsDevice);

            //TODO: Move these to wherever the fuck scalie keeps his detours
            On.Terraria.Main.DrawProjectiles += Main_DrawProjectiles;
            Main.OnPreDraw += Main_OnPreDraw;
        }

        private void Main_DrawProjectiles(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
        {
            StarlightRiver.primitives.DrawTarget(Main.spriteBatch);
            orig(self);
        }

        private void Main_OnPreDraw(GameTime obj)
        {
            if (Main.spriteBatch != null && StarlightRiver.primitives != null)
                StarlightRiver.primitives.DrawTrails(Main.spriteBatch, Main.graphics.GraphicsDevice);
        }
        public void Unload()
        {
            On.Terraria.Main.DrawProjectiles -= Main_DrawProjectiles;
            Main.OnPreDraw -= Main_OnPreDraw;
            StarlightRiver.primitives = null;
        }
    }
}
