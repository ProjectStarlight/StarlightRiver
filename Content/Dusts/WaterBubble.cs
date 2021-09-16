using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
    class WaterBubble : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "SpringBubble";
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 20, 20);
            dust.fadeIn = 10;
            dust.alpha = 200;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White * (dust.alpha / 240f) * (1 - dust.fadeIn / 10f);
        }

        public override bool Update(Dust dust)
        {
            if (dust.fadeIn > 0)
                dust.fadeIn--;

            dust.position += dust.velocity;
            dust.alpha -= 4;

            if (dust.alpha <= 0)
                dust.active = false;

            return false;
        }
    }
}
