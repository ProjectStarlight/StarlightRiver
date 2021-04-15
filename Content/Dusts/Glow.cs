using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Dusts
{
    public class Glow : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Keys/GlowSoft";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            if(dust.customData is null)
			{
                dust.position -= Vector2.One * 32 * dust.scale;
                dust.customData = true;
            }

            dust.shader.UseColor(dust.color);

            dust.position += dust.velocity;

            dust.velocity *= 0.99f;
            dust.color *= 0.95f;
            dust.scale *= 0.95f;

            Lighting.AddLight(dust.position, dust.color.ToVector3());

            if (dust.scale < 0.05f)
                dust.active = false;

            return false;
        }
    }
}