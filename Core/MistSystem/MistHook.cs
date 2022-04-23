using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using Terraria;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core;

namespace StarlightRiver.Core.MistSystem
{
    public class MistHook : IOrderedLoadable
    {
        public float Priority => 1.1f;

        public static MistFieldHost MFH;
        private int MaxMistFields => 1;

        public void Load()
        {
            On.Terraria.Main.DrawDust += DrawMist;
            Main.OnPreDraw += Main_OnPreDraw;
            MFH = new MistFieldHost();
        }

        public void Unload()
        {
            On.Terraria.Main.DrawDust -= DrawMist;
            Main.OnPreDraw -= Main_OnPreDraw;

            MFH = null;
        }
        private void Main_OnPreDraw(GameTime obj)
        {
            MFH?.Update();
        }

        private void DrawMist(On.Terraria.Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            MFH?.Draw(Main.spriteBatch);

        }
    }
}


