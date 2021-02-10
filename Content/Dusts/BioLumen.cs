using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    internal class BioLumen : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.position.Y += (float)Math.Sin(StarlightWorld.rottime + dust.fadeIn) * 0.3f;
            dust.position += dust.velocity;
            dust.scale *= 0.994f;
            //Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale);
            if (dust.scale <= 0.2f) dust.active = false;
            return false;
        }
    }
}