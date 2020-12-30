using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class Purify : ModDust
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

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.color *= 0.94f;
            dust.velocity *= 0.94f;
            dust.scale *= 0.94f;
            dust.rotation += dust.velocity.Length() * 0.1f;
            if (dust.scale <= 0.3)
                dust.active = false;

            float light = 0.2f * dust.scale;
            Lighting.AddLight(dust.position, new Vector3(1, 1, 1) * light);

            return false;
        }
    }

    public class Purify2 : ModDust
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

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.color *= 0.98f;
            dust.velocity *= 0.94f;
            dust.position += dust.velocity;
            dust.rotation = dust.velocity.ToRotation();
            dust.scale *= 0.98f;
            if (dust.scale <= 0.2)
                dust.active = false;

            float light = 0.2f * dust.scale;
            Lighting.AddLight(dust.position, new Vector3(1, 1, 1) * light);

            return false;
        }
    }
}