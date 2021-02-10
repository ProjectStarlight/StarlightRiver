using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class BlueStamina : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.3f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 0.5f;
            dust.color.R = 107;
            dust.color.G = 172;
            dust.color.B = 255;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += 0.15f;
            dust.velocity *= 0.95f;
            dust.color *= 0.98f;

            dust.scale *= 0.98f;
            if (dust.scale < 0.1f || dust.color == Color.Transparent)
                dust.active = false;
            return false;
        }
    }
}
