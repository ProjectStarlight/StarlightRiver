using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	class MoonstoneShimmer : ModDust
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
            dust.fadeIn = LifeTime;
            dust.frame = new Rectangle(0, 0, 13, 25);
        }

        const int LifeTime = 120;
        const float SinTime = LifeTime / (float)Math.PI;
        const float ScaleMultiplier = 0.01f;

        public override Color? GetAlpha(Dust dust, Color lightColor) =>
            dust.color * (float)Math.Sin(dust.fadeIn / SinTime) * 1.5f;

        public override bool Update(Dust dust)
        {
            Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale);

            dust.scale *= (-(float)Math.Sin(dust.fadeIn / (SinTime * 0.5f))  * ScaleMultiplier) + 1;
            dust.fadeIn--;
            dust.position += dust.velocity;

            if (dust.fadeIn < 0) 
                dust.active = false;
            return false;
        }
    }
}
