using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
	public class NanocompositeDust : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.color.R = 227;
            dust.color.G = 255;
            dust.color.B = 254;
            dust.fadeIn = 40;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.rotation += 0.16f;
            dust.color *= 0.94f;
            dust.position += dust.velocity;
            dust.velocity *= 0.98f;

            dust.scale *= 0.95f;
            if (dust.scale < 0.1f)
                dust.active = false;
            return false;
        }
    }
}