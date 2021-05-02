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
        public override int ChooseWaterfallStyle() => 0;

        public override int GetSplashDust() => 0;

        public override int GetDropletGore() => 0;

        public override bool ChooseLavaStyle()
        {
            BiomeHandler modPlayer = Main.LocalPlayer.GetModPlayer<BiomeHandler>();
            return modPlayer.ZoneGlass || modPlayer.FountainVitric;
        }

        public override bool SafeAutoload(ref string name, ref string texture, ref string blockTexture)
        {
            texture = "StarlightRiver/Assets/Waters/WaterJungleBloody";
            blockTexture = texture + "_Block";
            return true;
        }

        public override bool DrawEffects(int x, int y)
        {
            var tile = Framing.GetTileSafely(x, y - 1);
            if (Main.rand.Next(150) == 0 && tile.liquidType() == 1)
                Dust.NewDustPerfect(new Microsoft.Xna.Framework.Vector2(x, y) * 16, ModContent.DustType<Dusts.LavaBubble>(), -Microsoft.Xna.Framework.Vector2.UnitY, 0, default, Main.rand.NextFloat(0.6f, 0.7f));
            return true;
        }
    }
}
