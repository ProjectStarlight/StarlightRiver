using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Lavas
{
    class VitricLavaStyle : LavaStyle
    {
        public override int ChooseWaterfallStyle()
        {
            return 0;
        }

        public override int GetSplashDust()
        {
            return 0;
        }

        public override int GetDropletGore()
        {
            return 0;
        }

        public override bool ChooseLavaStyle => Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneGlass;

        public override bool SafeAutoload(ref string name, ref string texture, ref string blockTexture)
        {
            texture = "StarlightRiver/Assets/Waters/WaterJungleBloody";
            blockTexture = texture + "_Block";
            return true;
        }

        public override void DrawEffects()
        {
            Main.NewText("Fuck my pussy!");
        }
    }
}
