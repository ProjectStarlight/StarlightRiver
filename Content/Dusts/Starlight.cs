using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class Starlight : ModDust
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
            dust.color = new Color(185, 228, 237);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity * 0.1f;
            dust.color *= 0.982f;
            dust.scale *= 0.982f;
            dust.velocity *= 0.97f;
            if (dust.scale <= 0.2)
                dust.active = false;

            float light = 0.1f * dust.scale;
            Lighting.AddLight(dust.position, new Vector3(1.45f, 2.28f, 2.37f) * light);
            return false;
        }
    }

    public class StarlightSmoke : Starlight
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + "Starlight";
            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * (dust.alpha / 255f);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity * 0.1f;
            dust.color *= 0.982f;
            dust.scale *= 0.982f;
            dust.velocity *= 0.97f;
            if (dust.scale <= 0.2)
                dust.active = false;

            float light = 0.1f * dust.scale;
            Lighting.AddLight(dust.position, new Vector3(1.45f, 2.28f, 2.37f) * light);
            return false;
        }
    }
}