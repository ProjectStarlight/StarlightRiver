using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Dusts
{
    public class Shadowflame : ModDust
    {
        float startingScale;

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Dust + name;
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            startingScale = dust.scale;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.Purple * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            if (dust.scale <= 0)
                dust.active = false;

            dust.fadeIn = dust.scale / startingScale;
            Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), dust.scale * 0.6f, dust.scale * 0.2f, dust.scale);

            dust.velocity *= 0.9f;
            dust.position += dust.velocity;
            dust.rotation += 0.05f;
            dust.scale -= 0.04f;
            return false;
        }
    }
}
