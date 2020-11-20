using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Dusts
{
    public class Ink : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Dusts/Stamina";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.scale;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.scale *= 0.982f;
            dust.velocity.Y += 0.22f;

            if (dust.scale <= 0.2)
                dust.active = false;

            float light = 0.1f * dust.scale;
            Lighting.AddLight(dust.position, dust.color.ToVector3() * light);
            return false;
        }
    }
}