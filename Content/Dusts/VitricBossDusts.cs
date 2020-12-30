using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class VitricBossTell : ModDust
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
            dust.fadeIn = 1;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity * 0.1f;
            dust.rotation += 0.2f;
            dust.color *= 0.98f;
            dust.scale *= 0.988f;
            dust.velocity *= 0.97f;

            if (dust.scale <= 0.4)
                dust.active = false;

            float light = 0.02f * dust.scale;
            Lighting.AddLight(dust.position, new Vector3(1.45f, 2.28f, 2.37f) * light);

            return false;
        }
    }
}