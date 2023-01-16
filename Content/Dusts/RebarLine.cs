using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Dusts
{
    class RebarLine : ModDust
    {
        public override string Texture => AssetDirectory.VitricBoss + "RoarLine";

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        float Curve(float input) //shrug it works, just a cubic regression for a nice looking curve
		{
            return -2.65f + 19.196f * input - 32.143f * input * input + 15.625f * input * input * input;
		}

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 8, 128);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(4, 64).RotatedBy(dust.velocity.ToRotation() + 1.57f) * dust.scale;
                dust.customData = 1;
            }

            dust.rotation = dust.velocity.ToRotation() + 1.57f;
            dust.position += dust.velocity;
            dust.shader.UseColor(dust.color);
            dust.velocity *= 0.8f;
            dust.fadeIn++;
            dust.scale *= 0.98f;

            if (dust.fadeIn > 20)
                dust.active = false;
            return false;
        }
    }
}
