using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	class RoarLine : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricBoss + "RoarLine";
            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            var curveOut = Curve(1 - dust.fadeIn / 30f);
            var color = Color.Lerp(dust.color, new Color(255, 100, 0), dust.fadeIn / 30f);
            dust.color = color * (curveOut + 0.4f);
            return dust.color;
        }

        float Curve(float input) //shrug it works, just a cubic regression for a nice looking curve
		{
            return -2.65f + 19.196f * input - 32.143f * input * input + 15.625f * input * input * input;
		}

        public override void OnSpawn(Dust dust)
        {
            dust.color = Color.Transparent;
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 8, 128);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.color == Color.Transparent)
                dust.position -= new Vector2(4, 32) * dust.scale;

            dust.rotation = dust.velocity.ToRotation() + 1.57f;
            dust.position += dust.velocity;
            dust.shader.UseColor(dust.color);
            dust.fadeIn++;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

            if (dust.fadeIn > 30)
                dust.active = false;
            return false;
        }
    }
}
