using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class FireDust : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.color = new Color(255, 255, 200);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity * 0.1f;
            dust.scale *= 0.88f;
            if (dust.scale <= 0.3)
                dust.active = false;
            dust.color *= 0.9f;
            dust.color.G -= 3;
            dust.color.B -= 3;

            float light = 0.001f * dust.scale;
            Lighting.AddLight(dust.position, new Vector3(dust.color.R, dust.color.G, dust.color.B) * light);
            return false;
        }
    }

    public class FireDust2 : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.color = new Color(255, 255, 200);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity * 0.15f;
            dust.scale *= 0.9f;
            if (dust.scale <= 0.1)
                dust.active = false;
            dust.color *= 0.95f;
            dust.color.G -= 1;
            dust.color.B -= 1;

            float light = 0.001f * dust.scale;
            Lighting.AddLight(dust.position, new Vector3(dust.color.R, dust.color.G, dust.color.B) * light);
            return false;
        }
    }
}