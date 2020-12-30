using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class Darkness : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.color = Color.White;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor) => dust.color * (dust.alpha / 255f);

        public override bool Update(Dust dust)
        {
            dust.fadeIn++;
            dust.color.G -= 2;
            dust.color.R--;
            dust.alpha = (int)(dust.fadeIn * 17f / 2f - 17 * (float)(Math.Pow(dust.fadeIn, 2) / 240f));
            dust.position += dust.velocity;
            dust.rotation += 0.1f;

            if (dust.fadeIn > 120)
                dust.active = false;

            if (dust.velocity.X != 0)
            {
                dust.fadeIn += 4;
                dust.velocity *= 0.9f;
                dust.scale *= 0.9f;
            }

            return false;
        }
    }
}